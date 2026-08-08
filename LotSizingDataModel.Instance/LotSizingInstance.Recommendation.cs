using System;
using System.Xml.Serialization;
using LotSizingDataModel.Instance.Recommendation;

namespace LotSizingDataModel.Instance;

/// <summary>
/// Extends <see cref="LotSizingInstance"/> with
/// solution-method recommendation information.
/// </summary>
public sealed partial class LotSizingInstance
{
    private SolutionMethodRecommendationReport
        _solutionMethodRecommendationReport =
            new();

    /// <summary>
    /// Gets or sets the solution-method recommendation report
    /// generated for this problem instance.
    /// </summary>
    /// <remarks>
    /// The report records the evaluated methods, their
    /// compatibility levels, their ranks and the supply-chain
    /// fingerprint used during the evaluation.
    ///
    /// An empty report indicates that no recommendation has
    /// yet been generated.
    /// </remarks>
    [XmlElement("solutionMethodRecommendationReport")]
    public SolutionMethodRecommendationReport
        SolutionMethodRecommendationReport
    {
        get =>
            _solutionMethodRecommendationReport;

        set
        {
            SolutionMethodRecommendationReport
                normalizedValue =
                    value ??
                    new SolutionMethodRecommendationReport();

            if (SetProperty(
                    ref _solutionMethodRecommendationReport,
                    normalizedValue))
            {
                NotifySolutionMethodRecommendationProperties();
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether a recommendation
    /// report has been generated or contains recommendations.
    /// </summary>
    [XmlIgnore]
    public bool HasSolutionMethodRecommendationReport =>
        SolutionMethodRecommendationReport.HasBeenGenerated ||
        SolutionMethodRecommendationReport.HasRecommendations;

    /// <summary>
    /// Gets a value indicating whether at least one
    /// solution-method recommendation is available.
    /// </summary>
    [XmlIgnore]
    public bool HasSolutionMethodRecommendations =>
        SolutionMethodRecommendationReport.HasRecommendations;

    /// <summary>
    /// Gets the number of solution-method recommendations
    /// available for this instance.
    /// </summary>
    [XmlIgnore]
    public int SolutionMethodRecommendationCount =>
        SolutionMethodRecommendationReport
            .RecommendationCount;

    /// <summary>
    /// Gets the number of methods marked as recommended.
    /// </summary>
    [XmlIgnore]
    public int RecommendedSolutionMethodCount =>
        SolutionMethodRecommendationReport
            .RecommendedMethodCount;

    /// <summary>
    /// Gets a value indicating whether at least one method is
    /// directly compatible with the complete problem.
    /// </summary>
    [XmlIgnore]
    public bool HasDirectlyCompatibleSolutionMethod =>
        SolutionMethodRecommendationReport
            .HasDirectlyCompatibleMethod;

    /// <summary>
    /// Gets a value indicating whether at least one method is
    /// directly applicable or applicable after adaptation.
    /// </summary>
    [XmlIgnore]
    public bool HasApplicableSolutionMethod =>
        SolutionMethodRecommendationReport
            .HasApplicableMethod;

    /// <summary>
    /// Gets the best applicable solution-method
    /// recommendation.
    /// </summary>
    [XmlIgnore]
    public SolutionMethodRecommendation?
        BestSolutionMethodRecommendation =>
            SolutionMethodRecommendationReport
                .BestRecommendation;

    /// <summary>
    /// Gets a value indicating whether a best applicable
    /// solution-method recommendation exists.
    /// </summary>
    [XmlIgnore]
    public bool HasBestSolutionMethodRecommendation =>
        BestSolutionMethodRecommendation is not null;

    /// <summary>
    /// Replaces the complete solution-method recommendation
    /// report.
    /// </summary>
    /// <param name="report">
    /// New recommendation report.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="report"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the supplied report is invalid.
    /// </exception>
    public void ReplaceSolutionMethodRecommendationReport(
        SolutionMethodRecommendationReport report)
    {
        ArgumentNullException.ThrowIfNull(
            report);

        report.EnsureValid();

        SolutionMethodRecommendationReport =
            report;
    }

    /// <summary>
    /// Determines whether the recommendation report was
    /// generated for the supplied supply-chain fingerprint.
    /// </summary>
    /// <param name="currentSupplyChainFingerprint">
    /// Current supply-chain fingerprint.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the report has been
    /// generated and its fingerprint matches the supplied
    /// fingerprint; otherwise, <see langword="false"/>.
    /// </returns>
    public bool
        SolutionMethodRecommendationsMatchFingerprint(
            string currentSupplyChainFingerprint)
    {
        return
            SolutionMethodRecommendationReport
                .HasBeenGenerated &&
            SolutionMethodRecommendationReport
                .MatchesSupplyChainFingerprint(
                    currentSupplyChainFingerprint);
    }

    /// <summary>
    /// Clears the complete solution-method recommendation
    /// report.
    /// </summary>
    /// <remarks>
    /// This method should be called whenever the supply-chain
    /// data or the derived problem classification changes.
    /// </remarks>
    public void ClearSolutionMethodRecommendationReport()
    {
        SolutionMethodRecommendationReport
            .ClearReport();

        NotifySolutionMethodRecommendationProperties();
    }

    /// <summary>
    /// Determines whether the recommendation report must be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the report has been
    /// generated or contains recommendations; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method follows the naming convention recognized
    /// by <see cref="XmlSerializer"/>.
    /// </remarks>
    public bool
        ShouldSerializeSolutionMethodRecommendationReport()
    {
        return HasSolutionMethodRecommendationReport;
    }

    private void
        NotifySolutionMethodRecommendationProperties()
    {
        OnPropertyChanged(
            nameof(
                SolutionMethodRecommendationReport));

        OnPropertyChanged(
            nameof(
                HasSolutionMethodRecommendationReport));

        OnPropertyChanged(
            nameof(
                HasSolutionMethodRecommendations));

        OnPropertyChanged(
            nameof(
                SolutionMethodRecommendationCount));

        OnPropertyChanged(
            nameof(
                RecommendedSolutionMethodCount));

        OnPropertyChanged(
            nameof(
                HasDirectlyCompatibleSolutionMethod));

        OnPropertyChanged(
            nameof(
                HasApplicableSolutionMethod));

        OnPropertyChanged(
            nameof(
                BestSolutionMethodRecommendation));

        OnPropertyChanged(
            nameof(
                HasBestSolutionMethodRecommendation));
    }
}