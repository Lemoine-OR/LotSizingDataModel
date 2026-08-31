using LotSizingDataModel.Solver.Modeling;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

namespace LotSizingDataModel.Solver.Algorithms.Metaheuristics;

public sealed class MathematicalModelConstrainedMetaheuristicProblem :
    IContinuousConstrainedOptimizationProblem
{
    private readonly MathematicalModel _model;
    private readonly MathematicalModelMetaheuristicEncoding _encoding;
    private readonly MathematicalModelCandidateEvaluator _evaluator;
    private readonly LinearConstraint[] _inequalities;
    private readonly LinearConstraint[] _equalities;

    public MathematicalModelConstrainedMetaheuristicProblem(
        MathematicalModel model,
        MathematicalModelMetaheuristicEncoding encoding,
        double equalityTolerance = 1.0e-6)
    {
        ArgumentNullException.ThrowIfNull(
            model);

        ArgumentNullException.ThrowIfNull(
            encoding);

        if (!double.IsFinite(
                equalityTolerance) ||
            equalityTolerance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(equalityTolerance));
        }

        model.EnsureValid();

        _model =
            model;

        _encoding =
            encoding;

        _evaluator =
            new MathematicalModelCandidateEvaluator(
                model);

        if (_encoding.ModelDimension !=
            _evaluator.Dimension)
        {
            throw new InvalidOperationException(
                "Metaheuristic encoding and mathematical model dimensions differ.");
        }

        _inequalities =
            model.Constraints
                .Where(
                    constraint =>
                        constraint.IsEnabled &&
                        constraint.Sense is
                            MathematicalConstraintSense.LessThanOrEqual or
                            MathematicalConstraintSense.GreaterThanOrEqual)
                .OrderBy(
                    constraint =>
                        constraint.Id)
                .ToArray();

        _equalities =
            model.Constraints
                .Where(
                    constraint =>
                        constraint.IsEnabled &&
                        constraint.Sense ==
                            MathematicalConstraintSense.Equal)
                .OrderBy(
                    constraint =>
                        constraint.Id)
                .ToArray();

        EqualityTolerance =
            equalityTolerance;
    }

    public IBoundedContinuousSearchSpace SearchSpace =>
        _encoding.SearchSpace;

    public OptimizationSense Sense =>
        _model.Objective.Sense switch
        {
            ObjectiveSense.Minimize =>
                OptimizationSense.Minimize,

            ObjectiveSense.Maximize =>
                OptimizationSense.Maximize,

            _ =>
                throw new NotSupportedException(
                    $"Objective sense '{_model.Objective.Sense}' is not supported by the MetaheuristicsPlatform bridge.")
        };

    public int InequalityCount =>
        _inequalities.Length;

    public int EqualityCount =>
        _equalities.Length;

    public double EqualityTolerance
    {
        get;
    }

    public double EvaluateObjective(
        ReadOnlySpan<double> solution)
    {
        double[] decoded =
            _encoding.Decode(
                solution);

        return _evaluator.EvaluateObjective(
            decoded);
    }

    public void EvaluateConstraints(
        ReadOnlySpan<double> solution,
        Span<double> inequalities,
        Span<double> equalities)
    {
        if (inequalities.Length !=
                InequalityCount ||
            equalities.Length !=
                EqualityCount)
        {
            throw new ArgumentException(
                "Constraint output dimensions do not match the mathematical model.");
        }

        double[] decoded =
            _encoding.Decode(
                solution);

        for (int index = 0;
             index < _inequalities.Length;
             index++)
        {
            inequalities[index] =
                _evaluator.EvaluateCanonicalConstraintResidual(
                    _inequalities[index],
                    decoded);
        }

        for (int index = 0;
             index < _equalities.Length;
             index++)
        {
            equalities[index] =
                _evaluator.EvaluateCanonicalConstraintResidual(
                    _equalities[index],
                    decoded);
        }
    }

    public double[] DecodeCandidate(
        ReadOnlySpan<double> solution)
    {
        return _encoding.Decode(
            solution);
    }

    public bool IsDecodedCandidateFeasible(
        ReadOnlySpan<double> solution)
    {
        double[] decoded =
            DecodeCandidate(
                solution);

        return _evaluator.IsFeasible(
            decoded,
            EqualityTolerance);
    }
}
