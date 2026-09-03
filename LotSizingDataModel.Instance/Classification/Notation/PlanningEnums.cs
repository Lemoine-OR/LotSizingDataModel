using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Instance.Classification.Notation;

[Serializable]
[XmlType(TypeName = "lsiPlanningHorizonKind")]
public enum PlanningHorizonKind
{
    Unknown = 0,
    Finite = 1,
    Infinite = 2,
    Rolling = 3
}

[Serializable]
[XmlType(TypeName = "lsiTimeModelKind")]
public enum TimeModelKind
{
    Unknown = 0,
    Discrete = 1,
    Continuous = 2,
    Hybrid = 3
}

[Serializable]
[XmlType(TypeName = "lsiBucketStructureKind")]
public enum BucketStructureKind
{
    Unknown = 0,
    NotApplicable = 1,
    BigBucket = 2,
    SmallBucket = 3,
    Hybrid = 4,
    MacroMicro = 5
}

[Serializable]
[XmlType(TypeName = "lsiInformationStructureKind")]
public enum InformationStructureKind
{
    Unknown = 0,
    Deterministic = 1,
    Stochastic = 2,
    Robust = 3,
    Fuzzy = 4,
    Hybrid = 5
}

[Serializable]
[XmlType(TypeName = "lsiDemandPatternKind")]
public enum DemandPatternKind
{
    Unknown = 0,
    Stationary = 1,
    Dynamic = 2,
    Endogenous = 3,
    Mixed = 4
}

[Serializable]
[XmlType(TypeName = "lsiDemandSourceKind")]
public enum DemandSourceKind
{
    Unknown = 0,
    Exogenous = 1,
    Endogenous = 2,
    Mixed = 3
}
