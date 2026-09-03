using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class SchedulingSetupStateVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    public override string FamilyId =>
        MathematicalDecisionCategory.MicroPeriodSetupState;

    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        return instance.SupplyChain.Plants
            .SelectMany(plant => plant.WorkCenters)
            .Any(workCenter =>
                workCenter.SchedulingProfile is not null);
    }

    protected override ValueTask BuildFamilyAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (var plant in instance.SupplyChain.Plants)
        {
            foreach (var workCenter in plant.WorkCenters)
            {
                ProductionSchedulingProfile? profile =
                    workCenter.SchedulingProfile;

                if (profile is null)
                {
                    continue;
                }

                var itemIds =
                    instance.SupplyChain.ProductionRoutings
                        .Where(routing =>
                            routing.PlantId == plant.Id &&
                            routing.UsesWorkCenter(workCenter.Id))
                        .Select(routing => routing.ItemId)
                        .Distinct()
                        .ToArray();

                for (int period = 1;
                     period <= instance.PlanningHorizon;
                     period++)
                {
                    for (int microPeriod = 1;
                         microPeriod <= profile.MicroPeriodsPerPeriod;
                         microPeriod++)
                    {
                        foreach (int itemId in itemIds)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            string key =
                                SchedulingVariableKeyFactory.SetupState(
                                    plant.Id,
                                    workCenter.Id,
                                    itemId,
                                    period,
                                    microPeriod);

                            AddBinaryVariable(
                                context,
                                $"z_p{plant.Id}_w{workCenter.Id}_i{itemId}_t{period}_s{microPeriod}",
                                key,
                                "Micro-period production setup state.");
                        }
                    }
                }
            }
        }

        return ValueTask.CompletedTask;
    }
}
