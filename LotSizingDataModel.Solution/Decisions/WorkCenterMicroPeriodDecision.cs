using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Solution.Decisions;

[Serializable]
[XmlType(TypeName = "workCenterMicroPeriodDecision")]
public sealed class WorkCenterMicroPeriodDecision : ModelObject
{
    private int _period;
    private int _microPeriod;
    private int _itemId;
    private double _quantity;

    [XmlAttribute("period")]
    public int Period
    {
        get => _period;
        set => SetProperty(ref _period, value);
    }

    [XmlAttribute("microPeriod")]
    public int MicroPeriod
    {
        get => _microPeriod;
        set => SetProperty(ref _microPeriod, value);
    }

    [XmlAttribute("itemId")]
    public int ItemId
    {
        get => _itemId;
        set => SetProperty(ref _itemId, value);
    }

    [XmlAttribute("quantity")]
    public double Quantity
    {
        get => _quantity;
        set
        {
            if (!double.IsFinite(value) || value < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            SetProperty(ref _quantity, value);
        }
    }
}
