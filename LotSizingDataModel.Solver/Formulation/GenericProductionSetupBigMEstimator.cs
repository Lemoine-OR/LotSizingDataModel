using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Provides the default generic production/setup Big-M
/// estimator.
/// </summary>
/// <remarks>
/// <para>
/// The estimator is deliberately solver-independent and does not
/// rely on a benchmark-specific structure. It supports arbitrary
/// acyclic bills of materials, several external demands, shared
/// components, several production routings for the same item,
/// minimum lot sizes, lot-size multiples, and safety-stock
/// requirements.
/// </para>
/// <para>
/// The main structural bound is the full-horizon gross requirement
/// of the produced item. Gross requirements are obtained by
/// recursively propagating all external demand through the BOM.
/// Using the full horizon rather than only periods t..T keeps the
/// bound conservative when backlog and positive lead times are
/// present.
/// </para>
/// <para>
/// Minimum lot sizes and lot-size multiples can require production
/// above gross demand in a single setup. The estimate is therefore
/// enlarged when those restrictions require it. Safety-stock
/// requirements are also included because they can require
/// production even when direct demand is zero.
/// </para>
/// <para>
/// If no structural finite positive bound can be established, the
/// configured formulation fallback is returned. The fallback is a
/// last resort and is explicitly identified in the estimate.
/// </para>
/// </remarks>
public sealed class GenericProductionSetupBigMEstimator :
    IProductionSetupBigMEstimator
{
    /// <summary>
    /// Estimates a finite production upper bound for one routing
    /// and period.
    /// </summary>
    public ProductionSetupBigMEstimate Estimate(
        LotSizingInstance instance,
        ProductionRouting routing,
        int period,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(routing);
        ArgumentNullException.ThrowIfNull(options);

        options.EnsureValid();

        if (period < 1 || period > instance.PlanningHorizon)
        {
            throw new ArgumentOutOfRangeException(
                nameof(period),
                period,
                "The period must belong to the planning horizon.");
        }

        if (!options.UseAutomaticProductionSetupBigM)
        {
            return new ProductionSetupBigMEstimate(
                options.ProductionSetupBigM,
                "Configured fixed ProductionSetupBigM.",
                isFallback: true);
        }

        double grossRequirement;

        try
        {
            grossRequirement =
                CalculateFullHorizonGrossRequirement(
                    instance,
                    routing.ItemId);
        }
        catch (InvalidOperationException)
        {
            return CreateFallback(
                options,
                "Automatic gross-requirement propagation could " +
                "not establish a finite bound.");
        }

        double safetyStockRequirement =
            CalculateSafetyStockRequirement(
                instance,
                routing.ItemId);

        double structuralBound =
            checkedFiniteSum(
                grossRequirement,
                safetyStockRequirement);

        double minimumLotSize =
            routing.MinimumLotSize?[period] ?? 0.0;

        if (!double.IsFinite(minimumLotSize) ||
            minimumLotSize < 0.0)
        {
            return CreateFallback(
                options,
                "The routing minimum lot size is not a valid " +
                "finite non-negative number.");
        }

        double bound =
            Math.Max(
                structuralBound,
                minimumLotSize);

        if (routing.LotSizeMultiple is not null)
        {
            double multiple =
                routing.LotSizeMultiple[period];

            if (!double.IsFinite(multiple) || multiple <= 0.0)
            {
                return CreateFallback(
                    options,
                    "The routing lot-size multiple is not a " +
                    "valid finite positive number.");
            }

            if (bound > 0.0)
            {
                bound =
                    Math.Ceiling(bound / multiple) * multiple;
            }
        }

        if (!double.IsFinite(bound) || bound < 0.0)
        {
            return CreateFallback(
                options,
                "The automatically computed production bound " +
                "is not finite.");
        }

        if (bound <= options.StructuralZeroTolerance)
        {
            return new ProductionSetupBigMEstimate(
                0.0,
                "No external, BOM-induced, safety-stock, or " +
                "minimum-lot requirement exists for this item.",
                isFallback: false);
        }

        return new ProductionSetupBigMEstimate(
            bound,
            $"Automatic structural bound: grossRequirement=" +
            $"{grossRequirement:G17}; safetyStock=" +
            $"{safetyStockRequirement:G17}; minimumLotSize=" +
            $"{minimumLotSize:G17}.",
            isFallback: false);
    }

    private static double CalculateFullHorizonGrossRequirement(
        LotSizingInstance instance,
        int itemId)
    {
        var memo =
            new Dictionary<int, double>();

        var visiting =
            new HashSet<int>();

        return CalculateGrossRequirementRecursive(
            instance,
            itemId,
            memo,
            visiting);
    }

    private static double CalculateGrossRequirementRecursive(
        LotSizingInstance instance,
        int itemId,
        IDictionary<int, double> memo,
        ISet<int> visiting)
    {
        if (memo.TryGetValue(itemId, out double cached))
        {
            return cached;
        }

        if (!visiting.Add(itemId))
        {
            throw new InvalidOperationException(
                "The bill of materials contains a cycle.");
        }

        double result =
            0.0;

        foreach (
            Demand demand
            in instance.SupplyChain.Demands.Where(
                demand => demand.ItemId == itemId))
        {
            for (int period = 1;
                 period <= instance.PlanningHorizon;
                 period++)
            {
                result =
                    checkedFiniteSum(
                        result,
                        demand.GetQuantity(period));
            }
        }

        foreach (
            ComponentRequirement requirement
            in instance.SupplyChain.ComponentRequirements.Where(
                requirement =>
                    requirement.ComponentItemId == itemId))
        {
            double parentRequirement =
                CalculateGrossRequirementRecursive(
                    instance,
                    requirement.ParentItemId,
                    memo,
                    visiting);

            double inducedRequirement =
                parentRequirement * requirement.Quantity;

            if (!double.IsFinite(inducedRequirement) ||
                inducedRequirement < 0.0)
            {
                throw new InvalidOperationException(
                    "The propagated BOM requirement is not finite.");
            }

            result =
                checkedFiniteSum(
                    result,
                    inducedRequirement);
        }

        visiting.Remove(itemId);
        memo[itemId] = result;

        return result;
    }

    private static double CalculateSafetyStockRequirement(
        LotSizingInstance instance,
        int itemId)
    {
        double total =
            0.0;

        foreach (
            Inventory inventory
            in instance.SupplyChain.Inventories.Where(
                inventory => inventory.ItemId == itemId))
        {
            if (inventory.SafetyStock is null)
            {
                continue;
            }

            double maximum =
                0.0;

            for (int period = 1;
                 period <= instance.PlanningHorizon;
                 period++)
            {
                maximum =
                    Math.Max(
                        maximum,
                        inventory.SafetyStock[period]);
            }

            total =
                checkedFiniteSum(
                    total,
                    maximum);
        }

        return total;
    }

    private static ProductionSetupBigMEstimate CreateFallback(
        StandardLotSizingFormulationOptions options,
        string reason)
    {
        return new ProductionSetupBigMEstimate(
            options.ProductionSetupBigM,
            reason + " Configured ProductionSetupBigM fallback " +
            $"{options.ProductionSetupBigM:G17} was used.",
            isFallback: true);
    }

    private static double checkedFiniteSum(
        double left,
        double right)
    {
        double result = left + right;

        if (!double.IsFinite(result) || result < 0.0)
        {
            throw new InvalidOperationException(
                "A structural Big-M calculation overflowed or " +
                "produced a negative value.");
        }

        return result;
    }
}
