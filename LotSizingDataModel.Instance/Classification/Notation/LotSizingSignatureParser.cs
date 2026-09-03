using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LotSizingDataModel.Instance.Common;

namespace LotSizingDataModel.Instance.Classification.Notation;

/// <summary>
/// Parses the canonical LSI/1 representation produced by
/// <see cref="LotSizingSignatureCanonicalFormatter"/>.
/// </summary>
public static class LotSizingSignatureParser
{
    public static LotSizingInstanceSignature Parse(string text)
    {
        if (!TryParse(text, out LotSizingInstanceSignature? result, out string error))
        {
            throw new FormatException(error);
        }

        return result!;
    }

    public static bool TryParse(
        string? text,
        out LotSizingInstanceSignature? signature,
        out string error)
    {
        signature = null;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(text))
        {
            error = "An LSI notation string is required.";
            return false;
        }

        string trimmed = text.Trim();

        if (!trimmed.StartsWith("LSI/", StringComparison.Ordinal))
        {
            error = "The notation must start with 'LSI/'.";
            return false;
        }

        int colonIndex = trimmed.IndexOf(':');

        if (colonIndex <= 4)
        {
            error = "The LSI notation version separator ':' is missing.";
            return false;
        }

        string version =
            trimmed.Substring(4, colonIndex - 4).Trim();

        if (version.Length == 0)
        {
            error = "The LSI notation version is empty.";
            return false;
        }

        string body =
            trimmed.Substring(colonIndex + 1).Trim();

        string[] mainParts =
            body.Split(
                new[] { " | " },
                StringSplitOptions.None);

        if (mainParts.Length != 4)
        {
            error =
                "The canonical LSI body must contain pi, alpha, beta and gamma blocks.";
            return false;
        }

        int atIndex =
            mainParts[3].LastIndexOf(" @ ", StringComparison.Ordinal);

        if (atIndex < 0)
        {
            error = "The canonical LSI sigma block is missing.";
            return false;
        }

        string gammaText =
            mainParts[3].Substring(0, atIndex).Trim();

        string sigmaText =
            mainParts[3].Substring(atIndex + 3).Trim();

        try
        {
            var result = new LotSizingInstanceSignature
            {
                NotationVersion = version,
                Planning = ParsePlanning(mainParts[0]),
                System = ParseSystem(mainParts[1]),
                Features = ParseFeatures(mainParts[2]),
                Objective = ParseObjective(gammaText),
                Size = ParseSize(sigmaText)
            };

            signature = result;
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            signature = null;
            return false;
        }
    }

    private static PlanningSignature ParsePlanning(string text)
    {
        Dictionary<string, string> fields =
            ParseKeyValueBlock(text, "pi");

        return new PlanningSignature
        {
            Horizon =
                ParseHorizon(GetRequired(fields, "H")),

            TimeModel =
                ParseTimeModel(GetRequired(fields, "TM")),

            BucketStructure =
                ParseBucket(GetRequired(fields, "BK")),

            Information =
                ParseInformation(GetRequired(fields, "INF")),

            DemandPattern =
                ParseDemandPattern(GetRequired(fields, "DEM")),

            DemandSource =
                ParseDemandSource(GetRequired(fields, "DEM.SRC"))
        };
    }

    private static SystemSignature ParseSystem(string text)
    {
        Dictionary<string, string> fields =
            ParseKeyValueBlock(text, "alpha");

        return new SystemSignature
        {
            Items =
                ParseCardinality(GetRequired(fields, "I")),

            Levels =
                ParseCardinality(GetRequired(fields, "LV")),

            ProductStructure =
                ParseEnum<ProductStructureType>(
                    GetRequired(fields, "PS"),
                    ProductStructureType.Unknown),

            Network =
                ParseEnum<NetworkStructureKind>(
                    GetRequired(fields, "NET"),
                    NetworkStructureKind.Unknown),

            Routing =
                ParseEnum<RoutingStructureKind>(
                    GetRequired(fields, "ROUT"),
                    RoutingStructureKind.Unknown),

            ResourceEnvironment =
                ParseEnum<ResourceEnvironmentKind>(
                    GetRequired(fields, "RES"),
                    ResourceEnvironmentKind.Unknown)
        };
    }

    private static FeatureSignature ParseFeatures(string text)
    {
        string body = ExtractBlockBody(text, "beta");
        var signature = new FeatureSignature();

        if (string.IsNullOrWhiteSpace(body))
        {
            return signature;
        }

        foreach (string token in SplitTopLevel(body, ','))
        {
            int equalsIndex = token.IndexOf('=');

            if (equalsIndex <= 0)
            {
                throw new FormatException(
                    "Invalid LSI beta feature entry: '" +
                    token +
                    "'.");
            }

            string code =
                token.Substring(0, equalsIndex).Trim();

            string valueText =
                token.Substring(equalsIndex + 1).Trim();

            string stateText = valueText;
            string? temporalText = null;

            int tildeIndex = valueText.IndexOf('~');

            if (tildeIndex >= 0)
            {
                stateText =
                    valueText.Substring(0, tildeIndex).Trim();

                temporalText =
                    valueText.Substring(tildeIndex + 1).Trim();
            }

            FeatureState state = ParseFeatureState(stateText);
            TemporalProfile? profile = null;

            if (!string.IsNullOrWhiteSpace(temporalText))
            {
                profile = ParseTemporalProfile(temporalText);
            }

            signature.Set(code, state, profile);
        }

        return signature;
    }

    private static ObjectiveSignature ParseObjective(string text)
    {
        string body = ExtractBlockBody(text, "gamma");

        if (body == "?")
        {
            return new ObjectiveSignature
            {
                State = FeatureState.Unknown
            };
        }

        string[] tokens = SplitTopLevel(body, ',');

        if (tokens.Length == 0)
        {
            return new ObjectiveSignature
            {
                State = FeatureState.Unknown
            };
        }

        var result = new ObjectiveSignature
        {
            State = ParseFeatureState(tokens[0].Trim())
        };

        for (int index = 1; index < tokens.Length; index++)
        {
            string token = tokens[index];
            int equalsIndex = token.IndexOf('=');

            if (equalsIndex <= 0)
            {
                throw new FormatException(
                    "Invalid LSI gamma entry: '" +
                    token +
                    "'.");
            }

            string key =
                token.Substring(0, equalsIndex).Trim();

            string value =
                token.Substring(equalsIndex + 1).Trim();

            if (string.Equals(
                    key,
                    "SENSE",
                    StringComparison.Ordinal))
            {
                result.Sense =
                    ParseEnum<ObjectiveSenseKind>(
                        value,
                        ObjectiveSenseKind.Unknown);
            }
            else if (string.Equals(
                         key,
                         "AGG",
                         StringComparison.Ordinal))
            {
                result.Aggregation =
                    ParseEnum<ObjectiveAggregationKind>(
                        value,
                        ObjectiveAggregationKind.Unknown);
            }
            else if (string.Equals(
                         key,
                         "OBJ",
                         StringComparison.Ordinal))
            {
                string componentsBody =
                    ExtractAnonymousBraceBody(value);

                result.ReplaceComponents(
                    SplitTopLevel(componentsBody, ',')
                        .Select(component =>
                            ParseEnum<ObjectiveComponentKind>(
                                component.Trim(),
                                ObjectiveComponentKind.Unknown)));
            }
        }

        return result;
    }

    private static InstanceSizeSignature ParseSize(string text)
    {
        Dictionary<string, string> fields =
            ParseKeyValueBlock(text, "sigma");

        return new InstanceSizeSignature
        {
            Periods = ParseNonNegativeInt(fields, "T"),
            Items = ParseNonNegativeInt(fields, "I"),
            Plants = ParseNonNegativeInt(fields, "P"),
            WorkCenters = ParseNonNegativeInt(fields, "WC"),
            Warehouses = ParseNonNegativeInt(fields, "WH"),
            Suppliers = ParseNonNegativeInt(fields, "SUP"),
            DistributionCenters = ParseNonNegativeInt(fields, "DC"),
            TransportResources = ParseNonNegativeInt(fields, "TR"),
            BomRelationships = ParseNonNegativeInt(fields, "BOM"),
            MaximumBomDepth = ParseNonNegativeInt(fields, "DEPTH")
        };
    }

    private static TemporalProfile ParseTemporalProfile(string text)
    {
        int braceIndex = text.IndexOf('{');

        string kindText =
            braceIndex >= 0
                ? text.Substring(0, braceIndex).Trim()
                : text.Trim();

        var result = new TemporalProfile
        {
            Kind = ParseTemporalKind(kindText)
        };

        if (braceIndex >= 0)
        {
            string body =
                ExtractAnonymousBraceBody(
                    text.Substring(braceIndex));

            result.ReplaceComponents(
                SplitTopLevel(body, ',')
                    .Select(value =>
                        ParseTemporalKind(value.Trim())));
        }

        return result;
    }

    private static Dictionary<string, string> ParseKeyValueBlock(
        string text,
        string blockName)
    {
        string body = ExtractBlockBody(text, blockName);
        var result =
            new Dictionary<string, string>(
                StringComparer.Ordinal);

        if (string.IsNullOrWhiteSpace(body))
        {
            return result;
        }

        foreach (string token in SplitTopLevel(body, ','))
        {
            int equalsIndex = token.IndexOf('=');

            if (equalsIndex <= 0)
            {
                throw new FormatException(
                    "Invalid " +
                    blockName +
                    " entry: '" +
                    token +
                    "'.");
            }

            string key =
                token.Substring(0, equalsIndex).Trim();

            string value =
                token.Substring(equalsIndex + 1).Trim();

            if (!result.TryAdd(key, value))
            {
                throw new FormatException(
                    "Duplicate " +
                    blockName +
                    " field '" +
                    key +
                    "'.");
            }
        }

        return result;
    }

    private static string ExtractBlockBody(
        string text,
        string blockName)
    {
        string trimmed = text.Trim();
        string prefix = blockName + "{";

        if (!trimmed.StartsWith(prefix, StringComparison.Ordinal) ||
            !trimmed.EndsWith("}", StringComparison.Ordinal))
        {
            throw new FormatException(
                "Expected canonical LSI block '" +
                blockName +
                "{...}'.");
        }

        return trimmed.Substring(
            prefix.Length,
            trimmed.Length - prefix.Length - 1);
    }

    private static string ExtractAnonymousBraceBody(string text)
    {
        string trimmed = text.Trim();

        if (!trimmed.StartsWith("{", StringComparison.Ordinal) ||
            !trimmed.EndsWith("}", StringComparison.Ordinal))
        {
            throw new FormatException(
                "Expected a brace-delimited LSI value.");
        }

        return trimmed.Substring(1, trimmed.Length - 2);
    }

    private static string[] SplitTopLevel(string text, char separator)
    {
        var result = new List<string>();
        int depth = 0;
        int start = 0;

        for (int index = 0; index < text.Length; index++)
        {
            char current = text[index];

            if (current == '{')
            {
                depth++;
            }
            else if (current == '}')
            {
                depth--;

                if (depth < 0)
                {
                    throw new FormatException(
                        "Unbalanced braces in LSI notation.");
                }
            }
            else if (current == separator && depth == 0)
            {
                result.Add(
                    text.Substring(start, index - start).Trim());

                start = index + 1;
            }
        }

        if (depth != 0)
        {
            throw new FormatException(
                "Unbalanced braces in LSI notation.");
        }

        result.Add(text.Substring(start).Trim());

        return result
            .Where(item => item.Length > 0)
            .ToArray();
    }

    private static string GetRequired(
        IReadOnlyDictionary<string, string> fields,
        string key)
    {
        if (!fields.TryGetValue(key, out string? value))
        {
            throw new FormatException(
                "Required LSI field '" +
                key +
                "' is missing.");
        }

        return value;
    }

    private static int ParseNonNegativeInt(
        IReadOnlyDictionary<string, string> fields,
        string key)
    {
        string value = GetRequired(fields, key);

        if (!int.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int result) ||
            result < 0)
        {
            throw new FormatException(
                "LSI size field '" +
                key +
                "' must be a non-negative integer.");
        }

        return result;
    }

    private static FeatureState ParseFeatureState(string text)
    {
        return text switch
        {
            "0" => FeatureState.Absent,
            "1" => FeatureState.Present,
            "NA" => FeatureState.NotApplicable,
            "MIX" => FeatureState.Mixed,
            "?" => FeatureState.Unknown,
            _ => throw new FormatException(
                "Unknown LSI feature state '" +
                text +
                "'.")
        };
    }

    private static TemporalProfileKind ParseTemporalKind(string text)
    {
        return text switch
        {
            "Z" => TemporalProfileKind.Zero,
            "C" => TemporalProfileKind.Constant,
            "NI" => TemporalProfileKind.NonIncreasing,
            "ND" => TemporalProfileKind.NonDecreasing,
            "G" => TemporalProfileKind.General,
            "PER" => TemporalProfileKind.Periodic,
            "MIX" => TemporalProfileKind.Mixed,
            "NA" => TemporalProfileKind.NotApplicable,
            "?" => TemporalProfileKind.Unknown,
            _ => throw new FormatException(
                "Unknown LSI temporal profile '" +
                text +
                "'.")
        };
    }

    private static PlanningHorizonKind ParseHorizon(string text)
    {
        return text switch
        {
            "F" => PlanningHorizonKind.Finite,
            "INF" => PlanningHorizonKind.Infinite,
            "RH" => PlanningHorizonKind.Rolling,
            "?" => PlanningHorizonKind.Unknown,
            _ => throw new FormatException(
                "Unknown LSI horizon code '" +
                text +
                "'.")
        };
    }

    private static TimeModelKind ParseTimeModel(string text)
    {
        return text switch
        {
            "DT" => TimeModelKind.Discrete,
            "CT" => TimeModelKind.Continuous,
            "HYB" => TimeModelKind.Hybrid,
            "?" => TimeModelKind.Unknown,
            _ => throw new FormatException(
                "Unknown LSI time-model code '" +
                text +
                "'.")
        };
    }

    private static BucketStructureKind ParseBucket(string text)
    {
        return text switch
        {
            "NA" => BucketStructureKind.NotApplicable,
            "BB" => BucketStructureKind.BigBucket,
            "SB" => BucketStructureKind.SmallBucket,
            "HYB" => BucketStructureKind.Hybrid,
            "MM" => BucketStructureKind.MacroMicro,
            "?" => BucketStructureKind.Unknown,
            _ => throw new FormatException(
                "Unknown LSI bucket code '" +
                text +
                "'.")
        };
    }

    private static InformationStructureKind ParseInformation(string text)
    {
        return text switch
        {
            "DET" => InformationStructureKind.Deterministic,
            "STO" => InformationStructureKind.Stochastic,
            "ROB" => InformationStructureKind.Robust,
            "FUZ" => InformationStructureKind.Fuzzy,
            "HYB" => InformationStructureKind.Hybrid,
            "?" => InformationStructureKind.Unknown,
            _ => throw new FormatException(
                "Unknown LSI information code '" +
                text +
                "'.")
        };
    }

    private static DemandPatternKind ParseDemandPattern(string text)
    {
        return text switch
        {
            "STA" => DemandPatternKind.Stationary,
            "DYN" => DemandPatternKind.Dynamic,
            "END" => DemandPatternKind.Endogenous,
            "MIX" => DemandPatternKind.Mixed,
            "?" => DemandPatternKind.Unknown,
            _ => throw new FormatException(
                "Unknown LSI demand-pattern code '" +
                text +
                "'.")
        };
    }

    private static DemandSourceKind ParseDemandSource(string text)
    {
        return text switch
        {
            "EXO" => DemandSourceKind.Exogenous,
            "ENDO" => DemandSourceKind.Endogenous,
            "MIX" => DemandSourceKind.Mixed,
            "?" => DemandSourceKind.Unknown,
            _ => throw new FormatException(
                "Unknown LSI demand-source code '" +
                text +
                "'.")
        };
    }

    private static CardinalityKind ParseCardinality(string text)
    {
        return text switch
        {
            "0" => CardinalityKind.None,
            "1" => CardinalityKind.Single,
            "m" => CardinalityKind.Multiple,
            "?" => CardinalityKind.Unknown,
            _ => throw new FormatException(
                "Unknown LSI cardinality code '" +
                text +
                "'.")
        };
    }

    private static T ParseEnum<T>(string text, T unknown)
        where T : struct, Enum
    {
        if (Enum.TryParse(
                text,
                ignoreCase: false,
                out T result))
        {
            return result;
        }

        if (text == "?")
        {
            return unknown;
        }

        throw new FormatException(
            "Unknown LSI " +
            typeof(T).Name +
            " value '" +
            text +
            "'.");
    }
}
