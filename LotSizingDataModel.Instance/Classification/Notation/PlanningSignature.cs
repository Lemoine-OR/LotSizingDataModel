using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Instance.Classification.Notation;

/// <summary>
/// LSI planning and information block (pi).
/// </summary>
[Serializable]
[XmlType(TypeName = "lsiPlanningSignature")]
public sealed class PlanningSignature : ModelObject
{
    private PlanningHorizonKind _horizon =
        PlanningHorizonKind.Unknown;
    private TimeModelKind _timeModel =
        TimeModelKind.Unknown;
    private BucketStructureKind _bucketStructure =
        BucketStructureKind.Unknown;
    private InformationStructureKind _information =
        InformationStructureKind.Unknown;
    private DemandPatternKind _demandPattern =
        DemandPatternKind.Unknown;
    private DemandSourceKind _demandSource =
        DemandSourceKind.Unknown;

    [XmlAttribute("horizon")]
    public PlanningHorizonKind Horizon
    {
        get => _horizon;
        set => SetProperty(ref _horizon, value);
    }

    [XmlAttribute("timeModel")]
    public TimeModelKind TimeModel
    {
        get => _timeModel;
        set => SetProperty(ref _timeModel, value);
    }

    [XmlAttribute("bucketStructure")]
    public BucketStructureKind BucketStructure
    {
        get => _bucketStructure;
        set => SetProperty(ref _bucketStructure, value);
    }

    [XmlAttribute("information")]
    public InformationStructureKind Information
    {
        get => _information;
        set => SetProperty(ref _information, value);
    }

    [XmlAttribute("demandPattern")]
    public DemandPatternKind DemandPattern
    {
        get => _demandPattern;
        set => SetProperty(ref _demandPattern, value);
    }

    [XmlAttribute("demandSource")]
    public DemandSourceKind DemandSource
    {
        get => _demandSource;
        set => SetProperty(ref _demandSource, value);
    }
}
