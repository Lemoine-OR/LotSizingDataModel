using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Core.Indexing;
using LotSizingDataModel.Core.Relationships;

namespace LotSizingDataModel.Core.Analysis;

/// <summary>
/// Provides aggregate calculations over the demand relationships
/// contained in a supply-chain model.
///
/// Demand values correspond to D[c,i,t], where:
/// - c identifies a distribution center;
/// - i identifies an item;
/// - t identifies a planning period.
/// </summary>
public sealed class DemandAnalyzer
{
    private readonly Dictionary<int, List<Demand>>
        _demandsByItem = new();

    private readonly Dictionary<int, List<Demand>>
        _demandsByDistributionCenter = new();

    /// <summary>
    /// Initializes a demand analyzer and creates an entity index.
    /// </summary>
    /// <param name="supplyChain">
    /// Supply chain containing the demands to analyze.
    /// </param>
    public DemandAnalyzer(SupplyChain supplyChain)
        : this(
            new SupplyChainIndex(
                supplyChain ??
                throw new ArgumentNullException(
                    nameof(supplyChain))))
    {
    }

    /// <summary>
    /// Initializes a demand analyzer using an existing index.
    /// </summary>
    /// <param name="index">
    /// Index used to resolve item and distribution-center
    /// references.
    /// </param>
    public DemandAnalyzer(SupplyChainIndex index)
    {
        Index = index ??
            throw new ArgumentNullException(nameof(index));

        SupplyChain = index.SupplyChain;

        Rebuild();
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
    /// Rebuilds the entity index and the demand indexes.
    ///
    /// Call this method after adding, removing or modifying
    /// demand relationships.
    /// </summary>
    public void Rebuild()
    {
        Index.Rebuild();

        _demandsByItem.Clear();
        _demandsByDistributionCenter.Clear();

        foreach (Demand demand in SupplyChain.Demands)
        {
            /*
             * These calls ensure that the referenced entities
             * exist before the demand is indexed.
             */
            Index.GetRequiredItem(demand.ItemId);

            Index.GetRequiredDistributionCenter(
                demand.DistributionCenterId);

            ValidateDemandHorizon(demand);

            AddDemand(
                _demandsByItem,
                demand.ItemId,
                demand);

            AddDemand(
                _demandsByDistributionCenter,
                demand.DistributionCenterId,
                demand);
        }
    }

    #region Demand relationship queries

    /// <summary>
    /// Gets every demand defined for an item.
    /// </summary>
    public IReadOnlyList<Demand> GetDemandsForItem(
        int itemId)
    {
        Index.GetRequiredItem(itemId);

        if (!_demandsByItem.TryGetValue(
                itemId,
                out List<Demand>? demands))
        {
            return Array.Empty<Demand>();
        }

        return demands.ToArray();
    }

    /// <summary>
    /// Gets every demand expressed by a distribution center.
    /// </summary>
    public IReadOnlyList<Demand>
        GetDemandsForDistributionCenter(
            int distributionCenterId)
    {
        Index.GetRequiredDistributionCenter(
            distributionCenterId);

        if (!_demandsByDistributionCenter.TryGetValue(
                distributionCenterId,
                out List<Demand>? demands))
        {
            return Array.Empty<Demand>();
        }

        return demands.ToArray();
    }

    /// <summary>
    /// Finds the demand relationship defined for an item and
    /// a distribution center.
    ///
    /// Returns null when no demand relationship exists.
    /// </summary>
    public Demand? FindDemand(
        int itemId,
        int distributionCenterId)
    {
        Index.GetRequiredItem(itemId);

        Index.GetRequiredDistributionCenter(
            distributionCenterId);

        if (!_demandsByItem.TryGetValue(
                itemId,
                out List<Demand>? demands))
        {
            return null;
        }

        return demands.FirstOrDefault(
            demand =>
                demand.DistributionCenterId ==
                    distributionCenterId);
    }

    /// <summary>
    /// Gets the demand relationship defined for an item and
    /// a distribution center.
    /// </summary>
    public Demand GetRequiredDemand(
        int itemId,
        int distributionCenterId)
    {
        return FindDemand(
                   itemId,
                   distributionCenterId) ??
            throw new KeyNotFoundException(
                $"No demand exists for item {itemId} and " +
                $"distribution center {distributionCenterId}.");
    }

    #endregion

    #region Period calculations

    /// <summary>
    /// Gets the demand of an item at a distribution center
    /// during a planning period.
    /// </summary>
    public double GetDemandQuantity(
        int itemId,
        int distributionCenterId,
        int period)
    {
        ValidatePeriod(period);

        return GetRequiredDemand(
                itemId,
                distributionCenterId)
            .GetQuantity(period);
    }

    /// <summary>
    /// Calculates the total demand for an item during
    /// a planning period.
    /// </summary>
    public double GetTotalDemandForItem(
        int itemId,
        int period)
    {
        ValidatePeriod(period);

        IReadOnlyList<Demand> demands =
            GetDemandsForItem(itemId);

        return SumDemands(
            demands,
            period);
    }

    /// <summary>
    /// Calculates the total demand expressed by a distribution
    /// center during a planning period.
    /// </summary>
    public double GetTotalDemandForDistributionCenter(
        int distributionCenterId,
        int period)
    {
        ValidatePeriod(period);

        IReadOnlyList<Demand> demands =
            GetDemandsForDistributionCenter(
                distributionCenterId);

        return SumDemands(
            demands,
            period);
    }

    /// <summary>
    /// Calculates the total demand of the complete supply chain
    /// during a planning period.
    /// </summary>
    public double GetGlobalDemand(int period)
    {
        ValidatePeriod(period);

        return SumDemands(
            SupplyChain.Demands,
            period);
    }

    #endregion

    #region Demand profiles

    /// <summary>
    /// Calculates the aggregate demand profile of an item.
    /// </summary>
    public DemandProfile GetDemandProfileForItem(
        int itemId)
    {
        IReadOnlyList<Demand> demands =
            GetDemandsForItem(itemId);

        return CreateProfile(demands);
    }

    /// <summary>
    /// Calculates the aggregate demand profile of a
    /// distribution center.
    /// </summary>
    public DemandProfile
        GetDemandProfileForDistributionCenter(
            int distributionCenterId)
    {
        IReadOnlyList<Demand> demands =
            GetDemandsForDistributionCenter(
                distributionCenterId);

        return CreateProfile(demands);
    }

    /// <summary>
    /// Calculates the aggregate demand profile of the complete
    /// supply chain.
    /// </summary>
    public DemandProfile GetGlobalDemandProfile()
    {
        return CreateProfile(
            SupplyChain.Demands);
    }

    /// <summary>
    /// Calculates the demand profile of one item at one
    /// distribution center.
    /// </summary>
    public DemandProfile GetDemandProfile(
        int itemId,
        int distributionCenterId)
    {
        Demand demand =
            GetRequiredDemand(
                itemId,
                distributionCenterId);

        double[] quantities =
            new double[SupplyChain.PlanningHorizon];

        for (int period = 1;
             period <= SupplyChain.PlanningHorizon;
             period++)
        {
            quantities[period - 1] =
                demand.GetQuantity(period);
        }

        return new DemandProfile(quantities);
    }

    #endregion

    #region Cumulative calculations

    /// <summary>
    /// Calculates the cumulative demand for an item over
    /// the complete planning horizon.
    /// </summary>
    public double GetCumulativeDemandForItem(
        int itemId)
    {
        return GetDemandProfileForItem(itemId)
            .TotalQuantity;
    }

    /// <summary>
    /// Calculates the cumulative demand for an item over
    /// an inclusive period interval.
    /// </summary>
    public double GetCumulativeDemandForItem(
        int itemId,
        int startPeriod,
        int endPeriod)
    {
        return GetDemandProfileForItem(itemId)
            .GetCumulativeQuantity(
                startPeriod,
                endPeriod);
    }

    /// <summary>
    /// Calculates the cumulative demand expressed by a
    /// distribution center over the complete horizon.
    /// </summary>
    public double
        GetCumulativeDemandForDistributionCenter(
            int distributionCenterId)
    {
        return GetDemandProfileForDistributionCenter(
                distributionCenterId)
            .TotalQuantity;
    }

    /// <summary>
    /// Calculates the cumulative global demand over an
    /// inclusive period interval.
    /// </summary>
    public double GetCumulativeGlobalDemand(
        int startPeriod,
        int endPeriod)
    {
        return GetGlobalDemandProfile()
            .GetCumulativeQuantity(
                startPeriod,
                endPeriod);
    }

    #endregion

    #region Private helpers

    private DemandProfile CreateProfile(
        IEnumerable<Demand> demands)
    {
        ArgumentNullException.ThrowIfNull(demands);

        Demand[] demandArray =
            demands.ToArray();

        double[] quantities =
            new double[SupplyChain.PlanningHorizon];

        for (int period = 1;
             period <= SupplyChain.PlanningHorizon;
             period++)
        {
            quantities[period - 1] =
                SumDemands(
                    demandArray,
                    period);
        }

        return new DemandProfile(quantities);
    }

    private static double SumDemands(
        IEnumerable<Demand> demands,
        int period)
    {
        double total = 0.0;

        foreach (Demand demand in demands)
        {
            double quantity =
                demand.GetQuantity(period);

            if (!double.IsFinite(quantity) ||
                quantity < 0.0)
            {
                throw new InvalidOperationException(
                    "A demand quantity must be finite " +
                    "and non-negative.");
            }

            total += quantity;

            if (!double.IsFinite(total))
            {
                throw new InvalidOperationException(
                    "The aggregated demand exceeds the " +
                    "supported numerical range.");
            }
        }

        return total;
    }

    private void ValidateDemandHorizon(
        Demand demand)
    {
        if (demand.PlanningHorizon !=
            SupplyChain.PlanningHorizon)
        {
            throw new InvalidOperationException(
                $"The demand for item {demand.ItemId} and " +
                $"distribution center " +
                $"{demand.DistributionCenterId} uses a " +
                $"planning horizon of {demand.PlanningHorizon}, " +
                $"whereas the global planning horizon is " +
                $"{SupplyChain.PlanningHorizon}.");
        }
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

    private static void AddDemand(
        IDictionary<int, List<Demand>> dictionary,
        int key,
        Demand demand)
    {
        if (!dictionary.TryGetValue(
                key,
                out List<Demand>? demands))
        {
            demands = new List<Demand>();

            dictionary.Add(
                key,
                demands);
        }

        demands.Add(demand);
    }

    #endregion

    /// <summary>
    /// Represents an aggregated demand time series.
    ///
    /// Period numbers are one-based.
    /// </summary>
    public sealed class DemandProfile
    {
        private readonly double[] _quantities;

        /// <summary>
        /// Initializes a demand profile.
        /// </summary>
        public DemandProfile(
            IEnumerable<double> quantities)
        {
            ArgumentNullException.ThrowIfNull(quantities);

            _quantities = quantities.ToArray();

            for (int index = 0;
                 index < _quantities.Length;
                 index++)
            {
                double quantity =
                    _quantities[index];

                if (!double.IsFinite(quantity) ||
                    quantity < 0.0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(quantities),
                        quantity,
                        "A demand-profile quantity must be " +
                        "finite and non-negative.");
                }
            }

            TotalQuantity =
                CalculateTotalQuantity();

            AverageQuantity =
                _quantities.Length == 0
                    ? 0.0
                    : TotalQuantity /
                      _quantities.Length;

            (PeakPeriod, PeakQuantity) =
                CalculatePeak();
        }

        /// <summary>
        /// Gets the number of periods in the profile.
        /// </summary>
        public int PlanningHorizon =>
            _quantities.Length;

        /// <summary>
        /// Gets a copy of the quantities.
        /// </summary>
        public IReadOnlyList<double> Quantities =>
            Array.AsReadOnly(
                (double[])_quantities.Clone());

        /// <summary>
        /// Gets the demand quantity for a planning period.
        /// </summary>
        public double this[int period]
        {
            get
            {
                ValidateProfilePeriod(period);

                return _quantities[period - 1];
            }
        }

        /// <summary>
        /// Gets the cumulative demand over the complete profile.
        /// </summary>
        public double TotalQuantity { get; }

        /// <summary>
        /// Gets the average demand per planning period.
        /// </summary>
        public double AverageQuantity { get; }

        /// <summary>
        /// Gets the first period having the highest demand.
        ///
        /// Returns zero when the profile contains no period.
        /// </summary>
        public int PeakPeriod { get; }

        /// <summary>
        /// Gets the highest period demand.
        /// </summary>
        public double PeakQuantity { get; }

        /// <summary>
        /// Calculates the cumulative demand over an inclusive
        /// period interval.
        /// </summary>
        public double GetCumulativeQuantity(
            int startPeriod,
            int endPeriod)
        {
            ValidateProfilePeriod(startPeriod);
            ValidateProfilePeriod(endPeriod);

            if (startPeriod > endPeriod)
            {
                throw new ArgumentException(
                    "The start period cannot be greater " +
                    "than the end period.");
            }

            double total = 0.0;

            for (int period = startPeriod;
                 period <= endPeriod;
                 period++)
            {
                total += this[period];

                if (!double.IsFinite(total))
                {
                    throw new InvalidOperationException(
                        "The cumulative demand exceeds the " +
                        "supported numerical range.");
                }
            }

            return total;
        }

        private double CalculateTotalQuantity()
        {
            double total = 0.0;

            foreach (double quantity in _quantities)
            {
                total += quantity;

                if (!double.IsFinite(total))
                {
                    throw new InvalidOperationException(
                        "The total demand exceeds the supported " +
                        "numerical range.");
                }
            }

            return total;
        }

        private (int Period, double Quantity)
            CalculatePeak()
        {
            if (_quantities.Length == 0)
            {
                return (0, 0.0);
            }

            int peakPeriod = 1;
            double peakQuantity = _quantities[0];

            for (int index = 1;
                 index < _quantities.Length;
                 index++)
            {
                if (_quantities[index] >
                    peakQuantity)
                {
                    peakQuantity =
                        _quantities[index];

                    peakPeriod = index + 1;
                }
            }

            return (
                peakPeriod,
                peakQuantity
            );
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
}