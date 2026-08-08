using System;
using System.Collections.Generic;
using LotSizingDataModel.Core.Indexing;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Querying;
using LotSizingDataModel.Core.Relationships;

namespace LotSizingDataModel.Core.Analysis;

/// <summary>
/// Calculates the costs and revenues associated with
/// supply-chain activities.
///
/// Global resource costs and item-specific activity costs
/// are calculated separately to prevent global fixed costs
/// from being counted several times.
/// </summary>
public sealed class CostAnalyzer
{
    /// <summary>
    /// Initializes a cost analyzer and creates
    /// a new supply-chain index.
    /// </summary>
    public CostAnalyzer(SupplyChain supplyChain)
        : this(
            new SupplyChainIndex(
                supplyChain ??
                throw new ArgumentNullException(
                    nameof(supplyChain))))
    {
    }

    /// <summary>
    /// Initializes a cost analyzer using an existing index.
    /// </summary>
    public CostAnalyzer(SupplyChainIndex index)
    {
        Index = index ??
            throw new ArgumentNullException(nameof(index));

        SupplyChain = index.SupplyChain;
        Queries = new SupplyChainQueries(index);
    }

    /// <summary>
    /// Gets the analyzed supply chain.
    /// </summary>
    public SupplyChain SupplyChain { get; }

    /// <summary>
    /// Gets the entity index used by the analyzer.
    /// </summary>
    public SupplyChainIndex Index { get; }

    /// <summary>
    /// Gets the query service used to resolve relationships.
    /// </summary>
    public SupplyChainQueries Queries { get; }

    /// <summary>
    /// Rebuilds the underlying entity index.
    /// </summary>
    public void RebuildIndex()
    {
        Index.Rebuild();
    }

    #region Production costs

    /// <summary>
    /// Calculates the global cost of using a work center
    /// during one planning period.
    ///
    /// This cost must be calculated only once for the work
    /// center and period, regardless of the number of items
    /// produced on the work center.
    /// </summary>
    public CostBreakdown CalculateWorkCenterResourceCost(
        WorkCenterReference workCenterReference,
        int period,
        bool resourceActivated,
        double additionalCapacityUsed)
    {
        ArgumentNullException.ThrowIfNull(
            workCenterReference);

        ValidatePeriod(period);

        ValidateQuantity(
            additionalCapacityUsed,
            nameof(additionalCapacityUsed));

        ValidateResourceActivation(
            resourceActivated,
            additionalCapacityUsed);

        WorkCenter workCenter =
            Index.GetRequiredWorkCenter(
                workCenterReference);

        double fixedUsageCost =
            resourceActivated
                ? GetCostValue(
                    workCenter.FixedUsageCost?[period],
                    nameof(workCenter.FixedUsageCost))
                : 0.0;

        double additionalCapacityCost =
            Multiply(
                additionalCapacityUsed,
                GetCostValue(
                    workCenter.AdditionalCapacityCost?[period],
                    nameof(workCenter.AdditionalCapacityCost)),
                "work-center additional-capacity cost");

        return new CostBreakdown(
            fixedUsageCost: fixedUsageCost,
            additionalCapacityCost:
                additionalCapacityCost);
    }

    /// <summary>
    /// Calculates the item-specific production cost on a
    /// work center during one planning period.
    ///
    /// The global work-center cost is not included.
    /// </summary>
    public CostBreakdown CalculateProductionActivityCost(
        int itemId,
        WorkCenterReference workCenterReference,
        int period,
        double productionQuantity,
        bool setupActivated)
    {
        ArgumentNullException.ThrowIfNull(
            workCenterReference);

        ValidatePeriod(period);

        ValidateQuantity(
            productionQuantity,
            nameof(productionQuantity));

        ProductionCharacteristic characteristic =
            Queries.GetRequiredProductionCharacteristic(
                itemId,
                workCenterReference);

        double fixedSetupCost =
            setupActivated
                ? GetCostValue(
                    characteristic.FixedSetupCost?[period],
                    nameof(characteristic.FixedSetupCost))
                : 0.0;

        double unitUsageCost =
            Multiply(
                productionQuantity,
                GetCostValue(
                    characteristic.UnitUsageCost?[period],
                    nameof(characteristic.UnitUsageCost)),
                "production unit-usage cost");

        return new CostBreakdown(
            fixedSetupCost: fixedSetupCost,
            unitUsageCost: unitUsageCost);
    }

    #endregion

    #region Storage costs

    /// <summary>
    /// Calculates the global cost of using a warehouse
    /// during one planning period.
    ///
    /// This cost must be calculated only once for the
    /// warehouse and period.
    /// </summary>
    public CostBreakdown CalculateWarehouseResourceCost(
        WarehouseReference warehouseReference,
        int period,
        bool warehouseActivated,
        double additionalCapacityUsed)
    {
        ArgumentNullException.ThrowIfNull(
            warehouseReference);

        ValidatePeriod(period);

        ValidateQuantity(
            additionalCapacityUsed,
            nameof(additionalCapacityUsed));

        ValidateResourceActivation(
            warehouseActivated,
            additionalCapacityUsed);

        Warehouse warehouse =
            Index.GetRequiredWarehouse(
                warehouseReference);

        double fixedUsageCost =
            warehouseActivated
                ? GetCostValue(
                    warehouse.FixedUsageCost?[period],
                    nameof(warehouse.FixedUsageCost))
                : 0.0;

        double additionalCapacityCost =
            Multiply(
                additionalCapacityUsed,
                GetCostValue(
                    warehouse.AdditionalCapacityCost?[period],
                    nameof(warehouse.AdditionalCapacityCost)),
                "warehouse additional-capacity cost");

        return new CostBreakdown(
            fixedUsageCost: fixedUsageCost,
            additionalCapacityCost:
                additionalCapacityCost);
    }

    /// <summary>
    /// Calculates the item-specific storage cost during
    /// one planning period.
    ///
    /// The global warehouse cost is not included.
    /// </summary>
    public CostBreakdown CalculateInventoryActivityCost(
        int itemId,
        WarehouseReference warehouseReference,
        int period,
        double inventoryQuantity,
        bool setupActivated,
        double itemAdditionalCapacityUsed,
        double safetyStockViolationQuantity)
    {
        ArgumentNullException.ThrowIfNull(
            warehouseReference);

        ValidatePeriod(period);

        ValidateQuantity(
            inventoryQuantity,
            nameof(inventoryQuantity));

        ValidateQuantity(
            itemAdditionalCapacityUsed,
            nameof(itemAdditionalCapacityUsed));

        ValidateQuantity(
            safetyStockViolationQuantity,
            nameof(safetyStockViolationQuantity));

        Inventory inventory =
            Queries.GetRequiredInventory(
                itemId,
                warehouseReference);

        double fixedSetupCost =
            setupActivated
                ? GetCostValue(
                    inventory.FixedSetupCost?[period],
                    nameof(inventory.FixedSetupCost))
                : 0.0;

        double unitUsageCost =
            Multiply(
                inventoryQuantity,
                GetCostValue(
                    inventory.UnitUsageCost?[period],
                    nameof(inventory.UnitUsageCost)),
                "inventory unit-usage cost");

        double additionalCapacityCost =
            Multiply(
                itemAdditionalCapacityUsed,
                GetCostValue(
                    inventory.AdditionalCapacityCost?[period],
                    nameof(inventory.AdditionalCapacityCost)),
                "item-specific storage additional-capacity cost");

        double safetyStockViolationCost =
            Multiply(
                safetyStockViolationQuantity,
                GetCostValue(
                    inventory.SafetyStockViolationCost?[period],
                    nameof(inventory.SafetyStockViolationCost)),
                "safety-stock violation cost");

        return new CostBreakdown(
            additionalCapacityCost:
                additionalCapacityCost,
            fixedSetupCost: fixedSetupCost,
            unitUsageCost: unitUsageCost,
            safetyStockViolationCost:
                safetyStockViolationCost);
    }

    #endregion

    #region Transport costs

    /// <summary>
    /// Calculates the global cost of using a transport
    /// resource during one planning period.
    ///
    /// This cost must be calculated only once for the
    /// transport resource and period.
    /// </summary>
    public CostBreakdown CalculateTransportResourceCost(
        int transportResourceId,
        int period,
        bool resourceActivated,
        double additionalCapacityUsed)
    {
        ValidatePeriod(period);

        ValidateQuantity(
            additionalCapacityUsed,
            nameof(additionalCapacityUsed));

        ValidateResourceActivation(
            resourceActivated,
            additionalCapacityUsed);

        TransportResource resource =
            Index.GetRequiredTransportResource(
                transportResourceId);

        double fixedUsageCost =
            resourceActivated
                ? GetCostValue(
                    resource.FixedUsageCost?[period],
                    nameof(resource.FixedUsageCost))
                : 0.0;

        double additionalCapacityCost =
            Multiply(
                additionalCapacityUsed,
                GetCostValue(
                    resource.AdditionalCapacityCost?[period],
                    nameof(resource.AdditionalCapacityCost)),
                "transport-resource additional-capacity cost");

        return new CostBreakdown(
            fixedUsageCost: fixedUsageCost,
            additionalCapacityCost:
                additionalCapacityCost);
    }

    /// <summary>
    /// Calculates the item-specific transport cost during
    /// one planning period.
    ///
    /// The global transport-resource cost is not included.
    /// </summary>
    public CostBreakdown CalculateTransportActivityCost(
        int itemId,
        int transportResourceId,
        int period,
        double transportedQuantity,
        bool setupActivated,
        double itemAdditionalCapacityUsed)
    {
        ValidatePeriod(period);

        ValidateQuantity(
            transportedQuantity,
            nameof(transportedQuantity));

        ValidateQuantity(
            itemAdditionalCapacityUsed,
            nameof(itemAdditionalCapacityUsed));

        TransportCharacteristic characteristic =
            Queries.GetRequiredTransportCharacteristic(
                itemId,
                transportResourceId);

        double fixedSetupCost =
            setupActivated
                ? GetCostValue(
                    characteristic.FixedSetupCost?[period],
                    nameof(characteristic.FixedSetupCost))
                : 0.0;

        double unitUsageCost =
            Multiply(
                transportedQuantity,
                GetCostValue(
                    characteristic.UnitUsageCost?[period],
                    nameof(characteristic.UnitUsageCost)),
                "transport unit-usage cost");

        double additionalCapacityCost =
            Multiply(
                itemAdditionalCapacityUsed,
                GetCostValue(
                    characteristic.AdditionalCapacityCost?[period],
                    nameof(characteristic.AdditionalCapacityCost)),
                "item-specific transport additional-capacity cost");

        return new CostBreakdown(
            additionalCapacityCost:
                additionalCapacityCost,
            fixedSetupCost: fixedSetupCost,
            unitUsageCost: unitUsageCost);
    }

    #endregion

    #region Distribution costs and revenue

    /// <summary>
    /// Calculates the distribution penalties and sales revenue
    /// associated with a sourcing relationship.
    /// </summary>
    public CostBreakdown CalculateDistributionCost(
        DistributionCenterSourcing sourcing,
        int period,
        double soldQuantity,
        double backlogQuantity,
        double shortageQuantity)
    {
        ArgumentNullException.ThrowIfNull(sourcing);

        ValidatePeriod(period);

        ValidateQuantity(
            soldQuantity,
            nameof(soldQuantity));

        ValidateQuantity(
            backlogQuantity,
            nameof(backlogQuantity));

        ValidateQuantity(
            shortageQuantity,
            nameof(shortageQuantity));

        Index.GetRequiredItem(sourcing.ItemId);

        Index.GetRequiredDistributionCenter(
            sourcing.DistributionCenterId);

        Index.GetRequiredWarehouse(
            sourcing.Warehouse);

        double backlogCost =
            Multiply(
                backlogQuantity,
                GetCostValue(
                    sourcing.BacklogCost?[period],
                    nameof(sourcing.BacklogCost)),
                "backlog cost");

        double shortageCost =
            Multiply(
                shortageQuantity,
                GetCostValue(
                    sourcing.ShortageCost?[period],
                    nameof(sourcing.ShortageCost)),
                "shortage cost");

        double revenue =
            Multiply(
                soldQuantity,
                GetCostValue(
                    sourcing.SellingPrice?[period],
                    nameof(sourcing.SellingPrice)),
                "sales revenue");

        return new CostBreakdown(
            backlogCost: backlogCost,
            shortageCost: shortageCost,
            revenue: revenue);
    }

    #endregion

    #region Purchase costs

    /// <summary>
    /// Calculates the purchase cost associated with a supplier
    /// delivery during one planning period.
    /// </summary>
    public CostBreakdown CalculatePurchaseCost(
        SupplierDelivery delivery,
        int period,
        double purchasedQuantity)
    {
        ArgumentNullException.ThrowIfNull(delivery);

        ValidatePeriod(period);

        ValidateQuantity(
            purchasedQuantity,
            nameof(purchasedQuantity));

        Index.GetRequiredSupplier(delivery.SupplierId);
        Index.GetRequiredItem(delivery.ItemId);

        Index.GetRequiredWarehouse(
            delivery.Warehouse);

        double purchaseCost =
            Multiply(
                purchasedQuantity,
                GetCostValue(
                    delivery.PurchasePrice?[period],
                    nameof(delivery.PurchasePrice)),
                "purchase cost");

        return new CostBreakdown(
            purchaseCost: purchaseCost);
    }

    #endregion

    #region Aggregation

    /// <summary>
    /// Aggregates several cost calculations.
    /// </summary>
    public CostBreakdown Aggregate(
        IEnumerable<CostBreakdown> costs)
    {
        ArgumentNullException.ThrowIfNull(costs);

        CostBreakdown total =
            CostBreakdown.Empty;

        foreach (CostBreakdown cost in costs)
        {
            if (cost is null)
            {
                throw new ArgumentException(
                    "The cost collection cannot contain null.",
                    nameof(costs));
            }

            total = total.Add(cost);
        }

        return total;
    }

    #endregion

    #region Validation and calculation helpers

    private void ValidatePeriod(int period)
    {
        if (period < 1 ||
            period > SupplyChain.PlanningHorizon)
        {
            throw new ArgumentOutOfRangeException(
                nameof(period),
                period,
                $"The period must be between 1 and " +
                $"{SupplyChain.PlanningHorizon}.");
        }
    }

    private static void ValidateResourceActivation(
        bool resourceActivated,
        double additionalCapacityUsed)
    {
        if (!resourceActivated &&
            additionalCapacityUsed > 0.0)
        {
            throw new InvalidOperationException(
                "Additional capacity cannot be used when " +
                "the resource is not activated.");
        }
    }

    private static void ValidateQuantity(
        double quantity,
        string parameterName)
    {
        if (!double.IsFinite(quantity) ||
            quantity < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                quantity,
                "The quantity must be finite and non-negative.");
        }
    }

    private static double GetCostValue(
        double? cost,
        string parameterName)
    {
        double value = cost ?? 0.0;

        if (!double.IsFinite(value) ||
            value < 0.0)
        {
            throw new InvalidOperationException(
                $"The cost parameter '{parameterName}' must " +
                "be finite and non-negative.");
        }

        return value;
    }

    private static double Multiply(
        double quantity,
        double unitCost,
        string calculationName)
    {
        double result =
            quantity * unitCost;

        if (!double.IsFinite(result))
        {
            throw new InvalidOperationException(
                $"The {calculationName} exceeds the " +
                "supported numerical range.");
        }

        return result;
    }

    #endregion

    /// <summary>
    /// Represents a detailed cost and revenue calculation.
    /// </summary>
    public sealed class CostBreakdown
    {
        /// <summary>
        /// Gets an empty cost breakdown.
        /// </summary>
        public static CostBreakdown Empty { get; } =
            new();

        /// <summary>
        /// Initializes a detailed cost and revenue breakdown.
        /// </summary>
        /// <param name="fixedUsageCost">
        /// Fixed resource-usage cost.
        /// </param>
        /// <param name="additionalCapacityCost">
        /// Cost of additional capacity.
        /// </param>
        /// <param name="fixedSetupCost">
        /// Fixed setup cost.
        /// </param>
        /// <param name="unitUsageCost">
        /// Quantity-dependent resource-usage cost.
        /// </param>
        /// <param name="safetyStockViolationCost">
        /// Cost caused by a safety-stock violation.
        /// </param>
        /// <param name="backlogCost">
        /// Cost associated with backlogged demand.
        /// </param>
        /// <param name="shortageCost">
        /// Cost associated with lost or unsatisfied demand.
        /// </param>
        /// <param name="purchaseCost">
        /// Cost of purchased quantities.
        /// </param>
        /// <param name="revenue">
        /// Revenue generated by sales.
        /// </param>
        public CostBreakdown(
            double fixedUsageCost = 0.0,
            double additionalCapacityCost = 0.0,
            double fixedSetupCost = 0.0,
            double unitUsageCost = 0.0,
            double safetyStockViolationCost = 0.0,
            double backlogCost = 0.0,
            double shortageCost = 0.0,
            double purchaseCost = 0.0,
            double revenue = 0.0)
        {
            ValidateCost(
                fixedUsageCost,
                nameof(fixedUsageCost));

            ValidateCost(
                additionalCapacityCost,
                nameof(additionalCapacityCost));

            ValidateCost(
                fixedSetupCost,
                nameof(fixedSetupCost));

            ValidateCost(
                unitUsageCost,
                nameof(unitUsageCost));

            ValidateCost(
                safetyStockViolationCost,
                nameof(safetyStockViolationCost));

            ValidateCost(
                backlogCost,
                nameof(backlogCost));

            ValidateCost(
                shortageCost,
                nameof(shortageCost));

            ValidateCost(
                purchaseCost,
                nameof(purchaseCost));

            ValidateCost(
                revenue,
                nameof(revenue));

            FixedUsageCost = fixedUsageCost;

            AdditionalCapacityCost =
                additionalCapacityCost;

            FixedSetupCost = fixedSetupCost;
            UnitUsageCost = unitUsageCost;

            SafetyStockViolationCost =
                safetyStockViolationCost;

            BacklogCost = backlogCost;
            ShortageCost = shortageCost;
            PurchaseCost = purchaseCost;
            Revenue = revenue;

            TotalCost = SumFinite(
                fixedUsageCost,
                additionalCapacityCost,
                fixedSetupCost,
                unitUsageCost,
                safetyStockViolationCost,
                backlogCost,
                shortageCost,
                purchaseCost);

            NetCost = TotalCost - Revenue;

            if (!double.IsFinite(NetCost))
            {
                throw new InvalidOperationException(
                    "The net cost exceeds the supported " +
                    "numerical range.");
            }
        }

        /// <summary>
        /// Gets the fixed resource-usage cost.
        /// </summary>
        public double FixedUsageCost { get; }

        /// <summary>
        /// Gets the additional-capacity cost.
        /// </summary>
        public double AdditionalCapacityCost { get; }

        /// <summary>
        /// Gets the fixed setup cost.
        /// </summary>
        public double FixedSetupCost { get; }

        /// <summary>
        /// Gets the unit resource-usage cost.
        /// </summary>
        public double UnitUsageCost { get; }

        /// <summary>
        /// Gets the safety-stock violation cost.
        /// </summary>
        public double SafetyStockViolationCost { get; }

        /// <summary>
        /// Gets the backlog cost.
        /// </summary>
        public double BacklogCost { get; }

        /// <summary>
        /// Gets the shortage cost.
        /// </summary>
        public double ShortageCost { get; }

        /// <summary>
        /// Gets the purchase cost.
        /// </summary>
        public double PurchaseCost { get; }

        /// <summary>
        /// Gets the sales revenue.
        /// </summary>
        public double Revenue { get; }

        /// <summary>
        /// Gets the sum of all costs before revenue.
        /// </summary>
        public double TotalCost { get; }

        /// <summary>
        /// Gets total costs minus sales revenue.
        ///
        /// A negative value represents a positive margin.
        /// </summary>
        public double NetCost { get; }

        /// <summary>
        /// Gets the margin, defined as revenue minus costs.
        /// </summary>
        public double Margin => -NetCost;

        /// <summary>
        /// Gets a value indicating whether the calculation
        /// contains neither cost nor revenue.
        /// </summary>
        public bool IsEmpty =>
            TotalCost == 0.0 &&
            Revenue == 0.0;

        /// <summary>
        /// Returns a new breakdown containing the sum of
        /// the current calculation and another calculation.
        /// </summary>
        public CostBreakdown Add(
            CostBreakdown other)
        {
            ArgumentNullException.ThrowIfNull(other);

            return new CostBreakdown(
                fixedUsageCost:
                    AddFinite(
                        FixedUsageCost,
                        other.FixedUsageCost),

                additionalCapacityCost:
                    AddFinite(
                        AdditionalCapacityCost,
                        other.AdditionalCapacityCost),

                fixedSetupCost:
                    AddFinite(
                        FixedSetupCost,
                        other.FixedSetupCost),

                unitUsageCost:
                    AddFinite(
                        UnitUsageCost,
                        other.UnitUsageCost),

                safetyStockViolationCost:
                    AddFinite(
                        SafetyStockViolationCost,
                        other.SafetyStockViolationCost),

                backlogCost:
                    AddFinite(
                        BacklogCost,
                        other.BacklogCost),

                shortageCost:
                    AddFinite(
                        ShortageCost,
                        other.ShortageCost),

                purchaseCost:
                    AddFinite(
                        PurchaseCost,
                        other.PurchaseCost),

                revenue:
                    AddFinite(
                        Revenue,
                        other.Revenue));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return
                $"Total cost: {TotalCost}; " +
                $"revenue: {Revenue}; " +
                $"net cost: {NetCost}; " +
                $"margin: {Margin}";
        }

        private static void ValidateCost(
            double cost,
            string parameterName)
        {
            if (!double.IsFinite(cost) ||
                cost < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    cost,
                    "The cost must be finite and non-negative.");
            }
        }

        private static double AddFinite(
            double first,
            double second)
        {
            double result = first + second;

            if (!double.IsFinite(result))
            {
                throw new InvalidOperationException(
                    "The aggregated cost exceeds the " +
                    "supported numerical range.");
            }

            return result;
        }

        private static double SumFinite(
            params double[] values)
        {
            double total = 0.0;

            foreach (double value in values)
            {
                total = AddFinite(
                    total,
                    value);
            }

            return total;
        }
    }
}