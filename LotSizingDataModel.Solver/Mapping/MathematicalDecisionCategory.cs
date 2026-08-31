namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Provides the canonical mathematical domain-key categories
/// used to identify lot-sizing decision families.
/// </summary>
public static class MathematicalDecisionCategory
{
    /// <summary>
    /// Identifies production-quantity decision variables.
    /// </summary>
    public const string Production = "production";

    /// <summary>
    /// Identifies production setup decision variables.
    /// </summary>
    public const string Setup = "setup";

    /// <summary>
    /// Identifies inventory-level decision variables.
    /// </summary>
    public const string Inventory = "inventory";

    public const string InitialInventory = "initialInventory";

    /// <summary>
    /// Identifies inventory setup decision variables.
    /// </summary>
    public const string InventorySetup = "inventorySetup";

    /// <summary>
    /// Identifies safety-stock violation decision variables.
    /// </summary>
    public const string InventorySafetyStockViolation =
        "inventorySafetyStockViolation";

    /// <summary>
    /// Identifies additional inventory-capacity decision
    /// variables.
    /// </summary>
    public const string InventoryAdditionalCapacity =
        "inventoryAdditionalCapacity";

    /// <summary>
    /// Identifies delivered-quantity decision variables.
    /// </summary>
    public const string Delivery = "delivery";

    /// <summary>
    /// Identifies backlog-level decision variables.
    /// </summary>
    public const string Backlog = "backlog";

    /// <summary>
    /// Identifies shortage or lost-sales decision variables.
    /// </summary>
    public const string Shortage = "shortage";

    /// <summary>
    /// Identifies transported-quantity decision variables.
    /// </summary>
    public const string Transport = "transport";

    /// <summary>
    /// Identifies transport setup decision variables.
    /// </summary>
    public const string TransportSetup = "transportSetup";

    /// <summary>
    /// Identifies item-specific additional transport-capacity
    /// decision variables.
    /// </summary>
    public const string TransportAdditionalCapacity =
        "transportAdditionalCapacity";

    /// <summary>
    /// Identifies supplier-procurement decision variables.
    /// </summary>
    public const string Procurement = "procurement";

    /// <summary>
    /// Identifies subcontracting decision variables.
    /// </summary>
    public const string Subcontracting = "subcontracting";

    /// <summary>
    /// Identifies work-center activation decision variables.
    /// </summary>
    public const string WorkCenterActivation =
        "workCenterActivation";

    /// <summary>
    /// Identifies additional work-center-capacity decision
    /// variables.
    /// </summary>
    public const string WorkCenterAdditionalCapacity =
        "workCenterAdditionalCapacity";

    /// <summary>
    /// Identifies warehouse activation decision variables.
    /// </summary>
    public const string WarehouseActivation =
        "warehouseActivation";

    /// <summary>
    /// Identifies additional warehouse-capacity decision
    /// variables.
    /// </summary>
    public const string WarehouseAdditionalCapacity =
        "warehouseAdditionalCapacity";

    /// <summary>
    /// Identifies transport-resource activation decision
    /// variables.
    /// </summary>
    public const string TransportResourceActivation =
        "transportResourceActivation";

    /// <summary>
    /// Identifies additional transport-resource-capacity
    /// decision variables.
    /// </summary>
    public const string TransportResourceAdditionalCapacity =
        "transportResourceAdditionalCapacity";

    /// <summary>
    /// Identifies internal integer multiplier variables used to
    /// enforce production lot-size multiples.
    /// </summary>
    /// <remarks>
    /// This category is mathematical only and is intentionally
    /// not persisted as a business decision in the normalized
    /// lot-sizing solution.
    /// </remarks>
    public const string AuxiliaryLotSizeMultiplier =
        "auxiliaryLotSizeMultiplier";

    /// <summary>
    /// Mathematical-only DLSP full-bucket production activation.
    /// </summary>
    public const string AuxiliarySmallBucketProductionActivation =
        "auxiliarySmallBucketProductionActivation";

    /// <summary>
    /// Mathematical-only start of a persistent scheduling setup state.
    /// </summary>
    public const string AuxiliarySchedulingSetupStart =
        "auxiliarySchedulingSetupStart";

    public const string MicroPeriodProduction =
        "microPeriodProduction";

    public const string MicroPeriodSetupState =
        "microPeriodSetupState";

    public const string AuxiliaryMicroPeriodChangeover =
        "auxiliaryMicroPeriodChangeover";

    public const string AuxiliaryMicroPeriodSetupStart =
        "auxiliaryMicroPeriodSetupStart";

    public const string AuxiliaryMacroProductionActivation =
        "auxiliaryMacroProductionActivation";

    public const string AuxiliaryProductionStartUp =
        "auxiliaryProductionStartUp";
}
