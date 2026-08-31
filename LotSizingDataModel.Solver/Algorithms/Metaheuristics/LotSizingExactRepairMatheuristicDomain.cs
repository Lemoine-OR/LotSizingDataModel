using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Modeling;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Matheuristics;
using MetaheuristicsPlatform.Random;

namespace LotSizingDataModel.Solver.Algorithms.Metaheuristics;

public sealed class LotSizingExactRepairMatheuristicDomain :
    IExactRepairMatheuristicDomain
{
    private readonly MathematicalModel _sourceModel;
    private readonly MathematicalVariable[] _variables;
    private readonly MathematicalModelCandidateEvaluator _evaluator;
    private readonly MetaheuristicsPlatformExactRepairSubproblemBuilder _builder;
    private readonly MatheuristicModelSolveDelegate _exactSolver;
    private readonly MatheuristicModelSolveDelegate _relaxationSolver;
    private readonly MatheuristicPoint _initialPoint;
    private readonly MatheuristicVariableKind[] _variableKinds;
    private readonly double _feasibilityTolerance;

    public LotSizingExactRepairMatheuristicDomain(
        MathematicalModel sourceModel,
        IReadOnlyDictionary<int, double> initialValuesByVariableId,
        MatheuristicModelSolveDelegate exactSolver,
        MatheuristicModelSolveDelegate relaxationSolver,
        double feasibilityTolerance = 1.0e-6)
    {
        ArgumentNullException.ThrowIfNull(
            sourceModel);

        ArgumentNullException.ThrowIfNull(
            initialValuesByVariableId);

        _exactSolver =
            exactSolver ??
            throw new ArgumentNullException(
                nameof(exactSolver));

        _relaxationSolver =
            relaxationSolver ??
            throw new ArgumentNullException(
                nameof(relaxationSolver));

        if (!double.IsFinite(
                feasibilityTolerance) ||
            feasibilityTolerance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(feasibilityTolerance));
        }

        sourceModel.EnsureValid();

        _sourceModel =
            sourceModel;

        _variables =
            sourceModel.Variables
                .OrderBy(
                    variable =>
                        variable.Id)
                .ToArray();

        _variableKinds =
            _variables
                .Select(
                    GetVariableKind)
                .ToArray();

        _evaluator =
            new MathematicalModelCandidateEvaluator(
                sourceModel);

        _builder =
            new MetaheuristicsPlatformExactRepairSubproblemBuilder();

        _feasibilityTolerance =
            feasibilityTolerance;

        double[] initialValues =
            CreateCompleteValues(
                initialValuesByVariableId);

        if (!_evaluator.IsFeasible(
                initialValues,
                feasibilityTolerance))
        {
            throw new InvalidOperationException(
                "The exact-repair initial point must be feasible for the source mathematical model.");
        }

        _initialPoint =
            new MatheuristicPoint(
                initialValues,
                _evaluator.EvaluateObjective(
                    initialValues),
                isIntegerFeasible:
                    true);
    }

    public OptimizationSense Sense =>
        _sourceModel.Objective.Sense switch
        {
            ObjectiveSense.Minimize =>
                OptimizationSense.Minimize,

            ObjectiveSense.Maximize =>
                OptimizationSense.Maximize,

            _ =>
                throw new NotSupportedException(
                    $"Objective sense '{_sourceModel.Objective.Sense}' is not supported by the exact-repair bridge.")
        };

    public IReadOnlyList<MatheuristicVariableKind>
        VariableKinds =>
            _variableKinds;

    public MatheuristicPoint CreateInitial(
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(
            random);

        return new MatheuristicPoint(
            _initialPoint.Values,
            _initialPoint.Objective,
            _initialPoint.IsIntegerFeasible,
            _initialPoint.ReducedCosts);
    }

    public double Evaluate(
        IReadOnlyList<double> values)
    {
        return _evaluator.EvaluateObjective(
            values);
    }

    public bool IsIntegerFeasible(
        IReadOnlyList<double> values)
    {
        return _evaluator.IsIntegerFeasible(
            values,
            _feasibilityTolerance);
    }

    public MatheuristicSolveResult SolveRelaxation(
        ExactRepairRequest request,
        CancellationToken cancellationToken)
    {
        MathematicalModel subproblem =
            _builder.BuildRelaxation(
                _sourceModel,
                request);

        return SolveSubproblem(
            subproblem,
            request.NodeLimit,
            _relaxationSolver,
            requireIntegerFeasible:
                false,
            cancellationToken);
    }

    public MatheuristicSolveResult SolveExact(
        ExactRepairRequest request,
        CancellationToken cancellationToken)
    {
        MathematicalModel subproblem =
            _builder.BuildExact(
                _sourceModel,
                request);

        return SolveSubproblem(
            subproblem,
            request.NodeLimit,
            _exactSolver,
            requireIntegerFeasible:
                true,
            cancellationToken);
    }

    private MatheuristicSolveResult SolveSubproblem(
        MathematicalModel subproblem,
        int nodeLimit,
        MatheuristicModelSolveDelegate solver,
        bool requireIntegerFeasible,
        CancellationToken cancellationToken)
    {
        MathematicalModelSolveResult solveResult =
            solver(
                subproblem,
                nodeLimit,
                cancellationToken);

        ArgumentNullException.ThrowIfNull(
            solveResult);

        if (!solveResult.HasFeasibleSolution)
        {
            return MatheuristicSolveResult.NoSolution(
                exploredNodes:
                    ConvertExploredNodes(
                        solveResult.ExploredNodeCount));
        }

        var subproblemEvaluator =
            new MathematicalModelCandidateEvaluator(
                subproblem);

        double[] values =
            subproblemEvaluator.ExtractCompleteValues(
                solveResult);

        if (!subproblemEvaluator.IsConstraintFeasible(
                values,
                _feasibilityTolerance))
        {
            throw new InvalidOperationException(
                "Exact-repair solve delegate returned a point infeasible for its generated subproblem.");
        }

        bool integerFeasible =
            _evaluator.IsIntegerFeasible(
                values,
                _feasibilityTolerance);

        if (requireIntegerFeasible &&
            !integerFeasible)
        {
            throw new InvalidOperationException(
                "Exact-repair solve delegate returned a non-integral exact point.");
        }

        var point =
            new MatheuristicPoint(
                values,
                _evaluator.EvaluateObjective(
                    values),
                integerFeasible);

        return MatheuristicSolveResult.FromPoint(
            point,
            exploredNodes:
                ConvertExploredNodes(
                    solveResult.ExploredNodeCount));
    }

    private double[] CreateCompleteValues(
        IReadOnlyDictionary<int, double> valuesByVariableId)
    {
        var values =
            new double[_variables.Length];

        for (int index = 0;
             index < _variables.Length;
             index++)
        {
            MathematicalVariable variable =
                _variables[index];

            if (!valuesByVariableId.TryGetValue(
                    variable.Id,
                    out double value))
            {
                throw new InvalidOperationException(
                    $"Initial point is missing variable identifier '{variable.Id}'.");
            }

            if (!double.IsFinite(
                    value))
            {
                throw new InvalidOperationException(
                    $"Initial value for variable '{variable.Name}' is not finite.");
            }

            values[index] =
                value;
        }

        return values;
    }

    private static MatheuristicVariableKind
        GetVariableKind(
            MathematicalVariable variable)
    {
        return variable.VariableType switch
        {
            MathematicalVariableType.Continuous =>
                MatheuristicVariableKind.Continuous,

            MathematicalVariableType.Integer =>
                MatheuristicVariableKind.Integer,

            MathematicalVariableType.Binary =>
                MatheuristicVariableKind.Binary,

            _ =>
                throw new NotSupportedException(
                    $"Variable type '{variable.VariableType}' is outside the alpha.37 exact-repair bridge.")
        };
    }

    private static int ConvertExploredNodes(
        long? exploredNodes)
    {
        if (!exploredNodes.HasValue)
        {
            return 0;
        }

        if (exploredNodes.Value <
                0 ||
            exploredNodes.Value >
                int.MaxValue)
        {
            throw new InvalidOperationException(
                "Exact-repair explored-node count is outside the supported range.");
        }

        return (int)exploredNodes.Value;
    }
}
