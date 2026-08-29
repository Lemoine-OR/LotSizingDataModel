namespace LotSizingDataModel.Instance.Historical.Wolsey;

/// <summary>
/// Preserves Wolsey's optional multi-item/machine/level extension around the
/// single-item PROB-CAP-VAR classification.
/// </summary>
public sealed class WolseyExtendedClassification
{
    public WolseyExtendedClassification(
        WolseySingleItemClassification singleItem,
        int? itemCount = null,
        int? periodCount = null,
        WolseyMachineClassification? machines = null,
        WolseyMultiLevelClassification? multiLevel = null)
    {
        SingleItem =
            singleItem ??
            throw new ArgumentNullException(nameof(singleItem));

        if (itemCount is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(itemCount),
                itemCount,
                "NI must be positive when supplied.");
        }

        if (periodCount is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodCount),
                periodCount,
                "NT must be positive when supplied.");
        }

        ItemCount = itemCount;
        PeriodCount = periodCount;
        Machines = machines;
        MultiLevel = multiLevel;
    }

    public WolseySingleItemClassification SingleItem { get; }
    public int? ItemCount { get; }
    public int? PeriodCount { get; }
    public WolseyMachineClassification? Machines { get; }
    public WolseyMultiLevelClassification? MultiLevel { get; }

    public string HistoricalCode
    {
        get
        {
            var blocks = new List<string>();

            if (MultiLevel is not null)
            {
                blocks.Add(MultiLevel.HistoricalCode);
            }

            if (Machines is not null)
            {
                blocks.Add(Machines.HistoricalCode);
            }

            if (ItemCount.HasValue)
            {
                blocks.Add($"{{NI={ItemCount.Value}}}");
            }

            if (PeriodCount.HasValue)
            {
                blocks.Add($"{{NT={PeriodCount.Value}}}");
            }

            blocks.Add(
                "{" +
                SingleItem.HistoricalCode +
                "}");

            return string.Concat(blocks);
        }
    }

    public override string ToString() =>
        HistoricalCode;
}
