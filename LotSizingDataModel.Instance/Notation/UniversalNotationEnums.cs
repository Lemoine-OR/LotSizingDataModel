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
    Unknown = 0,
    Economic = 1,
    MultipleObjectives = 2,
    Financial = 3,
    Sustainability = 4,
    ServiceLevel = 5
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
    StartUpTime = 24,

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

    FinancialConstraint = 70,

    IntegratedScheduling = 80,
    BigBucketScheduling = 81,
    SmallBucketScheduling = 82,
    MacroMicroScheduling = 83,
    InitialSetupState = 84,
    SetupCarryOver = 85,
    SequenceDependentChangeoverTime = 86,
    SequenceDependentChangeoverCost = 87,
    MaximumSetupCount = 88,

    SingleSchedulingResource = 89,
    SmallBucketAllOrNothingProduction = 90,
    SmallBucketContinuousProduction = 91,
    AtMostOneProducedItemPerBucket = 92,
    AtMostTwoProducedItemsPerBucket = 93,
    AtMostOneSetupTransitionPerBucket = 94,

    ExplicitMicroPeriodGrid = 95,
    VariableLengthMicroPeriods = 96,
    FixedLengthMicroPeriods = 97,
    SingleItemPerMicroPeriod = 98,
    MultipleItemsPerMicroPeriod = 99,
    VariableMicroPeriodCount = 100,
    SetupCarryOverForbidden = 101,
    GroupingConstraint = 102
}
