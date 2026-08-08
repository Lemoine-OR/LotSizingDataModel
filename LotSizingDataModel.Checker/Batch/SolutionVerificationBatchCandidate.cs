using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.Results;
using LotSizingDataModel.Solution;

namespace LotSizingDataModel.Checker.Batch;

/// <summary>
/// Describes one candidate to verify in a batch campaign.
/// </summary>
/// <remarks>
/// A candidate represents either a standalone solution or the detailed
/// solution of a known result. Factory methods guarantee that the two forms
/// cannot be mixed accidentally.
/// </remarks>
public sealed class SolutionVerificationBatchCandidate
{
    private SolutionVerificationBatchCandidate(
        string candidateKey,
        string? candidateName,
        LotSizingInstance instance,
        LotSizingSolution? solution,
        KnownResult? knownResult,
        SolutionVerificationBatchCandidateKind kind)
    {
        CandidateKey =
            ValidateCandidateKey(candidateKey);

        CandidateName =
            string.IsNullOrWhiteSpace(candidateName)
                ? null
                : candidateName;

        Instance =
            instance ??
            throw new ArgumentNullException(nameof(instance));

        Solution =
            solution;

        KnownResult =
            knownResult;

        Kind =
            kind;
    }

    /// <summary>
    /// Gets the stable caller-defined identifier used to correlate results
    /// with the input candidate.
    /// </summary>
    public string CandidateKey
    {
        get;
    }

    /// <summary>
    /// Gets an optional human-readable candidate name.
    /// </summary>
    public string? CandidateName
    {
        get;
    }

    /// <summary>
    /// Gets the lot-sizing instance against which the candidate is checked.
    /// </summary>
    public LotSizingInstance Instance
    {
        get;
    }

    /// <summary>
    /// Gets the standalone solution when <see cref="Kind"/> is
    /// <see cref="SolutionVerificationBatchCandidateKind.StandaloneSolution"/>.
    /// </summary>
    public LotSizingSolution? Solution
    {
        get;
    }

    /// <summary>
    /// Gets the known result when <see cref="Kind"/> is
    /// <see cref="SolutionVerificationBatchCandidateKind.KnownResult"/>.
    /// </summary>
    public KnownResult? KnownResult
    {
        get;
    }

    /// <summary>
    /// Gets the candidate representation kind.
    /// </summary>
    public SolutionVerificationBatchCandidateKind Kind
    {
        get;
    }

    /// <summary>
    /// Creates a candidate from a standalone solution.
    /// </summary>
    /// <param name="candidateKey">Stable caller-defined candidate identifier.</param>
    /// <param name="instance">Instance defining the problem.</param>
    /// <param name="solution">Standalone candidate solution.</param>
    /// <param name="candidateName">Optional human-readable candidate name.</param>
    /// <returns>A validated standalone-solution candidate.</returns>
    public static SolutionVerificationBatchCandidate ForSolution(
        string candidateKey,
        LotSizingInstance instance,
        LotSizingSolution solution,
        string? candidateName = null)
    {
        ArgumentNullException.ThrowIfNull(solution);

        return new SolutionVerificationBatchCandidate(
            candidateKey,
            candidateName,
            instance,
            solution,
            knownResult: null,
            SolutionVerificationBatchCandidateKind.StandaloneSolution);
    }

    /// <summary>
    /// Creates a candidate from a known result containing a detailed solution.
    /// </summary>
    /// <param name="candidateKey">Stable caller-defined candidate identifier.</param>
    /// <param name="instance">Instance defining the problem.</param>
    /// <param name="knownResult">Known result to verify.</param>
    /// <param name="candidateName">Optional human-readable candidate name.</param>
    /// <returns>A validated known-result candidate.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the known result has no detailed solution.
    /// </exception>
    public static SolutionVerificationBatchCandidate ForKnownResult(
        string candidateKey,
        LotSizingInstance instance,
        KnownResult knownResult,
        string? candidateName = null)
    {
        ArgumentNullException.ThrowIfNull(knownResult);

        if (knownResult.DetailedSolution is null)
        {
            throw new InvalidOperationException(
                "The known result does not contain a detailed solution " +
                "that can be independently checked.");
        }

        return new SolutionVerificationBatchCandidate(
            candidateKey,
            candidateName,
            instance,
            solution: null,
            knownResult,
            SolutionVerificationBatchCandidateKind.KnownResult);
    }

    private static string ValidateCandidateKey(
        string candidateKey)
    {
        if (string.IsNullOrWhiteSpace(candidateKey))
        {
            throw new ArgumentException(
                "The candidate key cannot be empty.",
                nameof(candidateKey));
        }

        return candidateKey;
    }
}
