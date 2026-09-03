namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Creates preconfigured mathematical decision-mapper
/// registries for the standard lot-sizing solution model.
/// </summary>
public static class MathematicalDecisionMapperRegistryFactory
{
    /// <summary>
    /// Creates the default registry containing all standard
    /// business-decision mappers and internal auxiliary-variable
    /// consumers.
    /// </summary>
    public static MathematicalDecisionMapperRegistry CreateDefault()
    {
        var registry =
            new MathematicalDecisionMapperRegistry();

        registry.Register(new ProductionDecisionMapper());
        registry.Register(new SetupDecisionMapper());
        registry.Register(new InventoryDecisionMapper());
        registry.Register(new InventorySetupDecisionMapper());
        registry.Register(
            new InventorySafetyStockViolationDecisionMapper());
        registry.Register(
            new InventoryAdditionalCapacityDecisionMapper());
        registry.Register(new DeliveryDecisionMapper());
        registry.Register(new BacklogDecisionMapper());
        registry.Register(new ShortageDecisionMapper());
        registry.Register(new TransportDecisionMapper());
        registry.Register(new TransportSetupDecisionMapper());
        registry.Register(
            new TransportAdditionalCapacityDecisionMapper());
        registry.Register(new ProcurementDecisionMapper());
        registry.Register(new WorkCenterActivationDecisionMapper());
        registry.Register(
            new WorkCenterAdditionalCapacityDecisionMapper());
        registry.Register(new WarehouseActivationDecisionMapper());
        registry.Register(
            new WarehouseAdditionalCapacityDecisionMapper());
        registry.Register(
            new TransportResourceActivationDecisionMapper());
        registry.Register(
            new TransportResourceAdditionalCapacityDecisionMapper());

        // Mathematical-only variables are consumed but not
        // written to LotSizingSolution.
        registry.Register(
            new AuxiliaryLotSizeMultiplierDecisionMapper());

        registry.Register(
            new AuxiliaryProductionFamilySetupDecisionMapper());

        return registry;
    }
}
