using System.Reflection;
using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance.Historical;

namespace LotSizingDataModel.Instance.Classification.Historical;

/// <summary>
/// Projects generic LotSizingDataModel semantics onto Wolsey's historical dimensions.
/// </summary>
/// <remarks>
/// Detection is deliberately conservative. A source-declared historical code is never
/// used as detected evidence. IM/VM are preserved only as source labels.
/// </remarks>
public static class WolseyHistoricalSemanticsAnalyzer
{
    public static WolseyHistoricalDescriptor Analyze(
        LotSizingInstance instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        SupplyChain chain = instance.SupplyChain;

        bool zeroFull =
            DetectZeroFullProduction(instance);

        InitialInventoryDecisionMode[] modes =
            chain.Inventories
                .Select(
                    inventory =>
                        inventory.InitialInventoryDecisionMode)
                .Distinct()
                .ToArray();

        WolseyDetectedProblemVariant problem =
            DetectProblemVariant(
                zeroFull,
                modes);

        return new WolseyHistoricalDescriptor
        {
            ProblemVariant = problem,
            CapacityVariant = DetectCapacityVariant(chain),
            BucketVariant = DetectBucketVariant(chain),
            NumberOfMachines = chain.WorkCenters.Count(),
            NumberOfItems = chain.Items.Count,
            NumberOfPeriods = chain.PlanningHorizon,
            NumberOfLevels = DetectNumberOfLevels(chain),
            HasSalesOption = chain.SalesOptions.Count > 0,
            HasSetupTimes =
                chain.ProductionCharacteristics.Any(
                    characteristic =>
                        characteristic.SetupTime is not null),
            DeclaredMachineLabel =
                instance.HistoricalSemantics?
                    .DeclaredWolseyMachineLabel ??
                WolseyDeclaredMachineLabel.Unspecified
        };
    }

    public static IReadOnlyList<string> ValidateHistoricalSemantics(
        LotSizingInstance instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        var issues = new List<string>();
        SupplyChain chain = instance.SupplyChain;

        foreach (Inventory inventory in chain.Inventories)
        {
            if (!inventory.HasValidInitialInventoryDecisionSemantics)
            {
                issues.Add(
                    $"Inventory item {inventory.ItemId} has a non-zero fixed " +
                    "InitialInventory while its mode requires zero.");
            }
        }

        foreach (SalesOption salesOption in chain.SalesOptions)
        {
            if (!salesOption.IsInternallyValid)
            {
                issues.Add(
                    $"SalesOption item {salesOption.ItemId} / distribution center " +
                    $"{salesOption.DistributionCenterId} is internally invalid.");
            }

            if (!chain.Items.Any(item => item.Id == salesOption.ItemId))
            {
                issues.Add(
                    $"SalesOption references unknown item {salesOption.ItemId}.");
            }

            if (!chain.DistributionCenters.Any(
                    center =>
                        center.Id ==
                        salesOption.DistributionCenterId))
            {
                issues.Add(
                    "SalesOption references unknown distribution center " +
                    $"{salesOption.DistributionCenterId}.");
            }
        }

        return issues;
    }

    private static WolseyDetectedProblemVariant DetectProblemVariant(
        bool zeroFull,
        IReadOnlyCollection<InitialInventoryDecisionMode> modes)
    {
        if (!zeroFull || modes.Count != 1)
        {
            return WolseyDetectedProblemVariant.Undetermined;
        }

        return modes.Single() switch
        {
            InitialInventoryDecisionMode.VariableDecision =>
                WolseyDetectedProblemVariant.DLSI,

            InitialInventoryDecisionMode.AbsentFixedZero =>
                WolseyDetectedProblemVariant.DLS,

            _ =>
                WolseyDetectedProblemVariant.Undetermined
        };
    }

    private static bool DetectZeroFullProduction(
        LotSizingInstance instance)
    {
        bool schedulingEvidence =
            instance.SupplyChain.WorkCenters.Any(
                workCenter =>
                    workCenter.SchedulingProfile is not null &&
                    workCenter.SchedulingProfile.SmallBucketProductionMode ==
                        SmallBucketProductionMode.AllOrNothing);

        if (schedulingEvidence)
        {
            return true;
        }

        // Reuse existing generic feature semantics without a compile-time
        // dependency on the exact alpha.14 property name.
        object features =
            LotSizingProblemFeatureExtractor.Extract(
                instance.SupplyChain);

        string[] acceptedPropertyNames =
        [
            "HasZeroFullProduction",
            "HasZeroFullCapacityProduction",
            "UsesZeroFullProduction",
            "UsesZeroFullCapacityProduction"
        ];

        Type type = features.GetType();

        foreach (string propertyName in acceptedPropertyNames)
        {
            PropertyInfo? property =
                type.GetProperty(
                    propertyName,
                    BindingFlags.Instance |
                    BindingFlags.Public);

            if (property?.PropertyType == typeof(bool) &&
                property.GetValue(features) is true)
            {
                return true;
            }
        }

        return false;
    }

    private static WolseyDetectedCapacityVariant DetectCapacityVariant(
        SupplyChain chain)
    {
        WorkCenter[] workCenters =
            chain.WorkCenters.ToArray();

        if (workCenters.Length == 0)
        {
            return WolseyDetectedCapacityVariant.Undetermined;
        }

        CapacityConstraint[] capacities =
            workCenters
                .Select(
                    workCenter =>
                        workCenter.CapacityConstraint)
                .Where(
                    capacity =>
                        capacity is not null)
                .Cast<CapacityConstraint>()
                .ToArray();

        if (capacities.Length == 0)
        {
            return WolseyDetectedCapacityVariant.U;
        }

        if (capacities.Length != workCenters.Length)
        {
            return WolseyDetectedCapacityVariant.Undetermined;
        }

        bool allConstant = true;

        foreach (CapacityConstraint capacity in capacities)
        {
            if (capacity.PlanningHorizon != chain.PlanningHorizon)
            {
                return WolseyDetectedCapacityVariant.Undetermined;
            }

            if (chain.PlanningHorizon <= 1)
            {
                continue;
            }

            double first = capacity[1];

            for (int period = 2;
                 period <= chain.PlanningHorizon;
                 period++)
            {
                if (!capacity[period].Equals(first))
                {
                    allConstant = false;
                    break;
                }
            }
        }

        return allConstant
            ? WolseyDetectedCapacityVariant.CC
            : WolseyDetectedCapacityVariant.C;
    }

    private static WolseyDetectedBucketVariant DetectBucketVariant(
        SupplyChain chain)
    {
        WorkCenter[] workCenters =
            chain.WorkCenters.ToArray();

        bool jointCapacity =
            workCenters.Any(
                workCenter =>
                    workCenter.CapacityConstraint is not null &&
                    CountItemsUsingWorkCenter(
                        chain,
                        workCenter) > 1);

        ProductionSchedulingProfile[] profiles =
            workCenters
                .Where(
                    workCenter =>
                        workCenter.SchedulingProfile is not null)
                .Select(
                    workCenter =>
                        workCenter.SchedulingProfile!)
                .ToArray();

        if (profiles.Length == 0)
        {
            return WolseyDetectedBucketVariant.Undetermined;
        }

        if (profiles.All(
                profile =>
                    profile.BucketMode ==
                    SchedulingBucketMode.SmallBucket) &&
            profiles.All(
                profile =>
                    profile.MaximumSetupCount is not null))
        {
            int maximumSetupCount = 0;

            foreach (ProductionSchedulingProfile profile in profiles)
            {
                if (profile.MaximumSetupCount!.PlanningHorizon !=
                    chain.PlanningHorizon)
                {
                    return WolseyDetectedBucketVariant.Undetermined;
                }

                for (int period = 1;
                     period <= chain.PlanningHorizon;
                     period++)
                {
                    maximumSetupCount =
                        Math.Max(
                            maximumSetupCount,
                            profile.MaximumSetupCount.GetCount(period));
                }
            }

            if (maximumSetupCount <= 1)
            {
                return WolseyDetectedBucketVariant.SB1;
            }

            if (maximumSetupCount <= 2)
            {
                return WolseyDetectedBucketVariant.SB2;
            }

            return jointCapacity
                ? WolseyDetectedBucketVariant.BB
                : WolseyDetectedBucketVariant.Undetermined;
        }

        if (profiles.Any(
                profile =>
                    profile.BucketMode ==
                    SchedulingBucketMode.BigBucket) &&
            jointCapacity)
        {
            return WolseyDetectedBucketVariant.BB;
        }

        return WolseyDetectedBucketVariant.Undetermined;
    }

    private static int CountItemsUsingWorkCenter(
        SupplyChain chain,
        WorkCenter workCenter)
    {
        return chain.ProductionRoutings
            .Where(
                routing =>
                    routing.WorkCenters.Any(
                        reference =>
                            reference.WorkCenterId ==
                                workCenter.Id))
            .Select(
                routing =>
                    routing.ItemId)
            .Distinct()
            .Count();
    }

    private static int? DetectNumberOfLevels(
        SupplyChain chain)
    {
        if (chain.Items.Count == 0)
        {
            return 0;
        }

        if (chain.ComponentRequirements.Count == 0)
        {
            return 1;
        }

        var children =
            chain.ComponentRequirements
                .GroupBy(
                    requirement =>
                        requirement.ParentItemId)
                .ToDictionary(
                    group =>
                        group.Key,
                    group =>
                        group
                            .Select(
                                requirement =>
                                    requirement.ComponentItemId)
                            .Distinct()
                            .ToArray());

        var state = new Dictionary<int, int>();
        var memo = new Dictionary<int, int>();

        int? Visit(int itemId)
        {
            if (memo.TryGetValue(itemId, out int cached))
            {
                return cached;
            }

            if (state.TryGetValue(itemId, out int current) &&
                current == 1)
            {
                return null;
            }

            state[itemId] = 1;
            int depth = 1;

            if (children.TryGetValue(itemId, out int[]? childIds))
            {
                foreach (int childId in childIds)
                {
                    int? childDepth = Visit(childId);

                    if (childDepth is null)
                    {
                        return null;
                    }

                    depth =
                        Math.Max(
                            depth,
                            1 + childDepth.Value);
                }
            }

            state[itemId] = 2;
            memo[itemId] = depth;
            return depth;
        }

        int maximum = 1;

        foreach (var item in chain.Items)
        {
            int? depth = Visit(item.Id);

            if (depth is null)
            {
                return null;
            }

            maximum = Math.Max(maximum, depth.Value);
        }

        return maximum;
    }
}
