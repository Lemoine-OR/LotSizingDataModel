namespace LotSizingDataModel.Checker.Projection;

/// <summary>
/// Contains mathematical-variable values projected from one
/// normalized lot-sizing solution.
/// </summary>
public sealed class MathematicalSolutionProjectionResult
{
    private readonly Dictionary<int, double> _values =
        new();

    private readonly List<MathematicalSolutionProjectionIssue> _issues =
        new();

    /// <summary>
    /// Gets the projected values indexed by mathematical variable
    /// identifier.
    /// </summary>
    public IReadOnlyDictionary<int, double> Values =>
        _values;

    /// <summary>
    /// Gets all projection diagnostics.
    /// </summary>
    public IReadOnlyList<MathematicalSolutionProjectionIssue> Issues =>
        _issues;

    /// <summary>
    /// Gets whether every mathematical variable was projected
    /// successfully.
    /// </summary>
    public bool IsSuccessful =>
        _issues.Count == 0;

    /// <summary>
    /// Gets the number of projected variables.
    /// </summary>
    public int ProjectedVariableCount =>
        _values.Count;

    /// <summary>
    /// Adds one projected mathematical-variable value.
    /// </summary>
    /// <param name="variableId">Variable identifier.</param>
    /// <param name="value">Projected value.</param>
    public void AddValue(
        int variableId,
        double value)
    {
        if (variableId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(variableId),
                variableId,
                "A mathematical variable identifier must be positive.");
        }

        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "A projected mathematical value must be finite.");
        }

        if (!_values.TryAdd(
                variableId,
                value))
        {
            throw new InvalidOperationException(
                $"A projected value already exists for mathematical " +
                $"variable '{variableId}'.");
        }
    }

    /// <summary>
    /// Adds one projection issue.
    /// </summary>
    /// <param name="issue">Issue to append.</param>
    public void AddIssue(
        MathematicalSolutionProjectionIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);

        _issues.Add(issue);
    }

    /// <summary>
    /// Attempts to get the projected value of one mathematical
    /// variable.
    /// </summary>
    public bool TryGetValue(
        int variableId,
        out double value)
    {
        return _values.TryGetValue(
            variableId,
            out value);
    }
}
