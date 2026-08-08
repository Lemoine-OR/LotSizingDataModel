namespace LotSizingDataModel.Checker.Campaign;

/// <summary>
/// Records the source metadata used to create one batch verification
/// candidate from a serialized lot-sizing instance.
/// </summary>
public sealed class DirectoryVerificationCandidateSource
{
    /// <summary>
    /// Gets the stable candidate key passed to the batch checker.
    /// </summary>
    public string CandidateKey
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    /// Gets the absolute source instance file path.
    /// </summary>
    public string SourceFilePath
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    /// Gets the source file path relative to the campaign input directory.
    /// </summary>
    public string RelativeSourceFilePath
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    /// Gets the instance identifier stored in the serialized instance.
    /// </summary>
    public string InstanceId
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    /// Gets the instance name stored in the serialized instance.
    /// </summary>
    public string? InstanceName
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the zero-based position of the known result in the source
    /// instance collection.
    /// </summary>
    public int KnownResultIndex
    {
        get;
        init;
    }

    /// <summary>
    /// Gets the source known-result identifier.
    /// </summary>
    public string KnownResultId
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    /// Gets the optional source known-result name.
    /// </summary>
    public string? KnownResultName
    {
        get;
        init;
    }
}
