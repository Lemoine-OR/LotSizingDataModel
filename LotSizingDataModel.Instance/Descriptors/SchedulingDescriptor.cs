using LotSizingDataModel.Core.DecisionModel.Scheduling;

namespace LotSizingDataModel.Instance.Descriptors;

/// <summary>
/// Factual integrated lot-sizing/scheduling characteristics.
/// </summary>
public sealed class SchedulingDescriptor
{
    public bool HasIntegratedScheduling { get; init; }

    public SchedulingBucketMode BucketMode { get; init; }

    public SmallBucketProductionMode SmallBucketProductionMode
    {
        get;
        init;
    }

    public int SchedulingResourceCount { get; init; }

    public MicroPeriodLengthMode MicroPeriodLengthMode { get; init; }

    public MicroPeriodAssignmentMode MicroPeriodAssignmentMode { get; init; }

    public bool HasExplicitMicroPeriodGrid { get; init; }

    public int TotalMicroPeriodCount { get; init; }

    public int MaximumMicroPeriodCountPerMacroPeriod { get; init; }

    public bool HasVariableMicroPeriodCount { get; init; }

    public bool HasMaximumProducedItemCountConstraint { get; init; }

    public int MaximumProducedItemCountPerBucket { get; init; }

    public int MaximumSetupTransitionsPerBucket { get; init; }

    public bool HasInitialSetupState { get; init; }

    public bool HasSetupCarryOver { get; init; }

    public bool HasSequenceDependentChangeoverTimes { get; init; }

    public bool HasSequenceDependentChangeoverCosts { get; init; }

    public bool HasMaximumSetupCountConstraints { get; init; }

    public bool HasMicroPeriodStructure =>
        BucketMode ==
        SchedulingBucketMode.MacroMicro;

    public bool HasSmallBucketStructure =>
        BucketMode ==
        SchedulingBucketMode.SmallBucket;

    public bool HasVariableLengthMicroPeriods =>
        MicroPeriodLengthMode ==
        MicroPeriodLengthMode.Variable;

    public bool HasFixedLengthMicroPeriods =>
        MicroPeriodLengthMode ==
        MicroPeriodLengthMode.Fixed;

    public bool HasSingleItemPerMicroPeriod =>
        MicroPeriodAssignmentMode ==
        MicroPeriodAssignmentMode.SingleItem;

    public bool HasMultipleItemsPerMicroPeriod =>
        MicroPeriodAssignmentMode ==
        MicroPeriodAssignmentMode.MultipleItems;

    public bool HasBigBucketStructure =>
        BucketMode ==
        SchedulingBucketMode.BigBucket;

    public bool HasSingleSchedulingResource =>
        SchedulingResourceCount == 1;

    public bool HasAllOrNothingSmallBucketProduction =>
        SmallBucketProductionMode ==
        SmallBucketProductionMode.AllOrNothing;

    public bool HasContinuousSmallBucketProduction =>
        SmallBucketProductionMode ==
        SmallBucketProductionMode.Continuous;

    public bool HasAtMostOneProducedItemPerBucket =>
        HasMaximumProducedItemCountConstraint &&
        MaximumProducedItemCountPerBucket <= 1;

    public bool HasAtMostTwoProducedItemsPerBucket =>
        HasMaximumProducedItemCountConstraint &&
        MaximumProducedItemCountPerBucket <= 2;

    public bool HasAtMostOneSetupTransitionPerBucket =>
        HasMaximumSetupCountConstraints &&
        MaximumSetupTransitionsPerBucket <= 1;
}
