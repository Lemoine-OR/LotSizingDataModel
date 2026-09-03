using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class SchedulingChangeoverVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    public override string FamilyId =>
        MathematicalDecisionCategory.AuxiliaryMicroPeriodChangeover;

    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        return instance.SupplyChain.Plants
            .SelectMany(plant => plant.WorkCenters)
            .Any(workCenter =>
                workCenter.SetupTransitionProfile is not null &&
                workCenter.SetupTransitionProfile.Changeovers.Count > 0);
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
                var profile =
                    workCenter.SetupTransitionProfile;

                var scheduling =
                    workCenter.SchedulingProfile;

                if (profile is null ||
                    scheduling is null)
                {
                    continue;
                }

                for (int period = 1;
                     period <= instance.PlanningHorizon;
                     period++)
                {
                    for (int microPeriod = 1;
                         microPeriod <= scheduling.MicroPeriodsPerPeriod;
                         microPeriod++)
                    {
                        foreach (var changeover in profile.Changeovers)
                        {
                            string key =
                                SchedulingVariableKeyFactory.Changeover(
                                    plant.Id,
                                    workCenter.Id,
                                    changeover.FromItemId,
                                    changeover.ToItemId,
                                    period,
                                    microPeriod);

                            AddBinaryVariable(
                                context,
                                $"q_p{plant.Id}_w{workCenter.Id}_i{changeover.FromItemId}_j{changeover.ToItemId}_t{period}_s{microPeriod}",
                                key,
                                "Sequence-dependent setup changeover.");
                        }
                    }
                }
            }
        }

        return ValueTask.CompletedTask;
    }
}
