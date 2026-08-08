namespace LotSizingDataModel.Checker.Campaign;

/// <summary>
/// Describes one serialized instance file that could not be loaded.
/// </summary>
public sealed class InstanceFileLoadFailure
{
    /// <summary>
    /// Gets the absolute input file path.
    /// </summary>
    public string FilePath
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    /// Gets the deterministic path relative to the campaign input directory.
    /// </summary>
    public string RelativeFilePath
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    /// Gets the fully qualified exception type.
    /// </summary>
    public string ExceptionType
    {
        get;
        init;
    } = string.Empty;

    /// <summary>
    /// Gets the load failure message.
    /// </summary>
    public string Message
    {
        get;
        init;
    } = string.Empty;
}
