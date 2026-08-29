namespace LotSizingDataModel.Instance.Scientific;

/// <summary>
/// Describes what the current engine can safely do with one historical
/// classification scheme.
/// </summary>
public sealed class HistoricalClassificationCapability
{
    internal HistoricalClassificationCapability(
        string code,
        string name,
        HistoricalClassificationCapabilityKind kind,
        bool canMapDeclaredClassification,
        bool canDetectCompleteHistoricalCode,
        bool requiresExplicitParameterProfile,
        string note)
    {
        Code = code;
        Name = name;
        Kind = kind;
        CanMapDeclaredClassification =
            canMapDeclaredClassification;
        CanDetectCompleteHistoricalCode =
            canDetectCompleteHistoricalCode;
        RequiresExplicitParameterProfile =
            requiresExplicitParameterProfile;
        Note = note;
    }

    public string Code { get; }
    public string Name { get; }
    public HistoricalClassificationCapabilityKind Kind { get; }

    public bool CanMapDeclaredClassification { get; }

    /// <summary>
    /// Gets whether this engine can infer the complete historical source code
    /// from the current coarse descriptor alone.
    /// </summary>
    public bool CanDetectCompleteHistoricalCode { get; }

    public bool RequiresExplicitParameterProfile { get; }
    public string Note { get; }
}
