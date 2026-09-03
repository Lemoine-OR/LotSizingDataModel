using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Instance.Classification.Notation;

[Serializable]
[XmlType(TypeName = "lsiObjectiveSenseKind")]
public enum ObjectiveSenseKind
{
    Unknown = 0,
    Minimize = 1,
    Maximize = 2
}

[Serializable]
[XmlType(TypeName = "lsiObjectiveAggregationKind")]
public enum ObjectiveAggregationKind
{
    Unknown = 0,
    Single = 1,
    WeightedSum = 2,
    Lexicographic = 3,
    Pareto = 4,
    EpsilonConstraint = 5,
    Other = 6
}

[Serializable]
[XmlType(TypeName = "lsiObjectiveComponentKind")]
public enum ObjectiveComponentKind
{
    Unknown = 0,
    TotalCost = 1,
    ProductionCost = 2,
    SetupCost = 3,
    HoldingCost = 4,
    BacklogCost = 5,
    LostSalesCost = 6,
    PurchasingCost = 7,
    TransportationCost = 8,
    CapacityCost = 9,
    Revenue = 10,
    Profit = 11,
    ServiceLevel = 12,
    EnvironmentalImpact = 13,
    Other = 14,

    // Business-level objective families represented by Core.
    Economic = 20,
    Financial = 21,
    Sustainability = 22
}
