using System;
using System.Collections.Generic;
using System.Linq;

namespace LotSizingDataModel.Instance.Classification.Notation;

/// <summary>
/// Summarizes LSI coverage over a collection of signatures.
/// </summary>
public sealed class LsiSignatureCoverageReport
{
    public int SignatureCount { get; init; }

    public int CompletePlanningCount { get; init; }

    public int KnownObjectiveCount { get; init; }

    public int LegacyProjectedCount { get; init; }

    public IReadOnlyDictionary<string, int> UnknownFieldCounts
    {
        get;
        init;
    } = new Dictionary<string, int>();

    public static LsiSignatureCoverageReport Analyze(
        IEnumerable<LotSizingInstanceSignature> signatures)
    {
        ArgumentNullException.ThrowIfNull(signatures);

        LotSizingInstanceSignature[] items =
            signatures.ToArray();

        var unknown =
            new Dictionary<string, int>(
                StringComparer.Ordinal);

        foreach (LotSizingInstanceSignature signature in items)
        {
            CountUnknown(
                unknown,
                "pi.H",
                signature.Planning.Horizon ==
                    PlanningHorizonKind.Unknown);

            CountUnknown(
                unknown,
                "pi.TM",
                signature.Planning.TimeModel ==
                    TimeModelKind.Unknown);

            CountUnknown(
                unknown,
                "pi.BK",
                signature.Planning.BucketStructure ==
                    BucketStructureKind.Unknown);

            CountUnknown(
                unknown,
                "pi.INF",
                signature.Planning.Information ==
                    InformationStructureKind.Unknown);

            CountUnknown(
                unknown,
                "pi.DEM",
                signature.Planning.DemandPattern ==
                    DemandPatternKind.Unknown);

            CountUnknown(
                unknown,
                "pi.DEM.SRC",
                signature.Planning.DemandSource ==
                    DemandSourceKind.Unknown);

            CountUnknown(
                unknown,
                "alpha.I",
                signature.System.Items ==
                    CardinalityKind.Unknown);

            CountUnknown(
                unknown,
                "alpha.LV",
                signature.System.Levels ==
                    CardinalityKind.Unknown);

            CountUnknown(
                unknown,
                "alpha.NET",
                signature.System.Network ==
                    NetworkStructureKind.Unknown);

            CountUnknown(
                unknown,
                "alpha.ROUT",
                signature.System.Routing ==
                    RoutingStructureKind.Unknown);

            CountUnknown(
                unknown,
                "alpha.RES",
                signature.System.ResourceEnvironment ==
                    ResourceEnvironmentKind.Unknown);

            CountUnknown(
                unknown,
                "gamma",
                signature.Objective.State ==
                    FeatureState.Unknown);
        }

        return new LsiSignatureCoverageReport
        {
            SignatureCount = items.Length,

            CompletePlanningCount =
                items.Count(signature =>
                    signature.Planning.Horizon !=
                        PlanningHorizonKind.Unknown &&
                    signature.Planning.TimeModel !=
                        TimeModelKind.Unknown &&
                    signature.Planning.Information !=
                        InformationStructureKind.Unknown &&
                    signature.Planning.DemandPattern !=
                        DemandPatternKind.Unknown),

            KnownObjectiveCount =
                items.Count(signature =>
                    signature.Objective.State !=
                        FeatureState.Unknown),

            LegacyProjectedCount =
                items.Count(signature =>
                    LegacyProblemFamilyProjector
                        .Project(signature)
                        .HasProjection),

            UnknownFieldCounts = unknown
        };
    }

    private static void CountUnknown(
        IDictionary<string, int> counts,
        string code,
        bool unknown)
    {
        if (!unknown)
        {
            return;
        }

        if (!counts.TryGetValue(code, out int current))
        {
            current = 0;
        }

        counts[code] = current + 1;
    }
}
