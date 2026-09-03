using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Instance.Classification.Notation;

/// <summary>
/// LSI objective block (gamma).
/// </summary>
[Serializable]
[XmlType(TypeName = "lsiObjectiveSignature")]
public sealed class ObjectiveSignature : ModelObject
{
    private FeatureState _state = FeatureState.Unknown;
    private ObjectiveSenseKind _sense = ObjectiveSenseKind.Unknown;
    private ObjectiveAggregationKind _aggregation =
        ObjectiveAggregationKind.Unknown;

    [XmlAttribute("state")]
    public FeatureState State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }

    [XmlAttribute("sense")]
    public ObjectiveSenseKind Sense
    {
        get => _sense;
        set => SetProperty(ref _sense, value);
    }

    [XmlAttribute("aggregation")]
    public ObjectiveAggregationKind Aggregation
    {
        get => _aggregation;
        set => SetProperty(ref _aggregation, value);
    }

    [XmlArray("components")]
    [XmlArrayItem("component")]
    public List<ObjectiveComponentKind> Components { get; } =
        new();

    public void ReplaceComponents(
        IEnumerable<ObjectiveComponentKind>? components)
    {
        Components.Clear();

        if (components is null)
        {
            return;
        }

        foreach (ObjectiveComponentKind component in
                 components
                     .Where(value =>
                         value != ObjectiveComponentKind.Unknown)
                     .Distinct()
                     .OrderBy(value => (int)value))
        {
            Components.Add(component);
        }
    }
}
