using LotSizingDataModel.Core.DecisionModel.Scheduling;

namespace LotSizingDataModel.Instance.Descriptors;

/// <summary>
/// Factual integrated lot-sizing/scheduling characteristics.
/// </summary>
public sealed class SchedulingDescriptor
{
    public bool HasIntegratedScheduling { get; init; }

    public SchedulingBucketMode BucketMode { get; init; }

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

    public bool HasBigBucketStructure =>
        BucketMode ==
        SchedulingBucketMode.BigBucket;
}
