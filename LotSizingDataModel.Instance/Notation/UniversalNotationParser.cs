using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors.Network;

namespace LotSizingDataModel.Instance.Notation;

/// <summary>
/// Parses and canonicalizes notation scheme version 1.
/// </summary>
public sealed class UniversalNotationParser
{
    public UniversalLotSizingNotation Parse(
        string text,
        string? schemeVersion = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new FormatException(
                "Universal notation cannot be empty.");
        }

        string version =
            schemeVersion ??
            UniversalNotationScheme.CurrentVersion;

        if (!UniversalNotationScheme.IsSupported(version))
        {
            throw new NotSupportedException(
                $"Universal notation scheme version '{version}' " +
                "is not supported.");
        }

        string[] sections =
            text.Split(
                '|',
                StringSplitOptions.TrimEntries);

        if (sections.Length != 3)
        {
            throw new FormatException(
                "Universal notation must contain exactly three " +
                "alpha | beta | gamma sections.");
        }

        return new UniversalLotSizingNotation(
            ParseAlpha(sections[0]),
            ParseBeta(sections[1]),
            ParseGamma(sections[2]),
            version);
    }

    public string Canonicalize(
        string text,
        string? schemeVersion = null)
    {
        return Parse(text, schemeVersion).Render();
    }

    private static UniversalNotationAlpha ParseAlpha(
        string section)
    {
        string[] tokens =
            section.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

        UniversalItemCardinality cardinality =
            UniversalItemCardinality.Unknown;

        UniversalProblemLevel level =
            UniversalProblemLevel.Unknown;

        ProductStructureType productStructureType =
            ProductStructureType.Unknown;

        UniversalNetworkNotation? network = null;

        foreach (string token in tokens)
        {
            if (token == "1")
            {
                cardinality =
                    UniversalItemCardinality.Single;
            }
            else if (
                token.Equals(
                    "m",
                    StringComparison.OrdinalIgnoreCase))
            {
                cardinality =
                    UniversalItemCardinality.Multiple;
            }
            else if (token == "?")
            {
                cardinality =
                    UniversalItemCardinality.Unknown;
            }
            else if (
                token.Equals(
                    "SL",
                    StringComparison.OrdinalIgnoreCase))
            {
                level =
                    UniversalProblemLevel.SingleLevel;

                productStructureType =
                    ProductStructureType.IndependentItems;
            }
            else if (
                token.StartsWith(
                    "ML:",
                    StringComparison.OrdinalIgnoreCase))
            {
                level =
                    UniversalProblemLevel.MultiLevel;

                productStructureType =
                    UniversalNotationTokenCatalog
                        .ParseProductStructureCode(
                            token[3..]);
            }
            else if (
                token.Equals(
                    "Level:?",
                    StringComparison.OrdinalIgnoreCase))
            {
                level =
                    UniversalProblemLevel.Unknown;
            }
            else if (
                token.StartsWith(
                    "Net:",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (network is not null)
                {
                    throw new FormatException(
                        "Alpha section contains more than one network token.");
                }

                network = ParseNetwork(token);
            }
            else
            {
                throw new FormatException(
                    $"Unknown alpha token '{token}'.");
            }
        }

        if (network is null)
        {
            throw new FormatException(
                "Alpha section must contain a network token.");
        }

        return new UniversalNotationAlpha
        {
            ItemCardinality = cardinality,
            ProblemLevel = level,
            ProductStructureType = productStructureType,
            Network = network
        };
    }

    private static UniversalNetworkNotation ParseNetwork(
        string token)
    {
        string working = token.Trim();

        NetworkCouplingType coupling;
        SupplyNetworkTopologyType forwardTopology;
        SupplyNetworkTopologyType? reverseTopology = null;
        string modifiersText = string.Empty;

        if (
            working.StartsWith(
                "Net:CL(",
                StringComparison.OrdinalIgnoreCase))
        {
            int closeIndex = working.IndexOf(')');

            if (closeIndex < 0)
            {
                throw new FormatException(
                    "Closed-loop network token is missing ')'.");
            }

            string core =
                working.Substring(
                    "Net:CL(".Length,
                    closeIndex - "Net:CL(".Length);

            string[] parts =
                core.Split(
                    ';',
                    StringSplitOptions.TrimEntries);

            if (parts.Length != 2 ||
                !parts[0].StartsWith(
                    "F:",
                    StringComparison.OrdinalIgnoreCase) ||
                !parts[1].StartsWith(
                    "R:",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException(
                    "Closed-loop network must use " +
                    "Net:CL(F:<topology>;R:<topology>).");
            }

            coupling = NetworkCouplingType.ClosedLoop;

            forwardTopology =
                UniversalNotationTokenCatalog
                    .ParseNetworkTopologyCode(
                        parts[0][2..]);

            reverseTopology =
                UniversalNotationTokenCatalog
                    .ParseNetworkTopologyCode(
                        parts[1][2..]);

            if (closeIndex + 1 < working.Length)
            {
                if (working[closeIndex + 1] != ':')
                {
                    throw new FormatException(
                        "Invalid network modifier separator.");
                }

                modifiersText =
                    working[(closeIndex + 2)..];
            }
        }
        else
        {
            string[] parts =
                working.Split(
                    ':',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries);

            if (parts.Length < 2 ||
                !parts[0].Equals(
                    "Net",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException(
                    $"Invalid network token '{token}'.");
            }

            int modifierStart;

            if (
                parts.Length >= 3 &&
                parts[1].Equals(
                    "R",
                    StringComparison.OrdinalIgnoreCase))
            {
                coupling = NetworkCouplingType.ReverseOnly;

                reverseTopology =
                    UniversalNotationTokenCatalog
                        .ParseNetworkTopologyCode(
                            parts[2]);

                forwardTopology =
                    SupplyNetworkTopologyType.Unknown;

                modifierStart = 3;
            }
            else
            {
                coupling = NetworkCouplingType.ForwardOnly;

                forwardTopology =
                    UniversalNotationTokenCatalog
                        .ParseNetworkTopologyCode(
                            parts[1]);

                modifierStart = 2;
            }

            modifiersText =
                string.Join(
                    ":",
                    parts.Skip(modifierStart));
        }

        int? echelonCount = null;
        bool hasCycles = false;
        bool hasMultiSourcing = false;
        bool hasTransshipment = false;

        if (!string.IsNullOrWhiteSpace(modifiersText))
        {
            foreach (
                string modifier
                in modifiersText.Split(
                    ':',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries))
            {
                if (
                    modifier.StartsWith(
                        "E",
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (!int.TryParse(
                            modifier[1..],
                            out int parsedEchelon) ||
                        parsedEchelon < 0)
                    {
                        throw new FormatException(
                            $"Invalid echelon modifier '{modifier}'.");
                    }

                    echelonCount = parsedEchelon;
                }
                else if (
                    modifier.Equals(
                        "CY",
                        StringComparison.OrdinalIgnoreCase))
                {
                    hasCycles = true;
                }
                else if (
                    modifier.Equals(
                        "MS",
                        StringComparison.OrdinalIgnoreCase))
                {
                    hasMultiSourcing = true;
                }
                else if (
                    modifier.Equals(
                        "TS",
                        StringComparison.OrdinalIgnoreCase))
                {
                    hasTransshipment = true;
                }
                else
                {
                    throw new FormatException(
                        $"Unknown network modifier '{modifier}'.");
                }
            }
        }

        return new UniversalNetworkNotation
        {
            Coupling = coupling,
            ForwardTopology = forwardTopology,
            ReverseTopology = reverseTopology,
            EchelonCount = echelonCount,
            HasCycles = hasCycles,
            HasMultiSourcing = hasMultiSourcing,
            HasTransshipment = hasTransshipment
        };
    }

    private static UniversalNotationBeta ParseBeta(
        string section)
    {
        if (
            section.Equals(
                "None",
                StringComparison.OrdinalIgnoreCase))
        {
            return new UniversalNotationBeta();
        }

        string[] tokens =
            section.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

        var features =
            new List<UniversalNotationFeature>();

        var temporalQualifiers =
            new List<UniversalTemporalQualifier>();

        foreach (string token in tokens)
        {
            if (
                UniversalNotationTokenCatalog
                    .TryParseTemporalQualifier(
                        token,
                        out UniversalTemporalQualifier? qualifier))
            {
                temporalQualifiers.Add(
                    qualifier!);

                continue;
            }

            if (!UniversalNotationTokenCatalog.TryParseFeature(
                    token,
                    out UniversalNotationFeature feature))
            {
                throw new FormatException(
                    $"Unknown beta token '{token}'.");
            }

            features.Add(feature);
        }

        return new UniversalNotationBeta(
            features,
            temporalQualifiers);
    }

    private static UniversalNotationGamma ParseGamma(
        string section)
    {
        UniversalObjectiveKind objective =
            section.Trim().ToUpperInvariant() switch
            {
                "OBJ:ECON" =>
                    UniversalObjectiveKind.Economic,
                "OBJ:MULTI" =>
                    UniversalObjectiveKind.MultipleObjectives,
                "OBJ:?" =>
                    UniversalObjectiveKind.Unknown,
                _ => throw new FormatException(
                    $"Unknown gamma token '{section.Trim()}'.")
            };

        return new UniversalNotationGamma
        {
            Objective = objective
        };
    }
}
