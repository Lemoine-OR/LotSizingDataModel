using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Solution.Decisions;

namespace LotSizingDataModel.Solution;

public sealed partial class LotSizingSolution
{
    [XmlArray("closedLoopDecisions")]
    [XmlArrayItem("closedLoopDecision")]
    public List<ClosedLoopDecision> ClosedLoopDecisions
    {
        get;
    } = new();

    [XmlIgnore]
    public int ClosedLoopDecisionCount =>
        ClosedLoopDecisions.Count;

    [XmlIgnore]
    public bool HasValidClosedLoopDecisions =>
        ClosedLoopDecisions.All(
            decision =>
                decision is not null &&
                decision.IsInternallyValid &&
                decision.PlanningHorizon ==
                    PlanningHorizon) &&
        ClosedLoopDecisions
            .Select(
                decision =>
                    decision.ReturnStreamId)
            .Distinct()
            .Count() ==
        ClosedLoopDecisions.Count;

    public ClosedLoopDecision? FindClosedLoopDecision(
        int returnStreamId)
    {
        return ClosedLoopDecisions.FirstOrDefault(
            decision =>
                decision.ReturnStreamId ==
                returnStreamId);
    }

    public void ResizeClosedLoopDecisions()
    {
        foreach (ClosedLoopDecision decision
                 in ClosedLoopDecisions)
        {
            decision.ResizeTimeSeries(
                PlanningHorizon);
        }
    }
}
