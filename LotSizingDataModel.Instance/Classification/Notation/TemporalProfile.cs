using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Instance.Classification.Notation;

/// <summary>
/// Stores a classified temporal profile and, for mixed
/// profiles, the distinct component profiles.
/// </summary>
[Serializable]
[XmlType(TypeName = "lsiTemporalProfile")]
public sealed class TemporalProfile : ModelObject
{
    private TemporalProfileKind _kind = TemporalProfileKind.Unknown;

    [XmlAttribute("kind")]
    public TemporalProfileKind Kind
    {
        get => _kind;
        set => SetProperty(ref _kind, value);
    }

    [XmlArray("components")]
    [XmlArrayItem("profile")]
    public List<TemporalProfileKind> Components { get; } = new();

    [XmlIgnore]
    public bool IsKnown =>
        Kind != TemporalProfileKind.Unknown;

    public void ReplaceComponents(
        IEnumerable<TemporalProfileKind>? components)
    {
        Components.Clear();

        if (components is null)
        {
            return;
        }

        foreach (TemporalProfileKind component in
                 components
                     .Where(value =>
                         value != TemporalProfileKind.Unknown)
                     .Distinct()
                     .OrderBy(value => (int)value))
        {
            Components.Add(component);
        }
    }
}
