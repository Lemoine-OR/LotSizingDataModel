namespace LotSizingDataModel.Instance.Historical.Wolsey;

/// <summary>Bucket-size subfield in Wolsey's multi-item extension.</summary>
public enum WolseyBucketType
{
    SB1,
    SB2,
    BB
}

/// <summary>
/// Capacity-utilization/setup/changeover markers in Wolsey's multi-item
/// extension.
/// </summary>
public enum WolseyMachineFeature
{
    SET,
    ST,
    SQT,
    SQC
}

/// <summary>
/// Preserves the IM/VM machine-mode labels used in Wolsey's formal scheme.
/// </summary>
/// <remarks>
/// The historical symbols are preserved without expanding or reinterpreting
/// them here. Their exact wording is kept as source terminology until the
/// project has a verified primary-source definition suitable for semantic
/// mapping.
/// </remarks>
public enum WolseyMachineMode
{
    IM,
    VM
}

/// <summary>
/// Typed representation of the machine block
/// {NK=#,[IM,VM],[LT]*,[SB1,SB2,BB],[SET,ST,SQT,SQC]*}.
/// </summary>
public sealed class WolseyMachineClassification
{
    public WolseyMachineClassification(
        int machineCount,
        WolseyMachineMode machineMode,
        WolseyBucketType bucketType,
        bool hasLeadTimes = false,
        IEnumerable<WolseyMachineFeature>? features = null)
    {
        if (machineCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(machineCount),
                machineCount,
                "Wolsey NK must be strictly positive.");
        }

        MachineCount = machineCount;
        MachineMode = machineMode;
        BucketType = bucketType;
        HasLeadTimes = hasLeadTimes;

        Features =
            (features ?? Array.Empty<WolseyMachineFeature>())
                .Distinct()
                .OrderBy(feature => (int)feature)
                .ToArray();
    }

    public int MachineCount { get; }
    public WolseyMachineMode MachineMode { get; }
    public bool HasLeadTimes { get; }
    public WolseyBucketType BucketType { get; }
    public IReadOnlyCollection<WolseyMachineFeature> Features { get; }

    public string HistoricalCode
    {
        get
        {
            var parts =
                new List<string>
                {
                    $"NK={MachineCount}",
                    MachineMode.ToString()
                };

            if (HasLeadTimes)
            {
                parts.Add("LT");
            }

            parts.Add(BucketType.ToString());

            parts.AddRange(
                Features.Select(feature => feature.ToString()));

            return
                "{" +
                string.Join(",", parts) +
                "}";
        }
    }

    public override string ToString() =>
        HistoricalCode;
}
