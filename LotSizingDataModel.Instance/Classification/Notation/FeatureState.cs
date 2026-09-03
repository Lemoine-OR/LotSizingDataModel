using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Instance.Classification.Notation;

/// <summary>
/// Describes whether a semantic feature is present, absent,
/// unknown, not applicable or heterogeneous.
/// </summary>
[Serializable]
[XmlType(TypeName = "lsiFeatureState")]
public enum FeatureState
{
    Unknown = 0,
    Absent = 1,
    Present = 2,
    NotApplicable = 3,
    Mixed = 4
}
