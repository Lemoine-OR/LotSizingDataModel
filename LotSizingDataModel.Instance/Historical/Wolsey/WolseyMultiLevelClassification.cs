namespace LotSizingDataModel.Instance.Historical.Wolsey;

/// <summary>
/// Product-structure symbol used in Wolsey's multi-level extension.
/// </summary>
public enum WolseyMultiLevelStructure
{
    /// <summary>General product structure.</summary>
    G,

    /// <summary>Assembly structure.</summary>
    A,

    /// <summary>Series / linear structure.</summary>
    S
}

/// <summary>
/// Typed representation of Wolsey's multi-level block {NL=#,[G,A,S]}.
/// </summary>
public sealed class WolseyMultiLevelClassification
{
    public WolseyMultiLevelClassification(
        int levelCount,
        WolseyMultiLevelStructure structure)
    {
        if (levelCount <= 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(levelCount),
                levelCount,
                "The Wolsey multi-level block requires NL > 1.");
        }

        LevelCount = levelCount;
        Structure = structure;
    }

    public int LevelCount { get; }
    public WolseyMultiLevelStructure Structure { get; }

    public string HistoricalCode =>
        $"{{NL={LevelCount},{Structure}}}";

    public override string ToString() =>
        HistoricalCode;
}
