using System;
using System.Linq;
using System.Text;

namespace LotSizingDataModel.Instance.Classification.Notation;

/// <summary>
/// Produces a concise human-oriented LSI notation.
/// </summary>
public static class LotSizingSignatureCompactFormatter
{
    public static string Format(
        LotSizingInstanceSignature signature)
    {
        ArgumentNullException.ThrowIfNull(signature);

        var builder = new StringBuilder();

        builder.Append("LSI/");
        builder.Append(signature.NotationVersion);
        builder.Append(": ");

        AppendPlanning(builder, signature.Planning);
        builder.Append(" | ");
        AppendSystem(builder, signature.System);
        builder.Append(" | ");
        AppendFeatures(builder, signature.Features);
        builder.Append(" | ");
        AppendObjective(builder, signature.Objective);
        builder.Append(" @ ");
        builder.Append(signature.Size.Items);
        builder.Append('x');
        builder.Append(signature.Size.Periods);

        return builder.ToString();
    }

    private static void AppendPlanning(
        StringBuilder builder,
        PlanningSignature value)
    {
        builder.Append(
            value.TimeModel == TimeModelKind.Discrete
                ? "DT"
                : value.TimeModel == TimeModelKind.Continuous
                    ? "CT"
                    : "?");

        builder.Append(',');
        builder.Append(
            value.Information ==
                InformationStructureKind.Deterministic
                ? "DET"
                : value.Information ==
                    InformationStructureKind.Stochastic
                    ? "STO"
                    : "?");

        builder.Append(',');
        builder.Append(
            value.DemandPattern == DemandPatternKind.Dynamic
                ? "DYN"
                : value.DemandPattern ==
                    DemandPatternKind.Stationary
                    ? "STA"
                    : "?");
    }

    private static void AppendSystem(
        StringBuilder builder,
        SystemSignature value)
    {
        builder.Append(
            value.Items == CardinalityKind.Single
                ? "1I"
                : value.Items == CardinalityKind.Multiple
                    ? "mI"
                    : "?I");

        builder.Append(',');
        builder.Append(
            value.Levels == CardinalityKind.Single
                ? "1L"
                : value.Levels == CardinalityKind.Multiple
                    ? "mL"
                    : "?L");

        builder.Append(',');
        builder.Append(value.ProductStructure.ToString());
    }

    private static void AppendFeatures(
        StringBuilder builder,
        FeatureSignature value)
    {
        FeatureEntry[] active =
            value.Features
                .Where(entry =>
                    entry.State != FeatureState.Absent &&
                    entry.State != FeatureState.NotApplicable)
                .OrderBy(
                    entry => entry.Code,
                    StringComparer.Ordinal)
                .ToArray();

        if (active.Length == 0)
        {
            builder.Append('-');
            return;
        }

        builder.Append(
            string.Join(
                ",",
                active.Select(entry =>
                {
                    string text =
                        entry.Code + "=" +
                        LotSizingSignatureCanonicalFormatter
                            .StateCode(entry.State);

                    if (entry.TemporalProfile is not null)
                    {
                        text += "~" +
                            LotSizingSignatureCanonicalFormatter
                                .TemporalCode(
                                    entry.TemporalProfile.Kind);
                    }

                    return text;
                })));
    }

    private static void AppendObjective(
        StringBuilder builder,
        ObjectiveSignature value)
    {
        if (value.State == FeatureState.Unknown)
        {
            builder.Append('?');
            return;
        }

        builder.Append(value.Sense.ToString());

        if (value.Components.Count > 0)
        {
            builder.Append('{');
            builder.Append(
                string.Join(
                    ",",
                    value.Components
                        .OrderBy(item => (int)item)
                        .Select(item => item.ToString())));
            builder.Append('}');
        }
    }
}
