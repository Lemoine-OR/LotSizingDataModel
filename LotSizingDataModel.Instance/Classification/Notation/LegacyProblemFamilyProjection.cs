using System;
using System.Collections.Generic;
using System.Linq;

namespace LotSizingDataModel.Instance.Classification.Notation;

/// <summary>
/// Result of projecting an LSI signature onto historical
/// lot-sizing family identifiers.
/// </summary>
public sealed class LegacyProblemFamilyProjection
{
    public LegacyProblemFamilyProjection(
        IEnumerable<string> codes,
        string primaryCode = "")
    {
        ArgumentNullException.ThrowIfNull(codes);

        Codes =
            codes
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Select(code => code.Trim().ToUpperInvariant())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(code => code, StringComparer.Ordinal)
                .ToArray();

        PrimaryCode =
            primaryCode?.Trim().ToUpperInvariant()
            ?? string.Empty;
    }

    public IReadOnlyList<string> Codes { get; }

    public string PrimaryCode { get; }

    public bool HasProjection => Codes.Count > 0;
}
