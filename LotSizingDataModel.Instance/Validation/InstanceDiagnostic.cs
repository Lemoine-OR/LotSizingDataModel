using System.Collections.ObjectModel;

namespace LotSizingDataModel.Instance.Validation;

/// <summary>
/// Represents one structured diagnostic produced while checking
/// the semantic validity of a lot-sizing instance.
/// </summary>
public sealed class InstanceDiagnostic
{
    private readonly IReadOnlyDictionary<string, string> _values;

    /// <summary>
    /// Initializes a structured instance diagnostic.
    /// </summary>
    public InstanceDiagnostic(
        string code,
        InstanceDiagnosticSeverity severity,
        string path,
        string message,
        string? relatedPath = null,
        IReadOnlyDictionary<string, string>? values = null,
        string? suggestedAction = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "A diagnostic code is required.",
                nameof(code));
        }

        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(message);

        Code = code.Trim();
        Severity = severity;
        Path = path;
        Message = message;
        RelatedPath = relatedPath;
        SuggestedAction = suggestedAction;

        Dictionary<string, string> copy =
            values is null
                ? new Dictionary<string, string>(
                    StringComparer.Ordinal)
                : new Dictionary<string, string>(
                    values,
                    StringComparer.Ordinal);

        _values =
            new ReadOnlyDictionary<string, string>(copy);
    }

    /// <summary>
    /// Gets the stable technical code identifying the rule.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the diagnostic severity.
    /// </summary>
    public InstanceDiagnosticSeverity Severity { get; }

    /// <summary>
    /// Gets the logical path of the primary model element.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the human-readable diagnostic message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets an optional related logical path.
    /// </summary>
    public string? RelatedPath { get; }

    /// <summary>
    /// Gets stable contextual values associated with the rule.
    /// </summary>
    public IReadOnlyDictionary<string, string> Values => _values;

    /// <summary>
    /// Gets an optional action suggested to the caller or UI.
    /// </summary>
    public string? SuggestedAction { get; }

    /// <summary>
    /// Gets a value indicating whether this diagnostic blocks
    /// the instance from being treated as semantically valid.
    /// </summary>
    public bool IsBlocking =>
        Severity is
            InstanceDiagnosticSeverity.Error or
            InstanceDiagnosticSeverity.Fatal;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[{Severity}] {Code} at {Path}: {Message}";
    }
}
