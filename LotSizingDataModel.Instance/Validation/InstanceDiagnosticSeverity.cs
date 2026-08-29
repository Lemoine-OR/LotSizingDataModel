namespace LotSizingDataModel.Instance.Validation;

/// <summary>
/// Indicates the severity of one semantic instance diagnostic.
/// </summary>
public enum InstanceDiagnosticSeverity
{
    /// <summary>
    /// Informational diagnostic that does not affect validity.
    /// </summary>
    Information,

    /// <summary>
    /// Non-blocking issue that deserves user attention.
    /// </summary>
    Warning,

    /// <summary>
    /// Blocking issue that prevents the instance from being
    /// considered semantically valid.
    /// </summary>
    Error,

    /// <summary>
    /// Blocking issue indicating that meaningful validation
    /// cannot safely continue.
    /// </summary>
    Fatal
}
