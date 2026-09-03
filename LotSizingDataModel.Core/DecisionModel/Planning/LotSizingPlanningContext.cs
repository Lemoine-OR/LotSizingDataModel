using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.DecisionModel.Planning;

/// <summary>
/// Explicit planning semantics that cannot safely be inferred
/// from ordinary period-indexed data.
/// </summary>
[Serializable]
[XmlType(TypeName = "lotSizingPlanningContext")]
public sealed class LotSizingPlanningContext : ModelObject
{
    private PlanningBucketMode _bucketMode =
        PlanningBucketMode.Unspecified;

    [XmlAttribute("bucketMode")]
    public PlanningBucketMode BucketMode
    {
        get => _bucketMode;
        set => SetProperty(ref _bucketMode, value);
    }
}
