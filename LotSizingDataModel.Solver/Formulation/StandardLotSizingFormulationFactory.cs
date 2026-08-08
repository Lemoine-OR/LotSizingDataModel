using System;
using System.Collections.Generic;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Creates fully configured instances of the standard
/// solver-independent lot-sizing formulation.
/// </summary>
public static class StandardLotSizingFormulationFactory
{
    /// <summary>
    /// Creates the standard formulation with the default
    /// formulation options.
    /// </summary>
    /// <returns>
    /// Fully configured standard lot-sizing formulation.
    /// </returns>
    public static StandardLotSizingFormulation CreateDefault()
    {
        return Create(
            new StandardLotSizingFormulationOptions());
    }

    /// <summary>
    /// Creates the standard formulation with the supplied
    /// formulation options.
    /// </summary>
    /// <param name="options">
    /// Formulation options.
    /// </param>
    /// <returns>
    /// Fully configured standard lot-sizing formulation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static StandardLotSizingFormulation Create(
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        StandardLotSizingFormulationOptions normalizedOptions =
            options.Clone();

        normalizedOptions.EnsureValid();

        var variableBuilder =
            new StandardLotSizingVariableBuilder(
                CreateVariableFamilyBuilders());

        var objectiveBuilder =
            new StandardLotSizingObjectiveBuilder(
                CreateObjectiveTermBuilders());

        var constraintBuilder =
            new StandardLotSizingConstraintBuilder(
                CreateConstraintFamilyBuilders());

        return new StandardLotSizingFormulation(
            variableBuilder,
            objectiveBuilder,
            constraintBuilder,
            normalizedOptions);
    }

    /// <summary>
    /// Creates the ordered standard variable-family builders.
    /// </summary>
    /// <returns>
    /// Ordered variable-family builder collection.
    /// </returns>
    public static IReadOnlyList<IStandardLotSizingVariableFamilyBuilder>
        CreateVariableFamilyBuilders()
    {
        return
        [
            new ProductionVariableFamilyBuilder(),
            new SetupVariableFamilyBuilder(),
            new InventoryVariableFamilyBuilder(),
            new InventorySetupVariableFamilyBuilder(),
            new InventorySafetyStockViolationVariableFamilyBuilder(),
            new InventoryAdditionalCapacityVariableFamilyBuilder(),
            new DeliveryVariableFamilyBuilder(),
            new BacklogVariableFamilyBuilder(),
            new ShortageVariableFamilyBuilder(),
            new ProcurementVariableFamilyBuilder(),
            new TransportVariableFamilyBuilder(),
            new TransportSetupVariableFamilyBuilder(),
            new TransportAdditionalCapacityVariableFamilyBuilder(),
            new WorkCenterActivationVariableFamilyBuilder(),
            new WorkCenterAdditionalCapacityVariableFamilyBuilder(),
            new WarehouseActivationVariableFamilyBuilder(),
            new WarehouseAdditionalCapacityVariableFamilyBuilder(),
            new TransportResourceActivationVariableFamilyBuilder(),
            new TransportResourceAdditionalCapacityVariableFamilyBuilder(),
            new LotSizeMultipleVariableFamilyBuilder()
        ];
    }

    /// <summary>
    /// Creates the ordered standard objective-term builders.
    /// </summary>
    /// <returns>
    /// Ordered objective-term builder collection.
    /// </returns>
    public static IReadOnlyList<IStandardLotSizingObjectiveTermBuilder>
        CreateObjectiveTermBuilders()
    {
        return
        [
            new ProductionCostObjectiveTermBuilder(),
            new ProductionSetupCostObjectiveTermBuilder(),
            new InventoryCostObjectiveTermBuilder(),
            new InventorySetupCostObjectiveTermBuilder(),
            new InventorySafetyStockViolationCostObjectiveTermBuilder(),
            new InventoryAdditionalCapacityCostObjectiveTermBuilder(),
            new DeliveryRevenueObjectiveTermBuilder(),
            new BacklogCostObjectiveTermBuilder(),
            new ShortageCostObjectiveTermBuilder(),
            new ProcurementCostObjectiveTermBuilder(),
            new TransportCostObjectiveTermBuilder(),
            new TransportSetupCostObjectiveTermBuilder(),
            new TransportAdditionalCapacityCostObjectiveTermBuilder(),
            new WorkCenterResourceCostObjectiveTermBuilder(),
            new WorkCenterAdditionalCapacityCostObjectiveTermBuilder(),
            new WarehouseResourceCostObjectiveTermBuilder(),
            new WarehouseAdditionalCapacityCostObjectiveTermBuilder(),
            new TransportResourceCostObjectiveTermBuilder(),
            new TransportResourceAdditionalCapacityCostObjectiveTermBuilder()
        ];
    }

    /// <summary>
    /// Creates the ordered standard constraint-family builders.
    /// </summary>
    /// <remarks>
    /// Lot-sizing restrictions are created first, followed by
    /// physical balances, capacities, and resource-activation
    /// links. The ordering makes generated models deterministic
    /// and easier to inspect.
    /// </remarks>
    /// <returns>
    /// Ordered constraint-family builder collection.
    /// </returns>
    public static IReadOnlyList<IStandardLotSizingConstraintFamilyBuilder>
        CreateConstraintFamilyBuilders()
    {
        return
        [
            new ProductionSetupLinkConstraintFamilyBuilder(),
            new MinimumLotSizeConstraintFamilyBuilder(),
            new LotSizeMultipleConstraintFamilyBuilder(),
            new GroupingConstraintFamilyBuilder(),

            new SafetyStockConstraintFamilyBuilder(),

            new InventoryBalanceConstraintFamilyBuilder(),
            new DemandSatisfactionConstraintFamilyBuilder(),

            new InventoryCapacityConstraintFamilyBuilder(),
            new WorkCenterCapacityConstraintFamilyBuilder(),
            new WarehouseCapacityConstraintFamilyBuilder(),
            new TransportSpecificCapacityConstraintFamilyBuilder(),
            new TransportResourceCapacityConstraintFamilyBuilder(),

            new WorkCenterActivationLinkConstraintFamilyBuilder(),
            new WarehouseActivationLinkConstraintFamilyBuilder(),
            new TransportResourceActivationLinkConstraintFamilyBuilder()
        ];
    }
}
