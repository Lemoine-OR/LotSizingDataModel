using LotSizingDataModel.Checker.Feasibility;
using LotSizingDataModel.Core;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.ClosedLoop;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Common;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Feasibility;
using LotSizingDataModel.Solver.Modeling;
using Xunit;

namespace LotSizingDataModel.Checker.Tests.Formulation;

public sealed class FeasibilityCheckerExpansionTests
{
    [Fact]
    public void IntrinsicAnalyzer_ProvesImpossibleEquality()
    {
        MathematicalModel model =
            CreateBoundedModel(
                MathematicalConstraintSense.Equal,
                rhs: 2.0);

        IntrinsicFeasibilityAnalysisResult result =
            new InstanceFeasibilityAnalyzer()
                .AnalyzeModel(
                    model);

        Assert.Equal(
            IntrinsicFeasibilityStatus.Infeasible,
            result.Status);

        IntrinsicFeasibilityDiagnostic diagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            "LSDM-FEAS-004",
            diagnostic.Code);
    }

    [Fact]
    public void IntrinsicAnalyzer_NeverPromotesUnknownToFeasible()
    {
        MathematicalModel model =
            CreateBoundedModel(
                MathematicalConstraintSense.LessThanOrEqual,
                rhs: 1.0);

        IntrinsicFeasibilityAnalysisResult result =
            new MathematicalModelIntrinsicFeasibilityAnalyzer()
                .Analyze(
                    model);

        Assert.Equal(
            IntrinsicFeasibilityStatus.Unknown,
            result.Status);

        Assert.False(
            result.HasProofOfInfeasibility);
    }

    [Fact]
    public void MathematicalChecker_DetectsConstraintViolationIndependently()
    {
        MathematicalModel model =
            CreateBoundedModel(
                MathematicalConstraintSense.LessThanOrEqual,
                rhs: 0.25);

        MathematicalModelSolveResult solveResult =
            CreateSolveResult(
                model,
                value: 0.75);

        MathematicalFeasibilityCheckResult result =
            new MathematicalModelSolveResultFeasibilityChecker()
                .Check(
                    model,
                    solveResult);

        Assert.Equal(
            FeasibilityStatus.Infeasible,
            result.Status);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code ==
                "LSDM-FEAS-SOL-004");
    }

    [Fact]
    public void MathematicalChecker_MissingVariableIsPartialNotFalseInfeasible()
    {
        MathematicalModel model =
            CreateBoundedModel(
                MathematicalConstraintSense.LessThanOrEqual,
                rhs: 1.0);

        var solveResult =
            new MathematicalModelSolveResult
            {
                RunName = "partial",
                FormulationId = "test",
                SolverKind = SolverKind.Unknown,
                SolverName = "test",
                SolverVersion = "1",
                TerminationReason = SolverTerminationReason.Feasible,
                HasFeasibleSolution = true,
                IsOptimal = false,
                ObjectiveValue = 0.0
            };

        MathematicalFeasibilityCheckResult result =
            new MathematicalModelSolveResultFeasibilityChecker()
                .Check(
                    model,
                    solveResult);

        Assert.Equal(
            FeasibilityStatus.PartiallyEvaluated,
            result.Status);
    }

    [Fact]
    public void ClosedLoopChecker_ValidatesConservationYieldAndCapacity()
    {
        LotSizingInstance instance =
            CreateClosedLoopInstance();

        var solution =
            new LotSizingSolution(
                planningHorizon: 1);

        var decision =
            new ClosedLoopDecision(
                returnStreamId: 1,
                planningHorizon: 1);

        decision.RecoveryInputs[1] = 3.0;
        decision.DisposalQuantities[1] = 7.0;
        decision.RecoveredOutputs[1] = 2.4;

        solution.ClosedLoopDecisions.Add(
            decision);

        MathematicalFeasibilityCheckResult result =
            new ClosedLoopSolutionFeasibilityChecker()
                .Check(
                    instance,
                    solution);

        Assert.Equal(
            FeasibilityStatus.Feasible,
            result.Status);
    }

    [Fact]
    public void ClosedLoopChecker_MissingDecisionIsPartial()
    {
        LotSizingInstance instance =
            CreateClosedLoopInstance();

        var solution =
            new LotSizingSolution(
                planningHorizon: 1);

        MathematicalFeasibilityCheckResult result =
            new ClosedLoopSolutionFeasibilityChecker()
                .Check(
                    instance,
                    solution);

        Assert.Equal(
            FeasibilityStatus.PartiallyEvaluated,
            result.Status);
    }

    private static MathematicalModel CreateBoundedModel(
        MathematicalConstraintSense sense,
        double rhs)
    {
        var model =
            new MathematicalModel
            {
                Name = "feasibility-test"
            };

        model.AddVariable(
            new MathematicalVariable(
                1,
                "x",
                MathematicalVariableType.Continuous,
                0.0,
                1.0));

        var expression =
            new LinearExpression();

        expression.AddTerm(
            1,
            1.0);

        model.AddConstraint(
            new LinearConstraint(
                1,
                "c",
                expression,
                sense,
                rhs));

        model.EnsureValid();

        return model;
    }

    private static MathematicalModelSolveResult CreateSolveResult(
        MathematicalModel model,
        double value)
    {
        var result =
            new MathematicalModelSolveResult
            {
                RunName = "test",
                FormulationId = "test",
                SolverKind = SolverKind.Unknown,
                SolverName = "test",
                SolverVersion = "1",
                TerminationReason = SolverTerminationReason.Feasible,
                HasFeasibleSolution = true,
                IsOptimal = false,
                ObjectiveValue = value
            };

        MathematicalVariable variable =
            Assert.Single(
                model.Variables);

        result.AddVariableValue(
            new MathematicalVariableValue(
                variable.Id,
                value,
                variable.Name,
                variable.DomainKey));

        result.EnsureValid();

        return result;
    }

    private static LotSizingInstance CreateClosedLoopInstance()
    {
        var supplyChain =
            new SupplyChain(
                planningHorizon: 1);

        var instance =
            new LotSizingInstance(
                supplyChain);

        var stream =
            new ClosedLoopReturnStream(
                id: 1,
                itemId: 5,
                distributionCenterId: 7,
                recoveryWarehouse:
                    WarehouseReference.ForPlantWarehouse(
                        3),
                planningHorizon: 1)
            {
                RecoveryYield = 0.8,
                RecoveryCapacity =
                    new ClosedLoopTimeSeriesParameter(
                        1)
            };

        stream.ReturnQuantity[1] = 10.0;
        stream.RecoveryCapacity[1] = 4.0;

        instance.ClosedLoopReturnStreams.Add(
            stream);

        return instance;
    }
}
