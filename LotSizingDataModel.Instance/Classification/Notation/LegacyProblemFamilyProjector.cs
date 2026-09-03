using System;

namespace LotSizingDataModel.Instance.Classification.Notation;

/// <summary>
/// Compatibility projection from LSI to the five historical
/// families already recognized by the legacy classifier.
/// </summary>
/// <remarks>
/// The projection never changes or overrides the existing classifier.
/// </remarks>
public static class LegacyProblemFamilyProjector
{
    public const string UncapacitatedSingleItemCode = "LS-U";
    public const string CapacitatedSingleItemCode = "LS-C";
    public const string CapacitatedLotSizingCode = "CLSP";
    public const string MultiLevelLotSizingCode = "MLLP";
    public const string MultiLevelCapacitatedLotSizingCode = "MLCLSP";

    public static LegacyProblemFamilyProjection Project(
        LotSizingInstanceSignature signature)
    {
        ArgumentNullException.ThrowIfNull(signature);

        bool? hasDemand =
            GetPresence(signature, LsiFeatureCodes.Demand);

        bool? hasProduction =
            GetPresence(signature, LsiFeatureCodes.Production);

        // Pack 02 signatures did not yet emit DEM/PROD tokens.
        // Structural usability therefore permits the compatibility
        // projection when those tokens are absent.
        if (hasDemand == false || hasProduction == false)
        {
            return Empty();
        }

        bool? capacitated =
            GetPresence(
                signature,
                LsiFeatureCodes.ProductionCapacity);

        if (!capacitated.HasValue)
        {
            return Empty();
        }

        string code = ResolveCode(
            signature.System.Items,
            signature.System.Levels,
            capacitated.Value);

        return string.IsNullOrEmpty(code)
            ? Empty()
            : new LegacyProblemFamilyProjection(
                new[] { code },
                code);
    }

    public static bool IsConsistentWith(
        LotSizingProblemClassification classification)
    {
        ArgumentNullException.ThrowIfNull(classification);

        LegacyProblemFamilyProjection projection =
            Project(classification.Signature);

        if (string.IsNullOrWhiteSpace(
                classification.PrimaryProblemTypeCode))
        {
            return !projection.HasProjection;
        }

        return string.Equals(
            classification.PrimaryProblemTypeCode,
            projection.PrimaryCode,
            StringComparison.OrdinalIgnoreCase);
    }

    private static LegacyProblemFamilyProjection Empty()
    {
        return new LegacyProblemFamilyProjection(
            Array.Empty<string>());
    }

    private static string ResolveCode(
        CardinalityKind items,
        CardinalityKind levels,
        bool capacitated)
    {
        if (items == CardinalityKind.Single &&
            levels == CardinalityKind.Single)
        {
            return capacitated
                ? CapacitatedSingleItemCode
                : UncapacitatedSingleItemCode;
        }

        if (items == CardinalityKind.Multiple &&
            levels == CardinalityKind.Single &&
            capacitated)
        {
            return CapacitatedLotSizingCode;
        }

        if (items == CardinalityKind.Multiple &&
            levels == CardinalityKind.Multiple)
        {
            return capacitated
                ? MultiLevelCapacitatedLotSizingCode
                : MultiLevelLotSizingCode;
        }

        return string.Empty;
    }

    private static bool? GetPresence(
        LotSizingInstanceSignature signature,
        string featureCode)
    {
        FeatureEntry? entry =
            signature.Features.Find(featureCode);

        // Missing DEM/PROD is treated as "not encoded by this
        // LSI revision", not as absent.
        if (entry is null)
        {
            return null;
        }

        if (entry.State == FeatureState.Unknown ||
            entry.State == FeatureState.Mixed)
        {
            return null;
        }

        if (entry.State == FeatureState.NotApplicable)
        {
            return false;
        }

        return entry.State == FeatureState.Present;
    }
}
