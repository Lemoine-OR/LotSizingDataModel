using System.Xml.Serialization;
using LotSizingDataModel.Core.DecisionModel.Scheduling;

namespace LotSizingDataModel.Core.PhysicalModel;

/// <summary>
/// Adds optional setup-transition semantics to a work center.
/// </summary>
public sealed partial class WorkCenter
{
    private ProductionSetupTransitionProfile? _setupTransitionProfile;

    [XmlElement("setupTransitionProfile")]
    public ProductionSetupTransitionProfile? SetupTransitionProfile
    {
        get => _setupTransitionProfile;
        set => SetDecisionParameter(
            ref _setupTransitionProfile,
            value,
            nameof(SetupTransitionProfile));
    }
}
