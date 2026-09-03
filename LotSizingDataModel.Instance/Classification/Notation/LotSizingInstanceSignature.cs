using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Instance.Classification.Notation;

/// <summary>
/// Complete Lot-Sizing Instance (LSI) semantic signature.
/// </summary>
[Serializable]
[XmlType(TypeName = "lotSizingInstanceSignature")]
public sealed class LotSizingInstanceSignature : ModelObject
{
    public const string CurrentNotationVersion = "1.0";

    private string _notationVersion = CurrentNotationVersion;
    private PlanningSignature _planning = new();
    private SystemSignature _system = new();
    private FeatureSignature _features = new();
    private ObjectiveSignature _objective = new();
    private InstanceSizeSignature _size = new();

    [XmlAttribute("notationVersion")]
    public string NotationVersion
    {
        get => _notationVersion;
        set => SetProperty(
            ref _notationVersion,
            string.IsNullOrWhiteSpace(value)
                ? CurrentNotationVersion
                : value.Trim());
    }

    [XmlElement("planning")]
    public PlanningSignature Planning
    {
        get => _planning;
        set => SetProperty(
            ref _planning,
            value ?? new PlanningSignature());
    }

    [XmlElement("system")]
    public SystemSignature System
    {
        get => _system;
        set => SetProperty(
            ref _system,
            value ?? new SystemSignature());
    }

    [XmlElement("features")]
    public FeatureSignature Features
    {
        get => _features;
        set => SetProperty(
            ref _features,
            value ?? new FeatureSignature());
    }

    [XmlElement("objective")]
    public ObjectiveSignature Objective
    {
        get => _objective;
        set => SetProperty(
            ref _objective,
            value ?? new ObjectiveSignature());
    }

    [XmlElement("size")]
    public InstanceSizeSignature Size
    {
        get => _size;
        set => SetProperty(
            ref _size,
            value ?? new InstanceSizeSignature());
    }

    [XmlIgnore]
    public string CanonicalNotation =>
        LotSizingSignatureCanonicalFormatter.Format(this);

    [XmlIgnore]
    public string CompactNotation =>
        LotSizingSignatureCompactFormatter.Format(this);
}
