namespace LotSizingDataModel.Instance.Notation;

/// <summary>Canonical item-cardinality token.</summary>
public enum UniversalItemCardinality
{
    Unknown,
    Single,
    Multiple
}

/// <summary>Canonical BOM-level token.</summary>
public enum UniversalProblemLevel
{
    Unknown,
    SingleLevel,
    MultiLevel
}

/// <summary>
/// Objective family represented in the gamma field of notation v1.
/// </summary>
public enum UniversalObjectiveKind
{
    Unknown,
    Economic,
    MultipleObjectives
}

/// <summary>
/// Feature vocabulary represented in the beta field of notation v1.
/// Enum order is the canonical rendering order.
/// </summary>
public enum UniversalNotationFeature
{
    Demand = 0,
    DeterministicDemand = 1,
    TimeVaryingDemand = 2,
    Production = 3,

    /// <summary>Production exists without a production-capacity constraint.</summary>
    UncapacitatedProduction = 9,

    ProductionCapacity = 10,
    SharedProductionCapacity = 11,
    TimeVaryingProductionCapacity = 12,
    SupplierCapacity = 13,
    TransportCapacity = 14,
    WarehouseCapacity = 15,

    SetupCost = 20,
    SetupTime = 21,
    StartUpCost = 22,
    ProductionLeadTime = 23,

    MinimumLotSize = 30,
    MaximumLotSize = 31,
    LotSizeMultiple = 32,

    AdditionalProductionCapacity = 40,
    AdditionalWarehouseCapacity = 41,
    AdditionalTransportCapacity = 42,

    InitialInventory = 50,
    SafetyStock = 51,
    Backlogging = 52,
    LostSales = 53,

    Purchasing = 60,
    SupplierLeadTime = 61,
    Transportation = 62,
    TransportLeadTime = 63,
    Distribution = 64,

    FinancialConstraint = 70
}
