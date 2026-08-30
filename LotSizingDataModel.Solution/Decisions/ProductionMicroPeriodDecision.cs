using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel.Scheduling;

namespace LotSizingDataModel.Solution.Decisions;

[Serializable]
[XmlType(TypeName = "productionMicroPeriodDecision")]
public sealed class ProductionMicroPeriodDecision : ModelObject
{
    private ProductionMicroPeriodReference _microPeriod = new();
    private int _setupItemId;
    private int _routingId;
    private double _quantity;

    public ProductionMicroPeriodDecision()
    {
    }

    public ProductionMicroPeriodDecision(
        ProductionMicroPeriodReference microPeriod,
        int setupItemId,
        int routingId = 0,
        double quantity = 0.0)
    {
        MicroPeriod = microPeriod ??
            throw new ArgumentNullException(nameof(microPeriod));
        SetupItemId = setupItemId;
        RoutingId = routingId;
        Quantity = quantity;
    }

    [XmlElement("microPeriod")]
    public ProductionMicroPeriodReference MicroPeriod
    {
        get => _microPeriod;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            SetProperty(ref _microPeriod, value);
        }
    }

    [XmlAttribute("setupItemId")]
    public int SetupItemId
    {
        get => _setupItemId;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The setup item identifier cannot be negative.");

            SetProperty(ref _setupItemId, value);
        }
    }

    [XmlAttribute("routingId")]
    public int RoutingId
    {
        get => _routingId;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The routing identifier cannot be negative.");

            SetProperty(ref _routingId, value);
        }
    }

    [XmlAttribute("quantity")]
    public double Quantity
    {
        get => _quantity;
        set
        {
            if (!double.IsFinite(value) || value < 0.0)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The micro-period production quantity must be finite and non-negative.");

            SetProperty(ref _quantity, value);
        }
    }

    [XmlIgnore]
    public bool HasProduction => Quantity > 0.0;

    [XmlIgnore]
    public bool IsIdle => !HasProduction;

    [XmlIgnore]
    public bool IsInternallyValid =>
        MicroPeriod.IsInternallyValid &&
        SetupItemId >= 0 &&
        RoutingId >= 0 &&
        double.IsFinite(Quantity) &&
        Quantity >= 0.0 &&
        (!HasProduction ||
         (SetupItemId > 0 && RoutingId > 0));

    public bool RefersToSameMicroPeriod(
        ProductionMicroPeriodDecision? other) =>
            other is not null &&
            MicroPeriod.RefersToSameMicroPeriod(other.MicroPeriod);
}
