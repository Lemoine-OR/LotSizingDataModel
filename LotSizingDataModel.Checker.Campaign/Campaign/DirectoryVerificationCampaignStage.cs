namespace LotSizingDataModel.Checker.Campaign;

/// <summary>
/// Identifies the current high-level stage of a directory verification
/// campaign.
/// </summary>
public enum DirectoryVerificationCampaignStage
{
    /// <summary>
    /// Input files are being discovered.
    /// </summary>
    DiscoveringFiles = 0,

    /// <summary>
    /// Discovered XML files are being inspected and loaded.
    /// </summary>
    LoadingFiles = 1,

    /// <summary>
    /// Selected detailed solutions are being checked.
    /// </summary>
    VerifyingCandidates = 2,

    /// <summary>
    /// Campaign reports are being written.
    /// </summary>
    WritingReports = 3,

    /// <summary>
    /// The campaign execution reached its normal end.
    /// </summary>
    Completed = 4
}
