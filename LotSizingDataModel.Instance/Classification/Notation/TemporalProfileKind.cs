using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Instance.Classification.Notation;

/// <summary>
/// Describes the temporal profile of a numerical parameter.
/// </summary>
[Serializable]
[XmlType(TypeName = "lsiTemporalProfileKind")]
public enum TemporalProfileKind
{
    Unknown = 0,
    Zero = 1,
    Constant = 2,
    NonIncreasing = 3,
    NonDecreasing = 4,
    General = 5,
    Periodic = 6,
    Mixed = 7,
    NotApplicable = 8
}
