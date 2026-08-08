using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Core.Indexing;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Querying;
using LotSizingDataModel.Core.Relationships;

namespace LotSizingDataModel.Core.Analysis;

/// <summary>
/// Provides capacity-profile and capacity-load calculations
/// for production, storage and transport resources.
///
/// The analyzer does not modify the supply-chain model.
/// </summary>
public sealed class CapacityAnalyzer
{
    /// <summary>
    /// Initializes a capacity analyzer and creates
    /// a new supply-chain index.
    /// </summary>
    public CapacityAnalyzer(SupplyChain supplyChain)
        : this(
            new SupplyChainIndex(
                supplyChain ??
                throw new ArgumentNullException(
                    nameof(supplyChain))))
    {
    }

    /// <summary>
    /// Initializes a capacity analyzer using an existing index.
    /// </summary>
    public CapacityAnalyzer(SupplyChainIndex index)
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
    /// Gets the query service used to resolve relationships
    /// and applicable capacity parameters.
    /// </summary>
    public SupplyChainQueries Queries { get; }

    /// <summary>
    /// Rebuilds the underlying entity index.
    ///
    /// Call this method after directly adding or removing
    /// entities from the supply chain.
    /// </summary>
    public void RebuildIndex()
    {
        Index.Rebuild();
    }

    #region Capacity profiles

    /// <summary>
    /// Gets the complete capacity profile of a work center.
    /// </summary>
    public CapacityProfile GetWorkCenterCapacityProfile(
        WorkCenterReference workCenter)
    {
        ArgumentNullException.ThrowIfNull(workCenter);

        Index.GetRequiredWorkCenter(workCenter);

        return CreateCapacityProfile(
            period =>
                Queries.GetWorkCenterCapacity(
                    workCenter,
                    period));
    }

    /// <summary>
    /// Gets the complete global capacity profile
    /// of a warehouse.
    /// </summary>
    public CapacityProfile GetWarehouseCapacityProfile(
        WarehouseReference warehouse)
    {
        ArgumentNullException.ThrowIfNull(warehouse);

        Index.GetRequiredWarehouse(warehouse);

        return CreateCapacityProfile(
            period =>
                Queries.GetWarehouseCapacity(
                    warehouse,
                    period));
    }

    /// <summary>
    /// Gets the complete capacity profile applicable
    /// to an item stored in a warehouse.
    ///
    /// An item-specific inventory capacity is used when
    /// available. Otherwise, the global warehouse capacity
    /// is returned.
    /// </summary>
    public CapacityProfile GetInventoryCapacityProfile(
        int itemId,
        WarehouseReference warehouse)
    {
        ArgumentNullException.ThrowIfNull(warehouse);

        Queries.GetRequiredInventory(
            itemId,
            warehouse);

        return CreateCapacityProfile(
            period =>
                Queries.GetInventoryCapacity(
                    itemId,
                    warehouse,
                    period));
    }

    /// <summary>
    /// Gets the complete global capacity profile
    /// of a transport resource.
    /// </summary>
    public CapacityProfile
        GetTransportResourceCapacityProfile(
            int transportResourceId)
    {
        Index.GetRequiredTransportResource(
            transportResourceId);

        return CreateCapacityProfile(
            period =>
                Queries.GetTransportResourceCapacity(
                    transportResourceId,
                    period));
    }

    /// <summary>
    /// Gets the complete capacity profile applicable to
    /// an item transported by a transport resource.
    ///
    /// An item-specific capacity is used when available.
    /// Otherwise, the global transport-resource capacity
    /// is returned.
    /// </summary>
    public CapacityProfile GetTransportCapacityProfile(
        int itemId,
        int transportResourceId)
    {
        Queries.GetRequiredTransportCharacteristic(
            itemId,
            transportResourceId);

        return CreateCapacityProfile(
            period =>
                Queries.GetTransportCapacity(
                    itemId,
                    transportResourceId,
                    period));
    }

    #endregion

    #region Production load

    /// <summary>
    /// Calculates the capacity required to produce a quantity
    /// of an item on a work center during one period.
    /// </summary>
    public CapacityLoadSnapshot CalculateProductionLoad(
        int itemId,
        WorkCenterReference workCenter,
        int period,
        double productionQuantity,
        bool setupActivated)
    {
        ArgumentNullException.ThrowIfNull(workCenter);

        ValidatePeriod(period);
        ValidateQuantity(
            productionQuantity,
            nameof(productionQuantity));

        ProductionCharacteristic characteristic =
            Queries.GetRequiredProductionCharacteristic(
                itemId,
                workCenter);

        double unitCapacity =
            characteristic.UnitCapacityConsumption
                ?.GetConsumption(period) ??
            0.0;

        double setupCapacity =
            setupActivated
                ? characteristic.SetupTime
                      ?.GetSetupTime(period) ??
                  0.0
                : 0.0;

        SupplyChainQueries.CapacitySnapshot capacity =
            Queries.GetWorkCenterCapacity(
                workCenter,
                period);

        return CreateLoadSnapshot(
            quantity: productionQuantity,
            unitCapacity: unitCapacity,
            setupCapacity: setupCapacity,
            setupActivated: setupActivated,
            capacity: capacity);
    }

    #endregion

    #region Storage load

    /// <summary>
    /// Calculates the capacity required to store a quantity
    /// of an item in a warehouse during one period.
    /// </summary>
    public CapacityLoadSnapshot CalculateInventoryLoad(
        int itemId,
        WarehouseReference warehouse,
        int period,
        double inventoryQuantity,
        bool setupActivated)
    {
        ArgumentNullException.ThrowIfNull(warehouse);

        ValidatePeriod(period);
        ValidateQuantity(
            inventoryQuantity,
            nameof(inventoryQuantity));

        Inventory inventory =
            Queries.GetRequiredInventory(
                itemId,
                warehouse);

        double unitCapacity =
            inventory.UnitCapacityConsumption
                ?.GetConsumption(period) ??
            0.0;

        double setupCapacity =
            setupActivated
                ? inventory.SetupTime
                      ?.GetSetupTime(period) ??
                  0.0
                : 0.0;

        SupplyChainQueries.CapacitySnapshot capacity =
            Queries.GetInventoryCapacity(
                itemId,
                warehouse,
                period);

        return CreateLoadSnapshot(
            quantity: inventoryQuantity,
            unitCapacity: unitCapacity,
            setupCapacity: setupCapacity,
            setupActivated: setupActivated,
            capacity: capacity);
    }

    #endregion

    #region Transport load

    /// <summary>
    /// Calculates the capacity required to transport a quantity
    /// of an item using a transport resource during one period.
    /// </summary>
    public CapacityLoadSnapshot CalculateTransportLoad(
        int itemId,
        int transportResourceId,
        int period,
        double transportedQuantity,
        bool setupActivated)
    {
        ValidatePeriod(period);
        ValidateQuantity(
            transportedQuantity,
            nameof(transportedQuantity));

        TransportCharacteristic characteristic =
            Queries.GetRequiredTransportCharacteristic(
                itemId,
                transportResourceId);

        double unitCapacity =
            characteristic.UnitCapacityConsumption
                ?.GetConsumption(period) ??
            0.0;

        double setupCapacity =
            setupActivated
                ? characteristic.SetupTime
                      ?.GetSetupTime(period) ??
                  0.0
                : 0.0;

        SupplyChainQueries.CapacitySnapshot capacity =
            Queries.GetTransportCapacity(
                itemId,
                transportResourceId,
                period);

        return CreateLoadSnapshot(
            quantity: transportedQuantity,
            unitCapacity: unitCapacity,
            setupCapacity: setupCapacity,
            setupActivated: setupActivated,
            capacity: capacity);
    }

    #endregion

    #region Helpers

    private CapacityProfile CreateCapacityProfile(
        Func<
            int,
            SupplyChainQueries.CapacitySnapshot>
            capacityProvider)
    {
        ArgumentNullException.ThrowIfNull(capacityProvider);

        var regularCapacities =
            new double?[SupplyChain.PlanningHorizon];

        var additionalCapacities =
            new double?[SupplyChain.PlanningHorizon];

        bool isItemSpecific = false;

        for (int period = 1;
             period <= SupplyChain.PlanningHorizon;
             period++)
        {
            SupplyChainQueries.CapacitySnapshot snapshot =
                capacityProvider(period);

            regularCapacities[period - 1] =
                snapshot.MaximumRegularCapacity;

            additionalCapacities[period - 1] =
                snapshot.MaximumAdditionalCapacity;

            isItemSpecific |=
                snapshot.IsItemSpecific;
        }

        return new CapacityProfile(
            regularCapacities,
            additionalCapacities,
            isItemSpecific);
    }

    private static CapacityLoadSnapshot
        CreateLoadSnapshot(
            double quantity,
            double unitCapacity,
            double setupCapacity,
            bool setupActivated,
            SupplyChainQueries.CapacitySnapshot capacity)
    {
        ArgumentNullException.ThrowIfNull(capacity);

        ValidateNonNegativeFiniteValue(
            unitCapacity,
            nameof(unitCapacity));

        ValidateNonNegativeFiniteValue(
            setupCapacity,
            nameof(setupCapacity));

        double quantityCapacity;

        try
        {
            quantityCapacity = checked(
                quantity * unitCapacity);
        }
        catch (OverflowException exception)
        {
            throw new InvalidOperationException(
                "The calculated unit-capacity requirement " +
                "exceeds the supported numerical range.",
                exception);
        }

        double requiredCapacity =
            quantityCapacity +
            setupCapacity;

        if (!double.IsFinite(requiredCapacity))
        {
            throw new InvalidOperationException(
                "The calculated capacity requirement exceeds " +
                "the supported numerical range.");
        }

        return new CapacityLoadSnapshot(
            quantity,
            unitCapacity,
            setupCapacity,
            setupActivated,
            requiredCapacity,
            capacity.MaximumRegularCapacity,
            capacity.MaximumAdditionalCapacity,
            capacity.IsItemSpecific);
    }

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

    private static void ValidateQuantity(
        double quantity,
        string parameterName)
    {
        ValidateNonNegativeFiniteValue(
            quantity,
            parameterName);
    }

    private static void ValidateNonNegativeFiniteValue(
        double value,
        string parameterName)
    {
        if (!double.IsFinite(value) ||
            value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "The value must be finite and non-negative.");
        }
    }

    #endregion

    /// <summary>
    /// Represents the regular and additional capacity
    /// available over a complete planning horizon.
    ///
    /// Period numbers are one-based.
    /// </summary>
    public sealed class CapacityProfile
    {
        private readonly double?[] _regularCapacities;
        private readonly double?[] _additionalCapacities;

        /// <summary>
        /// Initializes a capacity profile over a complete
        /// planning horizon.
        /// </summary>
        /// <param name="regularCapacities">
        /// Regular-capacity limits for each planning period.
        /// A null value means that no regular-capacity constraint
        /// is defined for the corresponding period.
        /// </param>
        /// <param name="additionalCapacities">
        /// Additional-capacity limits for each planning period.
        /// A null value means that no additional capacity is defined.
        /// </param>
        /// <param name="isItemSpecific">
        /// Indicates whether the profile comes from an
        /// item-specific resource relationship.
        /// </param>
        public CapacityProfile(
            IEnumerable<double?> regularCapacities,
            IEnumerable<double?> additionalCapacities,
            bool isItemSpecific)
        {
            ArgumentNullException.ThrowIfNull(
                regularCapacities);

            ArgumentNullException.ThrowIfNull(
                additionalCapacities);

            _regularCapacities =
                regularCapacities.ToArray();

            _additionalCapacities =
                additionalCapacities.ToArray();

            if (_regularCapacities.Length !=
                _additionalCapacities.Length)
            {
                throw new ArgumentException(
                    "Regular and additional capacity series " +
                    "must have the same length.");
            }

            ValidateCapacitySeries(
                _regularCapacities,
                nameof(regularCapacities));

            ValidateCapacitySeries(
                _additionalCapacities,
                nameof(additionalCapacities));

            IsItemSpecific = isItemSpecific;
        }

        /// <summary>
        /// Gets the number of periods in the profile.
        /// </summary>
        public int PlanningHorizon =>
            _regularCapacities.Length;

        /// <summary>
        /// Gets a value indicating whether the capacity is
        /// defined for a specific item-resource relationship.
        /// </summary>
        public bool IsItemSpecific { get; }

        /// <summary>
        /// Gets the regular capacity for a period.
        ///
        /// Null means that no regular-capacity constraint
        /// is defined.
        /// </summary>
        public double? GetRegularCapacity(int period)
        {
            ValidateProfilePeriod(period);

            return _regularCapacities[period - 1];
        }

        /// <summary>
        /// Gets the additional capacity for a period.
        ///
        /// Null means that no additional capacity is defined.
        /// </summary>
        public double? GetAdditionalCapacity(int period)
        {
            ValidateProfilePeriod(period);

            return _additionalCapacities[period - 1];
        }

        /// <summary>
        /// Gets the total capacity for a period.
        ///
        /// Null means that no regular-capacity constraint
        /// is defined.
        /// </summary>
        public double? GetTotalCapacity(int period)
        {
            double? regularCapacity =
                GetRegularCapacity(period);

            if (!regularCapacity.HasValue)
            {
                return null;
            }

            return regularCapacity.Value +
                   (GetAdditionalCapacity(period) ?? 0.0);
        }

        /// <summary>
        /// Gets the sum of regular capacities over the horizon.
        ///
        /// Returns null when at least one period has no
        /// regular-capacity constraint.
        /// </summary>
        public double? TotalRegularCapacity =>
            SumNullableSeries(_regularCapacities);

        /// <summary>
        /// Gets the sum of additional capacities over the horizon.
        ///
        /// Undefined additional capacities are treated as zero.
        /// </summary>
        public double TotalAdditionalCapacity =>
            _additionalCapacities.Sum(
                value => value ?? 0.0);

        /// <summary>
        /// Gets the sum of total capacities over the horizon.
        ///
        /// Returns null when at least one period has no
        /// regular-capacity constraint.
        /// </summary>
        public double? TotalCapacity
        {
            get
            {
                double? regular =
                    TotalRegularCapacity;

                return regular.HasValue
                    ? regular.Value +
                      TotalAdditionalCapacity
                    : null;
            }
        }

        private static double? SumNullableSeries(
            IEnumerable<double?> values)
        {
            double total = 0.0;

            foreach (double? value in values)
            {
                if (!value.HasValue)
                {
                    return null;
                }

                total += value.Value;

                if (!double.IsFinite(total))
                {
                    throw new InvalidOperationException(
                        "The capacity total exceeds the " +
                        "supported numerical range.");
                }
            }

            return total;
        }

        private static void ValidateCapacitySeries(
            IEnumerable<double?> capacities,
            string parameterName)
        {
            foreach (double? capacity in capacities)
            {
                if (capacity.HasValue &&
                    (!double.IsFinite(capacity.Value) ||
                     capacity.Value < 0.0))
                {
                    throw new ArgumentOutOfRangeException(
                        parameterName,
                        capacity,
                        "Capacity values must be finite " +
                        "and non-negative.");
                }
            }
        }

        private void ValidateProfilePeriod(int period)
        {
            if (period < 1 ||
                period > PlanningHorizon)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(period),
                    period,
                    $"The period must be between 1 and " +
                    $"{PlanningHorizon}.");
            }
        }
    }

    /// <summary>
    /// Represents the capacity consumption and feasibility
    /// of one activity during one planning period.
    /// </summary>
    public sealed class CapacityLoadSnapshot
    {
        /// <summary>
        /// Initializes the result of a capacity-load calculation
        /// for one activity and one planning period.
        /// </summary>
        /// <param name="quantity">
        /// Quantity processed, stored or transported.
        /// </param>
        /// <param name="unitCapacity">
        /// Capacity consumed per quantity unit.
        /// </param>
        /// <param name="setupCapacity">
        /// Setup capacity included in the calculation.
        /// </param>
        /// <param name="setupActivated">
        /// Indicates whether setup is activated.
        /// </param>
        /// <param name="requiredCapacity">
        /// Total capacity required by the activity.
        /// </param>
        /// <param name="maximumRegularCapacity">
        /// Maximum regular capacity, or null when unconstrained.
        /// </param>
        /// <param name="maximumAdditionalCapacity">
        /// Maximum additional capacity, or null when unavailable.
        /// </param>
        /// <param name="isItemSpecificCapacity">
        /// Indicates whether the capacity limit is item-specific.
        /// </param>
        public CapacityLoadSnapshot(
            double quantity,
            double unitCapacity,
            double setupCapacity,
            bool setupActivated,
            double requiredCapacity,
            double? maximumRegularCapacity,
            double? maximumAdditionalCapacity,
            bool isItemSpecificCapacity)
        {
            ValidateNonNegativeFiniteValue(
                quantity,
                nameof(quantity));

            ValidateNonNegativeFiniteValue(
                unitCapacity,
                nameof(unitCapacity));

            ValidateNonNegativeFiniteValue(
                setupCapacity,
                nameof(setupCapacity));

            ValidateNonNegativeFiniteValue(
                requiredCapacity,
                nameof(requiredCapacity));

            ValidateNullableCapacity(
                maximumRegularCapacity,
                nameof(maximumRegularCapacity));

            ValidateNullableCapacity(
                maximumAdditionalCapacity,
                nameof(maximumAdditionalCapacity));

            Quantity = quantity;
            UnitCapacity = unitCapacity;
            SetupCapacity = setupCapacity;
            SetupActivated = setupActivated;
            RequiredCapacity = requiredCapacity;

            MaximumRegularCapacity =
                maximumRegularCapacity;

            MaximumAdditionalCapacity =
                maximumAdditionalCapacity;

            IsItemSpecificCapacity =
                isItemSpecificCapacity;
        }

        /// <summary>
        /// Gets the processed quantity.
        /// </summary>
        public double Quantity { get; }

        /// <summary>
        /// Gets the capacity consumed per processed unit.
        /// </summary>
        public double UnitCapacity { get; }

        /// <summary>
        /// Gets the setup-capacity consumption included
        /// in the calculation.
        /// </summary>
        public double SetupCapacity { get; }

        /// <summary>
        /// Gets a value indicating whether setup was activated.
        /// </summary>
        public bool SetupActivated { get; }

        /// <summary>
        /// Gets the total required capacity.
        /// </summary>
        public double RequiredCapacity { get; }

        /// <summary>
        /// Gets the maximum regular capacity.
        ///
        /// Null means that no capacity constraint is defined.
        /// </summary>
        public double? MaximumRegularCapacity { get; }

        /// <summary>
        /// Gets the maximum additional capacity.
        /// </summary>
        public double? MaximumAdditionalCapacity { get; }

        /// <summary>
        /// Gets a value indicating whether the capacity limit
        /// belongs to an item-specific relationship.
        /// </summary>
        public bool IsItemSpecificCapacity { get; }

        /// <summary>
        /// Gets a value indicating whether a capacity
        /// constraint is active.
        /// </summary>
        public bool IsCapacityConstrained =>
            MaximumRegularCapacity.HasValue;

        /// <summary>
        /// Gets the maximum total capacity.
        ///
        /// Null means that no regular-capacity constraint exists.
        /// </summary>
        public double? MaximumTotalCapacity =>
            MaximumRegularCapacity.HasValue
                ? MaximumRegularCapacity.Value +
                  (MaximumAdditionalCapacity ?? 0.0)
                : null;

        /// <summary>
        /// Gets the additional capacity required beyond
        /// the regular capacity.
        /// </summary>
        public double RequiredAdditionalCapacity =>
            MaximumRegularCapacity.HasValue
                ? Math.Max(
                    0.0,
                    RequiredCapacity -
                    MaximumRegularCapacity.Value)
                : 0.0;

        /// <summary>
        /// Gets a value indicating whether regular capacity
        /// is sufficient.
        ///
        /// An unconstrained resource is considered feasible.
        /// </summary>
        public bool FitsRegularCapacity =>
            !MaximumRegularCapacity.HasValue ||
            RequiredCapacity <=
                MaximumRegularCapacity.Value;

        /// <summary>
        /// Gets a value indicating whether additional capacity
        /// must be used.
        /// </summary>
        public bool UsesAdditionalCapacity =>
            MaximumRegularCapacity.HasValue &&
            RequiredCapacity >
                MaximumRegularCapacity.Value;

        /// <summary>
        /// Gets a value indicating whether the requirement fits
        /// within regular plus additional capacity.
        ///
        /// An unconstrained resource is considered feasible.
        /// </summary>
        public bool FitsTotalCapacity =>
            !MaximumTotalCapacity.HasValue ||
            RequiredCapacity <=
                MaximumTotalCapacity.Value;

        /// <summary>
        /// Gets the capacity requirement exceeding the maximum
        /// available capacity.
        /// </summary>
        public double CapacityExcess =>
            MaximumTotalCapacity.HasValue
                ? Math.Max(
                    0.0,
                    RequiredCapacity -
                    MaximumTotalCapacity.Value)
                : 0.0;

        /// <inheritdoc/>
        public override string ToString()
        {
            if (!IsCapacityConstrained)
            {
                return
                    $"Required capacity: {RequiredCapacity}; " +
                    "no capacity constraint";
            }

            return
                $"Required: {RequiredCapacity}; " +
                $"regular: {MaximumRegularCapacity}; " +
                $"additional: " +
                $"{MaximumAdditionalCapacity ?? 0.0}; " +
                $"feasible: {FitsTotalCapacity}";
        }

        private static void ValidateNullableCapacity(
            double? capacity,
            string parameterName)
        {
            if (capacity.HasValue)
            {
                ValidateNonNegativeFiniteValue(
                    capacity.Value,
                    parameterName);
            }
        }
    }
}