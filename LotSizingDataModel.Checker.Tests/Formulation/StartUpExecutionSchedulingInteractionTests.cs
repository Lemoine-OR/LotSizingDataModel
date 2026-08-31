using System.Reflection;
using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Tests.Formulation;

public sealed class StartUpExecutionSchedulingInteractionTests
{
    [Fact]
    public async Task Standard_StartUpCost_HasDistinctExactTransitionVariableAndObjectiveTerm()
    {
        LotSizingInstance instance =
            CreateStandardStartUpCostInstance();

        MathematicalModel model =
            await StandardLotSizingFormulationFactory
                .CreateDefault()
                .BuildAsync(instance);

        MathematicalVariable startUp =
            Assert.Single(
                model.Variables,
                variable =>
                    variable.DomainKey.StartsWith(
                        MathematicalDecisionCategory.AuxiliaryProductionStartUp + "|",
                        StringComparison.Ordinal));

        Assert.Contains(
            model.Constraints,
            constraint =>
                constraint.Name == "productionStartUpInitial_r1_t1");

        LinearTerm term =
            Assert.Single(
                model.Objective.Expression.Terms,
                candidate =>
                    candidate.VariableId == startUp.Id);

        Assert.Equal(
            7.0,
            term.Coefficient,
            12);
    }

    [Fact]
    public async Task Cslp_StartUpCostAndTime_AreExecutableAndAdditive()
    {
        LotSizingInstance instance =
            CreateSmallBucket(
                SmallBucketSchedulingFormulationKind.Cslp,
                includeStartUpTime:true);

        var formulation =
            SmallBucketSchedulingFormulationFactory.CreateCslp();

        Assert.True(
            formulation.CanBuild(instance));

        MathematicalModel model =
            await formulation.BuildAsync(instance);

        MathematicalVariable startUp =
            model.Variables.First(
                variable =>
                    variable.DomainKey.StartsWith(
                        MathematicalDecisionCategory.AuxiliaryProductionStartUp + "|",
                        StringComparison.Ordinal));

        Assert.Contains(
            model.Constraints,
            constraint =>
                constraint.Name == "smallBucketProductionStartUpDefinition_r1_t1");

        LinearConstraint capacity =
            model.Constraints.Single(
                constraint =>
                    constraint.Name == "smallBucketCapacity_p1_w1_t1");

        Assert.Contains(
            capacity.LeftHandSide.Terms,
            term =>
                term.VariableId == startUp.Id &&
                Math.Abs(term.Coefficient - 2.0) <= 1.0e-12);

        Assert.Contains(
            model.Objective.Expression.Terms,
            term =>
                term.VariableId == startUp.Id &&
                Math.Abs(term.Coefficient - 7.0) <= 1.0e-12);
    }

    [Fact]
    public void Dlsp_StartUpCost_IsExecutable_ButStartUpTimeRemainsExplicitlyUnsupported()
    {
        LotSizingInstance costOnly =
            CreateSmallBucket(
                SmallBucketSchedulingFormulationKind.Dlsp,
                includeStartUpTime:false);

        Assert.True(
            SmallBucketSchedulingFormulationFactory
                .CreateDlsp()
                .CanBuild(costOnly));

        LotSizingInstance withTime =
            CreateSmallBucket(
                SmallBucketSchedulingFormulationKind.Dlsp,
                includeStartUpTime:true);

        Assert.False(
            SmallBucketSchedulingFormulationFactory
                .CreateDlsp()
                .CanBuild(withTime));
    }

    [Fact]
    public async Task Glsp_StartUpTimeAndChangeoverTime_CoexistWithoutParameterReuse()
    {
        LotSizingInstance instance =
            CreateGlsp();

        var formulation =
            GlspSchedulingFormulationFactory.CreateDefault();

        Assert.True(
            formulation.CanBuild(instance));

        MathematicalModel model =
            await formulation.BuildAsync(instance);

        MathematicalVariable startUp =
            model.Variables.First(
                variable =>
                    variable.DomainKey.StartsWith(
                        MathematicalDecisionCategory.AuxiliaryProductionStartUp + "|",
                        StringComparison.Ordinal));

        LinearConstraint capacity =
            model.Constraints.Single(
                constraint =>
                    constraint.Name == "glspMacroCapacity_p1_w1_t1");

        Assert.Contains(
            capacity.LeftHandSide.Terms,
            term =>
                term.VariableId == startUp.Id &&
                Math.Abs(term.Coefficient - 2.0) <= 1.0e-12);

        Assert.Contains(
            model.Variables,
            variable =>
                variable.DomainKey.StartsWith(
                    MathematicalDecisionCategory.AuxiliaryMicroPeriodChangeover + "|",
                    StringComparison.Ordinal));
    }

    private static LotSizingInstance CreateStandardStartUpCostInstance()
    {
        const int horizon=1;
        var chain=new SupplyChain(horizon);
        chain.Items.Add(new Item(1,"I1",0));

        var plant=new Plant(
            1,
            "P1",
            new PlantWarehouse("P1-Warehouse"));

        plant.WorkCenters.Add(
            new WorkCenter(1,"M1"));

        chain.Plants.Add(plant);

        var routing=new ProductionRouting(1,1,1,0);
        routing.AddWorkCenter(1);
        chain.ProductionRoutings.Add(routing);

        var characteristic=
            new ProductionCharacteristic(1,1,1);

        SetSeries(
            characteristic,
            "StartUpCost",
            horizon,
            7.0);

        chain.ProductionCharacteristics.Add(characteristic);

        return new LotSizingInstance(
            chain,
            "standard-start-up");
    }

    private static LotSizingInstance CreateSmallBucket(
        SmallBucketSchedulingFormulationKind kind,
        bool includeStartUpTime)
    {
        const int horizon=2;
        var chain=new SupplyChain(horizon);
        chain.Items.Add(new Item(1,"I1",0));

        var profile=
            new ProductionSchedulingProfile
            {
                BucketMode=SchedulingBucketMode.SmallBucket,
                SmallBucketProductionMode=
                    kind==SmallBucketSchedulingFormulationKind.Dlsp
                        ? SmallBucketProductionMode.AllOrNothing
                        : SmallBucketProductionMode.Continuous,
                SetupCarryOverPolicy=SetupCarryOverPolicy.Allowed,
                MaximumProducedItemCount=
                    new MaximumProducedItemCount(horizon,1),
                MaximumSetupCount=
                    new MaximumSetupCount(horizon,1)
            };

        var workCenter=
            new WorkCenter(1,"M1")
            {
                CapacityConstraint=
                    new CapacityConstraint(horizon,10.0),
                SchedulingProfile=profile
            };

        var plant=
            new Plant(
                1,
                "P1",
                new PlantWarehouse("P1-Warehouse"));

        plant.WorkCenters.Add(workCenter);
        chain.Plants.Add(plant);

        var routing=new ProductionRouting(1,1,1,0);
        routing.AddWorkCenter(1);
        chain.ProductionRoutings.Add(routing);

        var characteristic=
            new ProductionCharacteristic(1,1,1)
            {
                UnitCapacityConsumption=
                    new UnitCapacityConsumption(horizon,1.0)
            };

        SetSeries(
            characteristic,
            "StartUpCost",
            horizon,
            7.0);

        if(includeStartUpTime)
        {
            SetSeries(
                characteristic,
                "StartUpTime",
                horizon,
                2.0);
        }

        chain.ProductionCharacteristics.Add(characteristic);

        return new LotSizingInstance(
            chain,
            "small-start-up");
    }

    private static LotSizingInstance CreateGlsp()
    {
        const int horizon=1;
        var chain=new SupplyChain(horizon);
        chain.Items.Add(new Item(1,"I1",0));
        chain.Items.Add(new Item(2,"I2",0));

        var profile=
            new ProductionSchedulingProfile
            {
                BucketMode=SchedulingBucketMode.MacroMicro,
                MicroPeriodLengthMode=MicroPeriodLengthMode.Variable,
                MicroPeriodAssignmentMode=MicroPeriodAssignmentMode.SingleItem,
                SetupCarryOverPolicy=SetupCarryOverPolicy.Allowed,
                MicroPeriodCount=new MicroPeriodCount(horizon,2)
            };

        profile.Changeovers.Add(
            new ProductionChangeover
            {
                FromItemId=1,
                ToItemId=2,
                ChangeoverTime=
                    new SequenceDependentChangeoverTime(horizon,0.5),
                ChangeoverCost=
                    new SequenceDependentChangeoverCost(horizon,3.0)
            });

        profile.Changeovers.Add(
            new ProductionChangeover
            {
                FromItemId=2,
                ToItemId=1,
                ChangeoverTime=
                    new SequenceDependentChangeoverTime(horizon,0.5),
                ChangeoverCost=
                    new SequenceDependentChangeoverCost(horizon,3.0)
            });

        var workCenter=
            new WorkCenter(1,"M1")
            {
                CapacityConstraint=
                    new CapacityConstraint(horizon,20.0),
                SchedulingProfile=profile
            };

        var plant=
            new Plant(
                1,
                "P1",
                new PlantWarehouse("P1-Warehouse"));

        plant.WorkCenters.Add(workCenter);
        chain.Plants.Add(plant);

        AddGlspRouting(chain,1,1,horizon);
        AddGlspRouting(chain,2,2,horizon);

        return new LotSizingInstance(
            chain,
            "glsp-start-up");
    }

    private static void AddGlspRouting(
        SupplyChain chain,
        int routingId,
        int itemId,
        int horizon)
    {
        var routing=
            new ProductionRouting(
                routingId,
                itemId,
                1,
                0);

        routing.AddWorkCenter(1);
        chain.ProductionRoutings.Add(routing);

        var characteristic=
            new ProductionCharacteristic(itemId,1,1)
            {
                UnitCapacityConsumption=
                    new UnitCapacityConsumption(horizon,1.0)
            };

        SetSeries(
            characteristic,
            "StartUpCost",
            horizon,
            7.0);

        SetSeries(
            characteristic,
            "StartUpTime",
            horizon,
            2.0);

        chain.ProductionCharacteristics.Add(characteristic);
    }

    private static void SetSeries(
        ProductionCharacteristic target,
        string propertyName,
        int horizon,
        double value)
    {
        PropertyInfo property=
            typeof(ProductionCharacteristic).GetProperty(propertyName)
            ?? throw new InvalidOperationException(
                $"ProductionCharacteristic.{propertyName} is missing.");

        Type parameterType=
            property.PropertyType;

        object parameter=
            Activator.CreateInstance(parameterType)
            ?? throw new InvalidOperationException(
                $"Cannot construct {parameterType.FullName}.");

        MethodInfo resize=
            parameterType.GetMethod(
                "ResizeTimeSeries",
                new[]{typeof(int)})
            ?? throw new InvalidOperationException(
                $"{parameterType.FullName}.ResizeTimeSeries(int) is missing.");

        resize.Invoke(
            parameter,
            new object[]{horizon});

        PropertyInfo indexer=
            parameterType.GetProperty("Item")
            ?? throw new InvalidOperationException(
                $"{parameterType.FullName} indexer is missing.");

        for(int period=1;
            period<=horizon;
            period++)
        {
            indexer.SetValue(
                parameter,
                value,
                new object[]{period});
        }

        property.SetValue(
            target,
            parameter);
    }
}
