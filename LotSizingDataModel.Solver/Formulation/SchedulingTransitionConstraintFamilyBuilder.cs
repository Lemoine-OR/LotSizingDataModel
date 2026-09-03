using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class SchedulingTransitionConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId =>
        "schedulingTransitions";

    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        return instance.SupplyChain.Plants
            .SelectMany(plant => plant.WorkCenters)
            .Any(workCenter =>
                workCenter.SchedulingProfile is not null &&
                workCenter.SetupTransitionProfile is not null);
    }

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (var plant in instance.SupplyChain.Plants)
        {
            foreach (var workCenter in plant.WorkCenters)
            {
                var scheduling = workCenter.SchedulingProfile;
                var transitions = workCenter.SetupTransitionProfile;

                if (scheduling is null ||
                    transitions is null)
                {
                    continue;
                }

                foreach (var changeover in transitions.Changeovers)
                {
                    for (int period = 1;
                         period <= instance.PlanningHorizon;
                         period++)
                    {
                        for (int microPeriod = 1;
                             microPeriod <= scheduling.MicroPeriodsPerPeriod;
                             microPeriod++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            if (period == 1 &&
                                microPeriod == 1)
                            {
                                continue;
                            }

                            int previousPeriod = period;
                            int previousMicroPeriod = microPeriod - 1;

                            if (microPeriod == 1)
                            {
                                if (transitions.CarryOverPolicy !=
                                    SetupCarryOverPolicy.Allowed)
                                {
                                    continue;
                                }

                                previousPeriod = period - 1;
                                previousMicroPeriod =
                                    scheduling.MicroPeriodsPerPeriod;

                                if (previousPeriod <= 0)
                                {
                                    continue;
                                }
                            }

                            string previousStateKey =
                                SchedulingVariableKeyFactory.SetupState(
                                    plant.Id,
                                    workCenter.Id,
                                    changeover.FromItemId,
                                    previousPeriod,
                                    previousMicroPeriod);

                            string currentStateKey =
                                SchedulingVariableKeyFactory.SetupState(
                                    plant.Id,
                                    workCenter.Id,
                                    changeover.ToItemId,
                                    period,
                                    microPeriod);

                            string changeoverKey =
                                SchedulingVariableKeyFactory.Changeover(
                                    plant.Id,
                                    workCenter.Id,
                                    changeover.FromItemId,
                                    changeover.ToItemId,
                                    period,
                                    microPeriod);

                            var expression =
                                new LinearExpressionBuilder();

                            expression.Add(
                                context.GetVariable(previousStateKey),
                                1.0);

                            expression.Add(
                                context.GetVariable(currentStateKey),
                                1.0);

                            expression.Add(
                                context.GetVariable(changeoverKey),
                                -1.0);

                            AddConstraint(
                                context,
                                $"changeoverDef_p{plant.Id}_w{workCenter.Id}_i{changeover.FromItemId}_j{changeover.ToItemId}_t{period}_s{microPeriod}",
                                expression.Build(),
                                MathematicalConstraintSense.LessThanOrEqual,
                                1.0,
                                description:
                                    "Sequence-dependent changeover lower bound.");
                        }
                    }
                }
            }
        }

        return ValueTask.CompletedTask;
    }
}
