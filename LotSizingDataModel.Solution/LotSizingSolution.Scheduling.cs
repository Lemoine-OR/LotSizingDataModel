using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Solution.Decisions;

namespace LotSizingDataModel.Solution;

public sealed partial class LotSizingSolution
{
    [XmlArray("workCenterSchedulingDecisions")]
    [XmlArrayItem("workCenterSchedulingDecision")]
    public List<WorkCenterSchedulingDecision>
        WorkCenterSchedulingDecisions { get; } = new();

    public void AddWorkCenterSchedulingDecision(
        WorkCenterSchedulingDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        PrepareDecision(
            decision.PlanningHorizon,
            decision.ResizeTimeSeries,
            nameof(decision));

        if (!decision.IsInternallyValid)
            throw new ArgumentException(
                "The work-center scheduling decision is not internally valid.",
                nameof(decision));

        if (WorkCenterSchedulingDecisions.Any(existing =>
                existing.WorkCenter.RefersToSameWorkCenter(
                    decision.WorkCenter)))
            throw new InvalidOperationException(
                "A scheduling decision already exists for this work center.");

        WorkCenterSchedulingDecisions.Add(decision);
        SubscribeToObject(decision);
        NotifyDecisionProperties();
    }
}
