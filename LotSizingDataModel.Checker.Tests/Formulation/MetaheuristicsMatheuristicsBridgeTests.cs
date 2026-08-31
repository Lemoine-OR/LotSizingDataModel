using LotSizingDataModel.Solver.Algorithms.Metaheuristics;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Modeling;
using Xunit;

namespace LotSizingDataModel.Checker.Tests.Formulation;

public sealed class MetaheuristicsMatheuristicsBridgeTests
{
    [Fact]
    public void MixedEncoding_RoundsDiscreteVariablesDeterministically()
    {
        MathematicalModel model =
            CreateToyModel();

        var encoding =
            new MathematicalModelMetaheuristicEncoding(
                model);

        double[] decoded =
            encoding.Decode(
                new[]
                {
                    0.8,
                    1.6
                });

        Assert.Equal(
            1.0,
            decoded[0],
            10);

        Assert.Equal(
            1.6,
            decoded[1],
            10);
    }

    [Fact]
    public void ConstraintBridge_UsesNonPositiveInequalityConvention()
    {
        MathematicalModel model =
            CreateToyModel();

        var encoding =
            new MathematicalModelMetaheuristicEncoding(
                model);

        var problem =
            new MathematicalModelConstrainedMetaheuristicProblem(
                model,
                encoding);

        var inequalities =
            new double[problem.InequalityCount];

        var equalities =
            new double[problem.EqualityCount];

        problem.EvaluateConstraints(
            new[]
            {
                1.0,
                1.0
            },
            inequalities,
            equalities);

        Assert.Single(
            inequalities);

        Assert.True(
            inequalities[0] <=
            0.0);

        Assert.Empty(
            equalities);
    }

    [Fact]
    public void DebConstraintGa_ReturnsFeasibleNonOptimalIncumbent()
    {
        MathematicalModel model =
            CreateToyModel();

        var optimizer =
            new DebConstraintGaMathematicalModelOptimizer();

        DebConstraintGaBridgeResult bridgeResult =
            optimizer.Optimize(
                model,
                new DebConstraintGaBridgeOptions
                {
                    PopulationSize = 8,
                    MaximumGenerations = 3,
                    Seed = 12345UL
                });

        Assert.True(
            bridgeResult.IsFeasible);

        Assert.True(
            bridgeResult.ModelVariableValues[0] is
                0.0 or
                1.0);

        var projector =
            new MetaheuristicsPlatformMathematicalResultProjector();

        MathematicalModelSolveResult projected =
            projector.Project(
                model,
                bridgeResult);

        Assert.True(
            projected.HasFeasibleSolution);

        Assert.False(
            projected.IsOptimal);

        Assert.Null(
            projected.BestBound);

        Assert.Null(
            projected.RelativeGap);

        Assert.Null(
            projected.AbsoluteGap);
    }

    [Fact]
    public void LocalBranchingSubproblem_IsSourcePreserving()
    {
        MathematicalModel source =
            CreateToyModel();

        var builder =
            new MetaheuristicsPlatformExactRepairSubproblemBuilder();

        MathematicalModel subproblem =
            builder.BuildLocalBranchingSubproblem(
                source,
                new[]
                {
                    0.0,
                    0.0
                },
                hammingRadius:
                    1,
                nodeLimit:
                    10);

        Assert.Single(
            source.Constraints);

        Assert.Equal(
            2,
            subproblem.Constraints.Count);

        Assert.DoesNotContain(
            source.Constraints,
            constraint =>
                constraint.Name ==
                "metaheuristicLocalBranchingHamming");

        Assert.Contains(
            subproblem.Constraints,
            constraint =>
                constraint.Name ==
                "metaheuristicLocalBranchingHamming");
    }

    [Fact]
    public void LocalBranchingBridge_ExecutesAgainstLotSizingExactRepairDomain()
    {
        MathematicalModel model =
            CreateToyModel();

        IReadOnlyDictionary<int, double> initial =
            new Dictionary<int, double>
            {
                [1] = 0.0,
                [2] = 0.0
            };

        MathematicalModelSolveResult SameIncumbentSolver(
            MathematicalModel subproblem,
            int nodeLimit,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Assert.True(
                nodeLimit > 0);

            var result =
                new MathematicalModelSolveResult
                {
                    RunName =
                        "fake-exact-repair",

                    FormulationId =
                        "toy",

                    SolverKind =
                        SolverKind.Unknown,

                    SolverName =
                        "deterministic-test-solver",

                    SolverVersion =
                        "1",

                    TerminationReason =
                        SolverTerminationReason.Optimal,

                    HasFeasibleSolution =
                        true,

                    IsOptimal =
                        true,

                    ObjectiveValue =
                        0.0
                };

            foreach (MathematicalVariable variable
                     in subproblem.Variables
                         .OrderBy(
                             variable =>
                                 variable.Id))
            {
                result.AddVariableValue(
                    new MathematicalVariableValue(
                        variable.Id,
                        0.0,
                        variable.Name,
                        variable.DomainKey));
            }

            result.EnsureValid();

            return result;
        }

        var domain =
            new LotSizingExactRepairMatheuristicDomain(
                model,
                initial,
                SameIncumbentSolver,
                SameIncumbentSolver);

        var bridge =
            new LocalBranchingMatheuristicBridge();

        LocalBranchingBridgeResult result =
            bridge.Optimize(
                domain,
                new LocalBranchingBridgeOptions
                {
                    MaximumIterations = 2,
                    HammingRadius = 1,
                    NodeLimit = 10,
                    Seed = 7UL
                });

        Assert.Equal(
            0.0,
            result.BestObjective,
            10);

        Assert.True(
            result.ExactSolves >= 1);

        Assert.Contains(
            "local-branching",
            result.Trace);
    }

    private static MathematicalModel CreateToyModel()
    {
        var model =
            new MathematicalModel
            {
                Name =
                    "metaheuristic-toy"
            };

        model.AddVariable(
            new MathematicalVariable(
                1,
                "x",
                MathematicalVariableType.Binary,
                0.0,
                1.0));

        model.AddVariable(
            new MathematicalVariable(
                2,
                "y",
                MathematicalVariableType.Continuous,
                0.0,
                2.0));

        model.Objective.Expression.AddTerm(
            1,
            1.0);

        model.Objective.Expression.AddTerm(
            2,
            1.0);

        var expression =
            new LinearExpression();

        expression.AddTerm(
            1,
            1.0);

        expression.AddTerm(
            2,
            1.0);

        model.AddConstraint(
            new LinearConstraint(
                10,
                "capacity",
                expression,
                MathematicalConstraintSense.LessThanOrEqual,
                3.0));

        model.EnsureValid();

        return model;
    }
}
