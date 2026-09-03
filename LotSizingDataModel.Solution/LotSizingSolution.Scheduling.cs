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
        WorkCenterSchedulingDecisions
    {
        get;
    } = new();

    public void ResizeSchedulingDecisions(int periodCount)
    {
        foreach (WorkCenterSchedulingDecision decision
                 in WorkCenterSchedulingDecisions)
        {
            decision.ResizeTimeSeries(periodCount);
        }
    }

    public bool HasConsistentSchedulingHorizon =>
        WorkCenterSchedulingDecisions.All(
            decision =>
                decision.PlanningHorizon ==
                PlanningHorizon);
}
