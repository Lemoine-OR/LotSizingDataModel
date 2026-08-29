namespace LotSizingDataModel.Instance.Notation;

/// <summary>
/// Identifies the versioned LotSizingDataModel universal notation scheme.
/// </summary>
public static class UniversalNotationScheme
{
    public const string Id = "LSDM";
    public const string CurrentVersion = "1";

    public static bool IsSupported(string version)
    {
        return string.Equals(
            version,
            CurrentVersion,
            StringComparison.Ordinal);
    }
}
