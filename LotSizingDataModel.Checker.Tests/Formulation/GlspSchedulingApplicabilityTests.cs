using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Formulation;

namespace LotSizingDataModel.Checker.Tests.Formulation;

public sealed class GlspSchedulingApplicabilityTests
{
    [Fact]
    public void GlspCanBuild_NonSchedulingInstance_ReturnsFalseWithoutThrowing()
    {
        LotSizingInstance instance =
            CreateReferenceLikeNonSchedulingInstance();

        GlspSchedulingFormulation formulation =
            GlspSchedulingFormulationFactory.CreateDefault();

        bool canBuild =
            formulation.CanBuild(
                instance);

        Assert.False(
            canBuild);
    }

    [Fact]
    public void DefaultRegistry_AllCanBuildPredicatesAreTotalForNonSchedulingInstance()
    {
        LotSizingInstance instance =
            CreateReferenceLikeNonSchedulingInstance();

        var registry =
            LotSizingFormulationRegistryFactory.CreateDefault();

        var results =
            registry.GetAll()
                .Select(
                    formulation =>
                        (
                            formulation.FormulationId,
                            CanBuild:
                                formulation.CanBuild(instance)
                        ))
                .ToArray();

        Assert.Equal(
            registry.GetAll().Count,
            results.Length);

        Assert.False(
            Assert.Single(
                results,
                result =>
                    result.FormulationId ==
                    GlspSchedulingFormulation.FormulationIdValue)
                .CanBuild);
    }

    [Fact]
    public void GlspCanBuild_IncompleteProductionCharacteristic_ReturnsFalse()
    {
        LotSizingInstance instance =
            CreateIncompleteGlspInstance();

        GlspSchedulingFormulation formulation =
            GlspSchedulingFormulationFactory.CreateDefault();

        Assert.False(
            formulation.CanBuild(
                instance));
    }

    private static LotSizingInstance
        CreateReferenceLikeNonSchedulingInstance()
    {
        var chain =
            new SupplyChain(2);

        chain.Items.Add(
            new Item(
                1,
                "I1",
                0));

        var plant =
            new Plant(
                1,
                "P1",
                new PlantWarehouse(
                    "P1-Warehouse"));

        plant.WorkCenters.Add(
            new WorkCenter(
                1,
                "M1"));

        chain.Plants.Add(
            plant);

        var routing =
            new ProductionRouting(
                1,
                1,
                1,
                0);

        routing.AddWorkCenter(
            1);

        chain.ProductionRoutings.Add(
            routing);

        return new LotSizingInstance(
            chain,
            "non-scheduling-regression");
    }

    private static LotSizingInstance
        CreateIncompleteGlspInstance()
    {
        const int horizon = 2;

        var chain =
            new SupplyChain(horizon);

        chain.Items.Add(
            new Item(
                1,
                "I1",
                0));

        chain.Items.Add(
            new Item(
                2,
                "I2",
                0));

        var workCenter =
            new WorkCenter(
                1,
                "M1")
            {
                CapacityConstraint =
                    new Core.DecisionModel.Constraints
                        .CapacityConstraint(
                            horizon,
                            10.0),

                SchedulingProfile =
                    new ProductionSchedulingProfile
                    {
                        BucketMode =
                            SchedulingBucketMode.MacroMicro,

                        MicroPeriodLengthMode =
                            MicroPeriodLengthMode.Variable,

                        MicroPeriodAssignmentMode =
                            MicroPeriodAssignmentMode.SingleItem,

                        SetupCarryOverPolicy =
                            SetupCarryOverPolicy.Allowed,

                        MicroPeriodCount =
                            new MicroPeriodCount(
                                horizon,
                                2)
                    }
            };

        var plant =
            new Plant(
                1,
                "P1",
                new PlantWarehouse(
                    "P1-Warehouse"));

        plant.WorkCenters.Add(
            workCenter);

        chain.Plants.Add(
            plant);

        AddRouting(
            chain,
            1,
            1);

        AddRouting(
            chain,
            2,
            2);

        // Intentionally no ProductionCharacteristic objects.
        // CanBuild must return false rather than expose a Single() failure.
        return new LotSizingInstance(
            chain,
            "incomplete-glsp-regression");
    }

    private static void AddRouting(
        SupplyChain chain,
        int routingId,
        int itemId)
    {
        var routing =
            new ProductionRouting(
                routingId,
                itemId,
                1,
                0);

        routing.AddWorkCenter(
            1);

        chain.ProductionRoutings.Add(
            routing);
    }
}
