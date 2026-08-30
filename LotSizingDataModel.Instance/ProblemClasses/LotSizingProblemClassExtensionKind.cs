namespace LotSizingDataModel.Instance.ProblemClasses;

/// <summary>
/// Modeled characteristics outside the canonical core signatures.
/// </summary>
public enum LotSizingProblemClassExtensionKind
{
    InitialInventory,
    SafetyStock,
    Backlogging,
    LostSales,

    SetupTimes,
    StartUpCosts,
    StartUpTimes,
    ProductionLeadTimes,

    MinimumLotSize,
    MaximumLotSize,
    LotSizeMultiple,

    AdditionalProductionCapacity,
    AdditionalWarehouseCapacity,
    AdditionalTransportCapacity,

    Purchasing,
    SupplierCapacity,
    SupplierLeadTime,

    Transportation,
    TransportCapacity,
    TransportLeadTime,
    Distribution,
    WarehouseCapacity,

    MultiSite,
    FinancialConstraints,
    MultipleObjectives,

    IntegratedScheduling,
    BigBucketScheduling,
    SmallBucketScheduling,
    MacroMicroScheduling,
    InitialSetupState,
    SetupCarryOver,
    SequenceDependentChangeoverTimes,
    SequenceDependentChangeoverCosts,
    MaximumSetupCount
}
