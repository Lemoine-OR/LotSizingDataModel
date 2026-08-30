using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.DecisionModel.Scheduling;

[Serializable]
[XmlType(TypeName = "productionMicroPeriodReference")]
public sealed class ProductionMicroPeriodReference : ModelObject
{
    private int _macroPeriod;
    private int _microPeriodIndex;

    public ProductionMicroPeriodReference()
    {
    }

    public ProductionMicroPeriodReference(
        int macroPeriod,
        int microPeriodIndex)
    {
        MacroPeriod = macroPeriod;
        MicroPeriodIndex = microPeriodIndex;
    }

    [XmlAttribute("macroPeriod")]
    public int MacroPeriod
    {
        get => _macroPeriod;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The macro-period index cannot be negative.");
            }

            SetProperty(ref _macroPeriod, value);
        }
    }

    [XmlAttribute("microPeriodIndex")]
    public int MicroPeriodIndex
    {
        get => _microPeriodIndex;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The micro-period index cannot be negative.");
            }

            SetProperty(ref _microPeriodIndex, value);
        }
    }

    [XmlIgnore]
    public bool IsInternallyValid =>
        MacroPeriod > 0 &&
        MicroPeriodIndex > 0;

    public bool RefersToSameMicroPeriod(
        ProductionMicroPeriodReference? other) =>
            other is not null &&
            MacroPeriod == other.MacroPeriod &&
            MicroPeriodIndex == other.MicroPeriodIndex;

    public override string ToString() =>
        $"Macro:{MacroPeriod}/Micro:{MicroPeriodIndex}";
}
