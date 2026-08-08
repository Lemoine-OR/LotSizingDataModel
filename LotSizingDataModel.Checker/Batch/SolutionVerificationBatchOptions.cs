using LotSizingDataModel.Checker.Configuration;

namespace LotSizingDataModel.Checker.Batch;

/// <summary>
/// Configures execution of a batch solution-verification campaign.
/// </summary>
public sealed class SolutionVerificationBatchOptions
{
    /// <summary>
    /// Gets or sets the maximum number of candidates verified concurrently.
    /// </summary>
    /// <remarks>
    /// The default is bounded to four workers to avoid excessive memory use
    /// while mathematical models are built for several candidates at once.
    /// </remarks>
    public int MaxDegreeOfParallelism
    {
        get;
        set;
    } = Math.Max(
        1,
        Math.Min(
            Environment.ProcessorCount,
            4));

    /// <summary>
    /// Gets or sets the verification policy applied independently to every
    /// candidate.
    /// </summary>
    public SolutionVerificationOptions VerificationOptions
    {
        get;
        set;
    } = new();

    /// <summary>
    /// Validates this batch configuration.
    /// </summary>
    public void EnsureValid()
    {
        if (MaxDegreeOfParallelism < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaxDegreeOfParallelism),
                MaxDegreeOfParallelism,
                "The maximum degree of parallelism must be at least one.");
        }

        if (VerificationOptions is null)
        {
            throw new InvalidOperationException(
                "VerificationOptions cannot be null.");
        }

        VerificationOptions.EnsureValid();
    }
}
