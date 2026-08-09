namespace LotSizingDataModel.Solver.External;

/// <summary>
/// Captures the observable result of an external solver process.
/// </summary>
public sealed class ExternalSolverProcessResult
{
    /// <summary>
    /// Gets or sets the process exit code.
    /// </summary>
    public int ExitCode
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the standard output text.
    /// </summary>
    public string StandardOutput
    {
        get;
        set;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the standard error text.
    /// </summary>
    public string StandardError
    {
        get;
        set;
    } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether cancellation requested process
    /// termination.
    /// </summary>
    public bool WasCancelled
    {
        get;
        set;
    }
}
