using LotSizingDataModel.Checker.Batch;
using LotSizingDataModel.Checker.Campaign;
using LotSizingDataModel.Checker.Common;
using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Reporting;

namespace LotSizingDataModel.Checker.Cli;

internal sealed class CliOptions
{
    public string InputDirectory { get; set; } = string.Empty;
    public string? OutputDirectory { get; set; }
    public string SearchPattern { get; set; } = "*.xml";
    public bool SearchSubdirectories { get; set; } = true;
    public bool IgnoreNonLotSizingInstanceXml { get; set; } = true;
    public bool WriteReports { get; set; } = true;
    public bool ShowProgress { get; set; } = true;
    public int MaxDegreeOfParallelism { get; set; } = Math.Max(1, Math.Min(Environment.ProcessorCount, 4));
    public string? KnownResultId { get; set; }
    public string? KnownResultNameContains { get; set; }
    public SolutionCheckOptions CheckOptions { get; } = new();
    public SolutionVerificationOptions VerificationOptions { get; } = new();
    public SolutionCheckReportOptions ReportOptions { get; } = new();

    public DirectoryVerificationCampaignOptions BuildCampaignOptions()
    {
        VerificationOptions.CheckOptions =
            CloneCheckOptions(CheckOptions);

        var campaignOptions =
            new DirectoryVerificationCampaignOptions
            {
                SearchPattern = SearchPattern,
                SearchSubdirectories = SearchSubdirectories,
                IgnoreNonLotSizingInstanceXml = IgnoreNonLotSizingInstanceXml,
                WriteReports = WriteReports,
                ReportOptions = CloneReportOptions(ReportOptions),
                BatchOptions =
                    new SolutionVerificationBatchOptions
                    {
                        MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                        VerificationOptions = CloneVerificationOptions(VerificationOptions)
                    }
            };

        if (!string.IsNullOrWhiteSpace(KnownResultId) ||
            !string.IsNullOrWhiteSpace(KnownResultNameContains))
        {
            string? requiredId = KnownResultId;
            string? requiredNameFragment = KnownResultNameContains;

            campaignOptions.KnownResultPredicate =
                result =>
                {
                    bool idMatches =
                        string.IsNullOrWhiteSpace(requiredId) ||
                        string.Equals(
                            result.ResultId,
                            requiredId,
                            StringComparison.OrdinalIgnoreCase);

                    bool nameMatches =
                        string.IsNullOrWhiteSpace(requiredNameFragment) ||
                        (result.Name ?? string.Empty).Contains(
                            requiredNameFragment,
                            StringComparison.OrdinalIgnoreCase);

                    return idMatches && nameMatches;
                };
        }

        campaignOptions.EnsureValid();
        return campaignOptions;
    }

    private static SolutionCheckOptions CloneCheckOptions(
        SolutionCheckOptions source)
    {
        return new SolutionCheckOptions
        {
            Level = source.Level,
            FeasibilityTolerance = source.FeasibilityTolerance,
            ZeroTolerance = source.ZeroTolerance,
            IntegralityTolerance = source.IntegralityTolerance,
            ObjectiveAbsoluteTolerance = source.ObjectiveAbsoluteTolerance,
            ObjectiveRelativeTolerance = source.ObjectiveRelativeTolerance,
            ReportedObjectiveValueOverride = source.ReportedObjectiveValueOverride,
            IgnoreDisabledConstraints = source.IgnoreDisabledConstraints,
            ContinueAfterStructuralErrors = source.ContinueAfterStructuralErrors
        };
    }

    private static SolutionVerificationOptions CloneVerificationOptions(
        SolutionVerificationOptions source)
    {
        return new SolutionVerificationOptions
        {
            CheckOptions = CloneCheckOptions(source.CheckOptions),
            ApplyToSolutionEvaluation = source.ApplyToSolutionEvaluation,
            UpdateKnownResultFeasibility = source.UpdateKnownResultFeasibility,
            PromoteFullyVerifiedKnownResult = source.PromoteFullyVerifiedKnownResult,
            EvaluatorName = source.EvaluatorName,
            EvaluatorVersion = source.EvaluatorVersion
        };
    }

    private static SolutionCheckReportOptions CloneReportOptions(
        SolutionCheckReportOptions source)
    {
        return new SolutionCheckReportOptions
        {
            IncludeStageDetails = source.IncludeStageDetails,
            IncludeFeasibilityMetrics = source.IncludeFeasibilityMetrics,
            IncludeObjectiveMetrics = source.IncludeObjectiveMetrics,
            IncludeInformationIssues = source.IncludeInformationIssues,
            IncludeWarningIssues = source.IncludeWarningIssues,
            IncludeErrorIssues = source.IncludeErrorIssues,
            MaximumDetailedIssues = source.MaximumDetailedIssues,
            SortIssuesBySeverity = source.SortIssuesBySeverity
        };
    }
}
