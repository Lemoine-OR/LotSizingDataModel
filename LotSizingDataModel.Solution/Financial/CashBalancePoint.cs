using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Solution.Financial;

[Serializable]
[XmlType(TypeName = "cashBalancePoint")]
public sealed class CashBalancePoint : ModelObject
{
    private int _period;
    private double _balance;

    [XmlAttribute("period")]
    public int Period
    {
        get => _period;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            SetProperty(ref _period, value);
        }
    }

    [XmlAttribute("balance")]
    public double Balance
    {
        get => _balance;
        set
        {
            if (!double.IsFinite(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            SetProperty(ref _balance, value);
        }
    }
}
