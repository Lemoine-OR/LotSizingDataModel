using System;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Solution.Decisions;

[Serializable]
[XmlType(TypeName = "closedLoopDecision")]
public sealed class ClosedLoopDecision :
    ModelObject,
    IPlanningHorizonAware
{
    private int _returnStreamId;
    private DoubleTimeSeries _recoveryInputs = new();
    private DoubleTimeSeries _disposalQuantities = new();
    private DoubleTimeSeries _recoveredOutputs = new();

    public ClosedLoopDecision()
    {
    }

    public ClosedLoopDecision(
        int returnStreamId,
        int planningHorizon)
    {
        if (returnStreamId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(returnStreamId));
        }

        ReturnStreamId = returnStreamId;
        ResizeTimeSeries(planningHorizon);
    }

    [XmlAttribute("returnStreamId")]
    public int ReturnStreamId
    {
        get => _returnStreamId;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value));
            }

            SetProperty(
                ref _returnStreamId,
                value);
        }
    }

    [XmlElement("recoveryInputs")]
    public DoubleTimeSeries RecoveryInputs
    {
        get => _recoveryInputs;
        set => _recoveryInputs =
            value ??
            new DoubleTimeSeries();
    }

    [XmlElement("disposalQuantities")]
    public DoubleTimeSeries DisposalQuantities
    {
        get => _disposalQuantities;
        set => _disposalQuantities =
            value ??
            new DoubleTimeSeries();
    }

    [XmlElement("recoveredOutputs")]
    public DoubleTimeSeries RecoveredOutputs
    {
        get => _recoveredOutputs;
        set => _recoveredOutputs =
            value ??
            new DoubleTimeSeries();
    }

    [XmlIgnore]
    public int PlanningHorizon =>
        RecoveryInputs.PeriodCount;

    [XmlIgnore]
    public bool HasConsistentPlanningHorizon =>
        DisposalQuantities.PeriodCount == PlanningHorizon &&
        RecoveredOutputs.PeriodCount == PlanningHorizon;

    [XmlIgnore]
    public bool IsInternallyValid =>
        ReturnStreamId > 0 &&
        PlanningHorizon > 0 &&
        HasConsistentPlanningHorizon &&
        RecoveryInputs.All(
            value =>
                double.IsFinite(value) &&
                value >= 0.0) &&
        DisposalQuantities.All(
            value =>
                double.IsFinite(value) &&
                value >= 0.0) &&
        RecoveredOutputs.All(
            value =>
                double.IsFinite(value) &&
                value >= 0.0);

    public void ResizeTimeSeries(
        int periodCount)
    {
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodCount));
        }

        RecoveryInputs.Resize(
            periodCount,
            0.0);

        DisposalQuantities.Resize(
            periodCount,
            0.0);

        RecoveredOutputs.Resize(
            periodCount,
            0.0);
    }
}
