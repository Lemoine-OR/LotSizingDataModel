using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Instance.Classification.Notation;

[Serializable]
[XmlType(TypeName = "lsiCardinalityKind")]
public enum CardinalityKind
{
    Unknown = 0,
    None = 1,
    Single = 2,
    Multiple = 3
}

[Serializable]
[XmlType(TypeName = "lsiNetworkStructureKind")]
public enum NetworkStructureKind
{
    Unknown = 0,
    SingleSite = 1,
    MultiSite = 2,
    SupplyChain = 3
}

[Serializable]
[XmlType(TypeName = "lsiRoutingStructureKind")]
public enum RoutingStructureKind
{
    Unknown = 0,
    Dedicated = 1,
    Alternative = 2,
    General = 3,
    Mixed = 4
}

[Serializable]
[XmlType(TypeName = "lsiResourceEnvironmentKind")]
public enum ResourceEnvironmentKind
{
    Unknown = 0,
    SingleResource = 1,
    Parallel = 2,
    Flow = 3,
    Job = 4,
    Open = 5,
    Flexible = 6,
    General = 7,
    Mixed = 8
}
