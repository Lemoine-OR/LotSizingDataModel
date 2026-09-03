using System.Xml.Serialization;

namespace LotSizingDataModel.Instance.Classification;

public sealed partial class LotSizingProblemFeatures
{
    [XmlAttribute("hasProductionSetupFamilies")]
    public bool HasProductionSetupFamilies { get; set; }

    [XmlAttribute("hasProductionSetupFamilyTimes")]
    public bool HasProductionSetupFamilyTimes { get; set; }
}
