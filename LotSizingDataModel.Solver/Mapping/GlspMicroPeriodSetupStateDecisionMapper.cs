using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Formulation;

namespace LotSizingDataModel.Solver.Mapping;

public sealed class GlspMicroPeriodSetupStateDecisionMapper : MathematicalDecisionMapperBase
{
    public override string Category => MathematicalDecisionCategory.MicroPeriodSetupState;

    protected override void MapValue(
        MathematicalSolutionMappingContext context,
        LotSizingSolution solution,
        MathematicalDomainKey domainKey,
        MathematicalVariableValue variableValue)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(domainKey);
        ArgumentNullException.ThrowIfNull(variableValue);

        if (variableValue.Value < 0.5) return;

        int plantId = domainKey.GetRequiredInt32(MathematicalDomainKeySegment.Plant);
        int workCenterId = domainKey.GetRequiredInt32(MathematicalDomainKeySegment.WorkCenter);
        int routingId = domainKey.GetRequiredInt32(MathematicalDomainKeySegment.Routing);
        int itemId = domainKey.GetRequiredInt32(MathematicalDomainKeySegment.Item);
        int period = domainKey.GetRequiredInt32(MathematicalDomainKeySegment.Period);
        int microPeriodIndex = domainKey.GetRequiredInt32(MathematicalDomainKeySegment.MicroPeriod);

        WorkCenterSchedulingDecision? schedule =
            solution.WorkCenterSchedulingDecisions.SingleOrDefault(existing =>
                existing.WorkCenter.PlantId == plantId &&
                existing.WorkCenter.WorkCenterId == workCenterId);

        if (schedule is null)
        {
            schedule = new WorkCenterSchedulingDecision(
                new WorkCenterReference(plantId, workCenterId),
                solution.PlanningHorizon);
            solution.AddWorkCenterSchedulingDecision(schedule);
        }

        ProductionMicroPeriodDecision? decision =
            schedule.MicroPeriods.SingleOrDefault(existing =>
                existing.MicroPeriod.MacroPeriod == period &&
                existing.MicroPeriod.MicroPeriodIndex == microPeriodIndex);

        if (decision is null)
        {
            decision = new ProductionMicroPeriodDecision(
                new ProductionMicroPeriodReference(period, microPeriodIndex),
                setupItemId: itemId);
            schedule.AddMicroPeriodDecision(decision);
        }
        else if (decision.SetupItemId != itemId)
        {
            throw new InvalidOperationException(
                "Two active GLSP setup states were mapped to the same micro-period.");
        }

        string productionKey =
            GlspFormulationVariableKeyFactory.CreateMicroProductionKey(
                plantId, workCenterId, routingId, itemId, decision.MicroPeriod);

        if (context.TryGetValue(productionKey, out MathematicalVariableValue? productionValue) &&
            productionValue is not null)
        {
            double quantity =
                Math.Abs(productionValue.Value) <= context.Options.ZeroTolerance
                    ? 0.0
                    : productionValue.Value;

            if (quantity > 0.0)
            {
                decision.RoutingId = routingId;
                decision.Quantity = quantity;
            }
        }
    }
}
