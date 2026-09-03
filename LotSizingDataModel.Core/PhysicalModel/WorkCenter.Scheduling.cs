using System.Xml.Serialization;
using LotSizingDataModel.Core.DecisionModel.Scheduling;

namespace LotSizingDataModel.Core.PhysicalModel;

public sealed partial class WorkCenter
{
    private ProductionSchedulingProfile? _schedulingProfile;

    [XmlElement("schedulingProfile")]
    public ProductionSchedulingProfile? SchedulingProfile
    {
        get => _schedulingProfile;
        set => SetDecisionParameter(
            ref _schedulingProfile,
            value,
            nameof(SchedulingProfile));
    }
}
