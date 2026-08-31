using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Instance.ClosedLoop;

/// <summary>
/// Describes one exogenous stream of returned units and its
/// recovery/disposal alternatives.
/// </summary>
[Serializable]
[XmlType(TypeName = "closedLoopReturnStream")]
public sealed class ClosedLoopReturnStream :
    ModelObject,
    IPlanningHorizonAware
{
    private int _id;
    private int _itemId;
    private int _distributionCenterId;
    private WarehouseReference _recoveryWarehouse =
        new();
    private double _recoveryYield = 1.0;
    private ClosedLoopTimeSeriesParameter _returnQuantity =
        new();
    private ClosedLoopTimeSeriesParameter? _recoveryCapacity;
    private ClosedLoopTimeSeriesParameter _collectionUnitCost =
        new();
    private ClosedLoopTimeSeriesParameter _recoveryUnitCost =
        new();
    private ClosedLoopTimeSeriesParameter _disposalUnitCost =
        new();

    public ClosedLoopReturnStream()
    {
    }

    public ClosedLoopReturnStream(
        int id,
        int itemId,
        int distributionCenterId,
        WarehouseReference recoveryWarehouse,
        int planningHorizon)
    {
        Id =
            id;

        ItemId =
            itemId;

        DistributionCenterId =
            distributionCenterId;

        RecoveryWarehouse =
            recoveryWarehouse;

        ResizeTimeSeries(
            planningHorizon);
    }

    [XmlAttribute("id")]
    public int Id
    {
        get => _id;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value));
            }

            SetProperty(
                ref _id,
                value);
        }
    }

    [XmlAttribute("itemId")]
    public int ItemId
    {
        get => _itemId;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value));
            }

            SetProperty(
                ref _itemId,
                value);
        }
    }

    [XmlAttribute("distributionCenterId")]
    public int DistributionCenterId
    {
        get => _distributionCenterId;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value));
            }

            SetProperty(
                ref _distributionCenterId,
                value);
        }
    }

    [XmlElement("recoveryWarehouse")]
    public WarehouseReference RecoveryWarehouse
    {
        get => _recoveryWarehouse;
        set =>
            SetProperty(
                ref _recoveryWarehouse,
                value ??
                new WarehouseReference());
    }

    [XmlAttribute("recoveryYield")]
    public double RecoveryYield
    {
        get => _recoveryYield;
        set
        {
            if (!double.IsFinite(value) ||
                value <= 0.0 ||
                value > 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "Recovery yield must belong to ]0,1].");
            }

            SetProperty(
                ref _recoveryYield,
                value);
        }
    }

    [XmlElement("returnQuantity")]
    public ClosedLoopTimeSeriesParameter ReturnQuantity
    {
        get => _returnQuantity;
        set =>
            SetProperty(
                ref _returnQuantity,
                value ??
                new ClosedLoopTimeSeriesParameter());
    }

    [XmlElement("recoveryCapacity", IsNullable = true)]
    public ClosedLoopTimeSeriesParameter? RecoveryCapacity
    {
        get => _recoveryCapacity;
        set =>
            SetProperty(
                ref _recoveryCapacity,
                value);
    }

    [XmlElement("collectionUnitCost")]
    public ClosedLoopTimeSeriesParameter CollectionUnitCost
    {
        get => _collectionUnitCost;
        set =>
            SetProperty(
                ref _collectionUnitCost,
                value ??
                new ClosedLoopTimeSeriesParameter());
    }

    [XmlElement("recoveryUnitCost")]
    public ClosedLoopTimeSeriesParameter RecoveryUnitCost
    {
        get => _recoveryUnitCost;
        set =>
            SetProperty(
                ref _recoveryUnitCost,
                value ??
                new ClosedLoopTimeSeriesParameter());
    }

    [XmlElement("disposalUnitCost")]
    public ClosedLoopTimeSeriesParameter DisposalUnitCost
    {
        get => _disposalUnitCost;
        set =>
            SetProperty(
                ref _disposalUnitCost,
                value ??
                new ClosedLoopTimeSeriesParameter());
    }

    [XmlIgnore]
    public int PlanningHorizon =>
        ReturnQuantity.PlanningHorizon;

    [XmlIgnore]
    public bool HasConsistentPlanningHorizon
    {
        get
        {
            int expected =
                ReturnQuantity.PlanningHorizon;

            if (CollectionUnitCost.PlanningHorizon != expected ||
                RecoveryUnitCost.PlanningHorizon != expected ||
                DisposalUnitCost.PlanningHorizon != expected)
            {
                return false;
            }

            return RecoveryCapacity is null ||
                   RecoveryCapacity.PlanningHorizon ==
                       expected;
        }
    }

    public void ResizeTimeSeries(
        int periodCount)
    {
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodCount));
        }

        ReturnQuantity.ResizeTimeSeries(
            periodCount);

        CollectionUnitCost.ResizeTimeSeries(
            periodCount);

        RecoveryUnitCost.ResizeTimeSeries(
            periodCount);

        DisposalUnitCost.ResizeTimeSeries(
            periodCount);

        RecoveryCapacity?.ResizeTimeSeries(
            periodCount);

        OnPropertyChanged(
            nameof(PlanningHorizon));

        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));
    }

    public void EnsureValid()
    {
        if (Id <= 0)
        {
            throw new InvalidOperationException(
                "A closed-loop return stream requires a strictly positive identifier.");
        }

        if (ItemId <= 0)
        {
            throw new InvalidOperationException(
                "A closed-loop return stream requires a strictly positive item identifier.");
        }

        if (DistributionCenterId <= 0)
        {
            throw new InvalidOperationException(
                "A closed-loop return stream requires a strictly positive distribution-center identifier.");
        }

        if (RecoveryWarehouse.ReferenceId <= 0)
        {
            throw new InvalidOperationException(
                "A closed-loop return stream requires a valid recovery warehouse.");
        }

        if (!double.IsFinite(RecoveryYield) ||
            RecoveryYield <= 0.0 ||
            RecoveryYield > 1.0)
        {
            throw new InvalidOperationException(
                "Recovery yield must belong to ]0,1].");
        }

        if (!HasConsistentPlanningHorizon)
        {
            throw new InvalidOperationException(
                "All closed-loop return-stream time series must use one planning horizon.");
        }
    }
}
