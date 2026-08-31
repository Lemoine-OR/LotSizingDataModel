using LotSizingDataModel.Instance.Historical;

namespace LotSizingDataModel.Instance.Classification.Historical;

public enum WolseyDetectedProblemVariant
{
    Undetermined = 0,
    DLSI = 1,
    DLS = 2
}

public enum WolseyDetectedCapacityVariant
{
    Undetermined = 0,
    U = 1,
    CC = 2,
    C = 3
}

public enum WolseyDetectedBucketVariant
{
    Undetermined = 0,
    SB1 = 1,
    SB2 = 2,
    BB = 3
}

/// <summary>
/// Lossless detected/source-preserved Wolsey descriptor.
/// </summary>
public sealed class WolseyHistoricalDescriptor
{
    public WolseyDetectedProblemVariant ProblemVariant { get; init; }

    public WolseyDetectedCapacityVariant CapacityVariant { get; init; }

    public WolseyDetectedBucketVariant BucketVariant { get; init; }

    public int NumberOfMachines { get; init; }

    public int NumberOfItems { get; init; }

    public int NumberOfPeriods { get; init; }

    public int? NumberOfLevels { get; init; }

    public bool HasSalesOption { get; init; }

    public bool HasSetupTimes { get; init; }

    public WolseyDeclaredMachineLabel DeclaredMachineLabel { get; init; }

    /// <summary>
    /// Always false in alpha.32: IM/VM are never inferred.
    /// </summary>
    public bool MachineLabelWasInferred => false;

    public bool HasExactCounters =>
        NumberOfMachines >= 0 &&
        NumberOfItems >= 0 &&
        NumberOfPeriods >= 0 &&
        NumberOfLevels is >= 1;

    public bool HasDetectedProblemVariant =>
        ProblemVariant != WolseyDetectedProblemVariant.Undetermined;

    public string ToDetectedSummary()
    {
        var parts = new List<string>();

        if (ProblemVariant != WolseyDetectedProblemVariant.Undetermined)
        {
            parts.Add(ProblemVariant.ToString());
        }

        if (CapacityVariant != WolseyDetectedCapacityVariant.Undetermined)
        {
            parts.Add(CapacityVariant.ToString());
        }

        var variants = new List<string>();

        if (HasSalesOption)
        {
            variants.Add("SL");
        }

        if (HasSetupTimes)
        {
            variants.Add("SET");
        }

        string basic =
            parts.Count == 0
                ? "Undetermined"
                : string.Join("-", parts);

        if (variants.Count > 0)
        {
            basic += "-{" + string.Join(",", variants) + "}";
        }

        string counts =
            $"NK={NumberOfMachines};NI={NumberOfItems};NT={NumberOfPeriods};" +
            $"NL={(NumberOfLevels?.ToString() ?? "?")}";

        string bucket =
            BucketVariant == WolseyDetectedBucketVariant.Undetermined
                ? string.Empty
                : ";" + BucketVariant;

        return basic + ";" + counts + bucket;
    }
}
