using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.DecisionModel.Scheduling;

/// <summary>
/// Scheduling semantics attached to one production work center.
/// </summary>
[Serializable]
[XmlType(TypeName = "productionSchedulingProfile")]
public sealed class ProductionSchedulingProfile :
    ModelObject,
    IPlanningHorizonAware
{
    private SchedulingBucketMode _bucketMode;
    private SmallBucketProductionMode _smallBucketProductionMode;
    private MicroPeriodLengthMode _microPeriodLengthMode;
    private MicroPeriodAssignmentMode _microPeriodAssignmentMode;
    private SetupCarryOverPolicy _setupCarryOverPolicy;
    private int _initialSetupItemId;

    [XmlAttribute("bucketMode")]
    public SchedulingBucketMode BucketMode
    {
        get => _bucketMode;
        set => SetProperty(ref _bucketMode, value);
    }

    [XmlAttribute("smallBucketProductionMode")]
    public SmallBucketProductionMode SmallBucketProductionMode
    {
        get => _smallBucketProductionMode;
        set => SetProperty(
            ref _smallBucketProductionMode,
            value);
    }

    [XmlAttribute("microPeriodLengthMode")]
    public MicroPeriodLengthMode MicroPeriodLengthMode
    {
        get => _microPeriodLengthMode;
        set => SetProperty(ref _microPeriodLengthMode, value);
    }

    [XmlAttribute("microPeriodAssignmentMode")]
    public MicroPeriodAssignmentMode MicroPeriodAssignmentMode
    {
        get => _microPeriodAssignmentMode;
        set => SetProperty(ref _microPeriodAssignmentMode, value);
    }

    [XmlAttribute("setupCarryOverPolicy")]
    public SetupCarryOverPolicy SetupCarryOverPolicy
    {
        get => _setupCarryOverPolicy;
        set => SetProperty(ref _setupCarryOverPolicy, value);
    }

    /// <summary>
    /// Item whose setup state is present before the first modeled bucket.
    /// Zero means that no initial setup state is declared.
    /// </summary>
    [XmlAttribute("initialSetupItemId")]
    public int InitialSetupItemId
    {
        get => _initialSetupItemId;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The initial setup item identifier cannot be negative.");
            }

            SetProperty(ref _initialSetupItemId, value);
        }
    }

    [XmlElement("microPeriodCount")]
    public MicroPeriodCount? MicroPeriodCount
    {
        get;
        set;
    }

    [XmlElement("maximumSetupCount")]
    public MaximumSetupCount? MaximumSetupCount
    {
        get;
        set;
    }

    [XmlElement("maximumProducedItemCount")]
    public MaximumProducedItemCount? MaximumProducedItemCount
    {
        get;
        set;
    }

    [XmlArray("changeovers")]
    [XmlArrayItem("changeover")]
    public List<ProductionChangeover> Changeovers
    {
        get;
    } = new();

    [XmlIgnore]
    public bool HasInitialSetupState =>
        InitialSetupItemId > 0;

    [XmlIgnore]
    public bool HasSetupCarryOver =>
        SetupCarryOverPolicy ==
        SetupCarryOverPolicy.Allowed;

    [XmlIgnore]
    public bool HasSequenceDependentChangeoverTimes =>
        Changeovers.Any(
            changeover =>
                changeover.ChangeoverTime is not null);

    [XmlIgnore]
    public bool HasSequenceDependentChangeoverCosts =>
        Changeovers.Any(
            changeover =>
                changeover.ChangeoverCost is not null);

    [XmlIgnore]
    public bool HasMaximumProducedItemCountConstraint =>
        MaximumProducedItemCount is not null;

    [XmlIgnore]
    public bool HasExplicitMicroPeriodGrid =>
        BucketMode == SchedulingBucketMode.MacroMicro &&
        MicroPeriodCount is not null &&
        MicroPeriodCount.PlanningHorizon > 0;

    [XmlIgnore]
    public int TotalMicroPeriodCount =>
        MicroPeriodCount is null
            ? 0
            : Enumerable
                .Range(1, MicroPeriodCount.PlanningHorizon)
                .Sum(period => MicroPeriodCount.GetCount(period));

    [XmlIgnore]
    public int MaximumMicroPeriodCountPerMacroPeriod =>
        GetMaximumValue(
            MicroPeriodCount,
            parameter => parameter.GetCount);

    [XmlIgnore]
    public bool HasVariableMicroPeriodCount =>
        MicroPeriodCount is not null &&
        MicroPeriodCount.PlanningHorizon > 1 &&
        Enumerable
            .Range(1, MicroPeriodCount.PlanningHorizon)
            .Select(period => MicroPeriodCount.GetCount(period))
            .Distinct()
            .Take(2)
            .Count() > 1;

    [XmlIgnore]
    public bool HasVariableLengthMicroPeriods =>
        MicroPeriodLengthMode == MicroPeriodLengthMode.Variable;

    [XmlIgnore]
    public bool HasSingleItemPerMicroPeriod =>
        MicroPeriodAssignmentMode == MicroPeriodAssignmentMode.SingleItem;

    [XmlIgnore]
    public int MaximumProducedItemCountPerBucket =>
        GetMaximumValue(
            MaximumProducedItemCount,
            parameter =>
                parameter.GetCount);

    [XmlIgnore]
    public int MaximumSetupTransitionsPerBucket =>
        GetMaximumValue(
            MaximumSetupCount,
            parameter =>
                parameter.GetCount);

    [XmlIgnore]
    public int PlanningHorizon
    {
        get
        {
            if (MicroPeriodCount is not null)
            {
                return MicroPeriodCount.PlanningHorizon;
            }

            if (MaximumSetupCount is not null)
            {
                return MaximumSetupCount.PlanningHorizon;
            }

            if (MaximumProducedItemCount is not null)
            {
                return MaximumProducedItemCount.PlanningHorizon;
            }

            ProductionChangeover? first =
                Changeovers.FirstOrDefault(
                    changeover =>
                        changeover.PlanningHorizon > 0);

            return first?.PlanningHorizon ?? 0;
        }
    }

    [XmlIgnore]
    public bool HasConsistentPlanningHorizon
    {
        get
        {
            int[] horizons =
                EnumeratePlanningHorizons()
                    .Where(value => value > 0)
                    .Distinct()
                    .ToArray();

            return
                horizons.Length <= 1 &&
                Changeovers.All(
                    changeover =>
                        changeover.HasConsistentPlanningHorizon);
        }
    }

    public void ResizeTimeSeries(int periodCount)
    {
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodCount),
                periodCount,
                "The planning horizon cannot be negative.");
        }

        MicroPeriodCount?.ResizeTimeSeries(periodCount);
        MaximumSetupCount?.ResizeTimeSeries(periodCount);
        MaximumProducedItemCount?.ResizeTimeSeries(periodCount);

        foreach (
            ProductionChangeover changeover
            in Changeovers)
        {
            changeover.ResizeTimeSeries(periodCount);
        }

        OnPropertyChanged(nameof(PlanningHorizon));
        OnPropertyChanged(nameof(HasConsistentPlanningHorizon));
    }

    public IEnumerable<ProductionMicroPeriodReference>
        EnumerateMicroPeriods()
    {
        if (MicroPeriodCount is null)
        {
            yield break;
        }

        for (int macroPeriod = 1;
             macroPeriod <= MicroPeriodCount.PlanningHorizon;
             macroPeriod++)
        {
            int count =
                MicroPeriodCount.GetCount(macroPeriod);

            for (int microPeriodIndex = 1;
                 microPeriodIndex <= count;
                 microPeriodIndex++)
            {
                yield return
                    new ProductionMicroPeriodReference(
                        macroPeriod,
                        microPeriodIndex);
            }
        }
    }

    private static int GetMaximumValue<T>(
        T? parameter,
        Func<T, Func<int, int>> accessorFactory)
        where T : class, IPlanningHorizonAware
    {
        if (
            parameter is null ||
            parameter.PlanningHorizon <= 0)
        {
            return 0;
        }

        Func<int, int> accessor =
            accessorFactory(parameter);

        return Enumerable
            .Range(
                1,
                parameter.PlanningHorizon)
            .Max(accessor);
    }

    private IEnumerable<int> EnumeratePlanningHorizons()
    {
        if (MicroPeriodCount is not null)
        {
            yield return MicroPeriodCount.PlanningHorizon;
        }

        if (MaximumSetupCount is not null)
        {
            yield return MaximumSetupCount.PlanningHorizon;
        }

        if (MaximumProducedItemCount is not null)
        {
            yield return MaximumProducedItemCount.PlanningHorizon;
        }

        foreach (
            ProductionChangeover changeover
            in Changeovers)
        {
            if (changeover.PlanningHorizon > 0)
            {
                yield return changeover.PlanningHorizon;
            }
        }
    }
}
