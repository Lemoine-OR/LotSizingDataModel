namespace LotSizingDataModel.Checker.Projection;

/// <summary>
/// Describes one issue encountered while projecting a business
/// solution onto mathematical-model variables.
/// </summary>
public sealed class MathematicalSolutionProjectionIssue
{
    /// <summary>
    /// Gets or sets the mathematical variable identifier.
    /// </summary>
    public int? VariableId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the mathematical variable name.
    /// </summary>
    public string? VariableName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the mathematical domain key.
    /// </summary>
    public string? DomainKey
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a human-readable diagnostic.
    /// </summary>
    public string Message
    {
        get;
        set;
    } = string.Empty;
}
