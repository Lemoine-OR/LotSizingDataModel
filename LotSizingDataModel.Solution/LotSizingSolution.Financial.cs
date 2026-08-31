using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Solution.Financial;

namespace LotSizingDataModel.Solution;

/// <summary>
/// Adds normalized financial execution traces to a solution.
/// </summary>
public sealed partial class LotSizingSolution
{
    [XmlArray("cashBalances")]
    [XmlArrayItem("cashBalance")]
    public List<CashBalancePoint> CashBalances
    {
        get;
    } = new();

    public void SetCashBalance(
        int period,
        double balance)
    {
        CashBalancePoint? existing =
            CashBalances.SingleOrDefault(
                point =>
                    point.Period == period);

        if (existing is null)
        {
            CashBalances.Add(
                new CashBalancePoint
                {
                    Period = period,
                    Balance = balance
                });

            return;
        }

        existing.Balance = balance;
    }

    public bool TryGetCashBalance(
        int period,
        out double balance)
    {
        CashBalancePoint? existing =
            CashBalances.SingleOrDefault(
                point =>
                    point.Period == period);

        if (existing is null)
        {
            balance = 0.0;
            return false;
        }

        balance = existing.Balance;
        return true;
    }
}
