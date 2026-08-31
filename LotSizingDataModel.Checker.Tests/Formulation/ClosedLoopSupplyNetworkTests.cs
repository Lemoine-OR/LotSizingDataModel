using System.Xml.Serialization;
using LotSizingDataModel.Core;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.ClosedLoop;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;
using Xunit;

namespace LotSizingDataModel.Checker.Tests.Formulation;

public sealed class ClosedLoopSupplyNetworkTests
{
    [Fact]
    public void ReturnStream_ResizesWithInstanceAdd()
    {
        var instance =
            new LotSizingInstance(
                new SupplyChain(
                    planningHorizon:
                        3));

        var stream =
            CreateStream(
                planningHorizon:
                    1);

        instance.AddClosedLoopReturnStream(
            stream);

        Assert.Equal(
            3,
            stream.PlanningHorizon);

        Assert.Equal(
            3,
            stream.CollectionUnitCost.PlanningHorizon);

        Assert.Equal(
            3,
            stream.RecoveryUnitCost.PlanningHorizon);

        Assert.Equal(
            3,
            stream.DisposalUnitCost.PlanningHorizon);
    }

    [Fact]
    public void ClosedLoopInstanceExtension_IsXmlSerializable()
    {
        var instance =
            new LotSizingInstance(
                new SupplyChain(
                    planningHorizon:
                        2));

        var stream =
            CreateStream(
                planningHorizon:
                    2);

        stream.ReturnQuantity[1] =
            10.0;

        stream.ReturnQuantity[2] =
            4.0;

        instance.ClosedLoopReturnStreams.Add(
            stream);

        var serializer =
            new XmlSerializer(
                typeof(LotSizingInstance));

        using var writer =
            new StringWriter();

        serializer.Serialize(
            writer,
            instance);

        string xml =
            writer.ToString();

        Assert.Contains(
            "closedLoopReturnStreams",
            xml,
            StringComparison.Ordinal);

        Assert.Contains(
            "closedLoopReturnStream",
            xml,
            StringComparison.Ordinal);

        using var reader =
            new StringReader(
                xml);

        var clone =
            Assert.IsType<LotSizingInstance>(
                serializer.Deserialize(
                    reader));

        ClosedLoopReturnStream clonedStream =
            Assert.Single(
                clone.ClosedLoopReturnStreams);

        Assert.Equal(
            10.0,
            clonedStream.ReturnQuantity[1],
            10);

        Assert.Equal(
            0.8,
            clonedStream.RecoveryYield,
            10);
    }

    [Fact]
    public void Decorator_ConservesReturnsAndInjectsRecoveredInventoryInflow()
    {
        ClosedLoopReturnStream stream =
            CreateStream(
                planningHorizon:
                    1);

        stream.ReturnQuantity[1] =
            10.0;

        stream.CollectionUnitCost[1] =
            0.5;

        stream.RecoveryUnitCost[1] =
            2.0;

        stream.DisposalUnitCost[1] =
            5.0;

        MathematicalModel source =
            CreateInventoryBalanceModel(
                stream);

        var decorator =
            new ClosedLoopSupplyNetworkModelDecorator();

        MathematicalModel decorated =
            decorator.Apply(
                new[]
                {
                    stream
                },
                source);

        Assert.Single(
            source.Variables);

        Assert.Single(
            source.Constraints);

        Assert.Equal(
            3,
            decorated.Variables.Count);

        Assert.Equal(
            2,
            decorated.Constraints.Count);

        LinearConstraint allocation =
            decorated.Constraints.Single(
                constraint =>
                    constraint.Name.StartsWith(
                        "closedLoopReturnAllocation_",
                        StringComparison.Ordinal));

        Assert.Equal(
            10.0,
            allocation.RightHandSide,
            10);

        Assert.Equal(
            2,
            allocation.LeftHandSide.Terms.Count);

        LinearConstraint inventoryBalance =
            decorated.Constraints.Single(
                constraint =>
                    constraint.Name.StartsWith(
                        "inventoryBalance_",
                        StringComparison.Ordinal));

        MathematicalVariable recoveryVariable =
            decorated.Variables.Single(
                variable =>
                    variable.DomainKey.StartsWith(
                        ClosedLoopMathematicalDecisionCategory.RecoveryInput,
                        StringComparison.Ordinal));

        double recoveryCoefficient =
            inventoryBalance.LeftHandSide.Terms
                .Where(
                    term =>
                        term.VariableId ==
                        recoveryVariable.Id)
                .Sum(
                    term =>
                        term.Coefficient);

        Assert.Equal(
            -0.8,
            recoveryCoefficient,
            10);

        Assert.Equal(
            5.0,
            decorated.Objective.Expression.Constant,
            10);
    }

    [Fact]
    public void RecoveryCapacity_AddsExplicitUpperConstraint()
    {
        ClosedLoopReturnStream stream =
            CreateStream(
                planningHorizon:
                    1);

        stream.ReturnQuantity[1] =
            10.0;

        stream.RecoveryCapacity =
            new ClosedLoopTimeSeriesParameter(
                1);

        stream.RecoveryCapacity[1] =
            3.0;

        MathematicalModel source =
            CreateInventoryBalanceModel(
                stream);

        MathematicalModel decorated =
            new ClosedLoopSupplyNetworkModelDecorator()
                .Apply(
                    new[]
                    {
                        stream
                    },
                    source);

        LinearConstraint capacity =
            decorated.Constraints.Single(
                constraint =>
                    constraint.Name.StartsWith(
                        "closedLoopRecoveryCapacity_",
                        StringComparison.Ordinal));

        Assert.Equal(
            3.0,
            capacity.RightHandSide,
            10);
    }

    [Fact]
    public void Projector_ReportsRecoveryDisposalAndRecoveredOutput()
    {
        ClosedLoopReturnStream stream =
            CreateStream(
                planningHorizon:
                    1);

        stream.ReturnQuantity[1] =
            10.0;

        MathematicalModel model =
            new ClosedLoopSupplyNetworkModelDecorator()
                .Apply(
                    new[]
                    {
                        stream
                    },
                    CreateInventoryBalanceModel(
                        stream));

        MathematicalVariable recovery =
            model.Variables.Single(
                variable =>
                    variable.DomainKey.StartsWith(
                        ClosedLoopMathematicalDecisionCategory.RecoveryInput,
                        StringComparison.Ordinal));

        MathematicalVariable disposal =
            model.Variables.Single(
                variable =>
                    variable.DomainKey.StartsWith(
                        ClosedLoopMathematicalDecisionCategory.Disposal,
                        StringComparison.Ordinal));

        var solveResult =
            new MathematicalModelSolveResult
            {
                RunName =
                    "closed-loop-test",

                FormulationId =
                    "closed-loop",

                SolverKind =
                    SolverKind.Unknown,

                SolverName =
                    "test",

                SolverVersion =
                    "1",

                TerminationReason =
                    SolverTerminationReason.Feasible,

                HasFeasibleSolution =
                    true,

                IsOptimal =
                    false,

                ObjectiveValue =
                    0.0
            };

        solveResult.AddVariableValue(
            new MathematicalVariableValue(
                recovery.Id,
                6.0,
                recovery.Name,
                recovery.DomainKey));

        solveResult.AddVariableValue(
            new MathematicalVariableValue(
                disposal.Id,
                4.0,
                disposal.Name,
                disposal.DomainKey));

        ClosedLoopDecisionSnapshot snapshot =
            Assert.Single(
                new ClosedLoopDecisionProjector()
                    .Project(
                        new[]
                        {
                            stream
                        },
                        model,
                        solveResult));

        Assert.Equal(
            6.0,
            snapshot.RecoveryInput,
            10);

        Assert.Equal(
            4.0,
            snapshot.DisposalQuantity,
            10);

        Assert.Equal(
            4.8,
            snapshot.RecoveredOutput,
            10);
    }

    private static ClosedLoopReturnStream CreateStream(
        int planningHorizon)
    {
        return new ClosedLoopReturnStream(
            id:
                1,
            itemId:
                5,
            distributionCenterId:
                7,
            recoveryWarehouse:
                WarehouseReference.ForPlantWarehouse(
                    3),
            planningHorizon:
                planningHorizon)
        {
            RecoveryYield =
                0.8
        };
    }

    private static MathematicalModel CreateInventoryBalanceModel(
        ClosedLoopReturnStream stream)
    {
        var model =
            new MathematicalModel
            {
                Name =
                    "closed-loop-inventory-test"
            };

        string inventoryKey =
            new LotSizingDataModel.Solver.Building
                .MathematicalDomainKeyBuilder(
                    MathematicalDecisionCategory.Inventory)
                .Add(
                    MathematicalDomainKeySegment.Item,
                    stream.ItemId)
                .Add(
                    MathematicalDomainKeySegment.Plant,
                    stream.RecoveryWarehouse.ReferenceId)
                .Add(
                    MathematicalDomainKeySegment.Period,
                    1)
                .Build();

        var inventory =
            new MathematicalVariable(
                1,
                "I_i5_p3_t1",
                MathematicalVariableType.Continuous,
                0.0,
                100.0)
            {
                DomainKey =
                    inventoryKey
            };

        model.AddVariable(
            inventory);

        var balance =
            new LinearExpression();

        balance.AddTerm(
            inventory.Id,
            1.0);

        model.AddConstraint(
            new LinearConstraint(
                1,
                "inventoryBalance_i5_w3_t1",
                balance,
                MathematicalConstraintSense.Equal,
                0.0));

        model.EnsureValid();

        return model;
    }
}
