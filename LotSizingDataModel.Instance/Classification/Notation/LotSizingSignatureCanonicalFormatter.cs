using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LotSizingDataModel.Instance.Classification.Notation;

/// <summary>
/// Produces the stable, machine-oriented LSI notation.
/// </summary>
public static class LotSizingSignatureCanonicalFormatter
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
        AppendSize(builder, signature.Size);

        return builder.ToString();
    }

    private static void AppendPlanning(
        StringBuilder builder,
        PlanningSignature value)
    {
        builder.Append("pi{");
        builder.Append("H=");
        builder.Append(HorizonCode(value.Horizon));
        builder.Append(",TM=");
        builder.Append(TimeCode(value.TimeModel));
        builder.Append(",BK=");
        builder.Append(BucketCode(value.BucketStructure));
        builder.Append(",INF=");
        builder.Append(InformationCode(value.Information));
        builder.Append(",DEM=");
        builder.Append(DemandCode(value.DemandPattern));
        builder.Append(",DEM.SRC=");
        builder.Append(DemandSourceCode(value.DemandSource));
        builder.Append('}');
    }

    private static void AppendSystem(
        StringBuilder builder,
        SystemSignature value)
    {
        builder.Append("alpha{");
        builder.Append("I=");
        builder.Append(CardinalityCode(value.Items));
        builder.Append(",LV=");
        builder.Append(CardinalityCode(value.Levels));
        builder.Append(",PS=");
        builder.Append(value.ProductStructure.ToString());
        builder.Append(",NET=");
        builder.Append(value.Network.ToString());
        builder.Append(",ROUT=");
        builder.Append(value.Routing.ToString());
        builder.Append(",RES=");
        builder.Append(value.ResourceEnvironment.ToString());
        builder.Append('}');
    }

    private static void AppendFeatures(
        StringBuilder builder,
        FeatureSignature value)
    {
        builder.Append("beta{");

        bool first = true;

        foreach (FeatureEntry feature in
                 value.Features
                     .OrderBy(
                         entry => entry.Code,
                         StringComparer.Ordinal))
        {
            if (!first)
            {
                builder.Append(',');
            }

            first = false;
            builder.Append(feature.Code);
            builder.Append('=');
            builder.Append(StateCode(feature.State));

            if (feature.TemporalProfile is not null)
            {
                builder.Append('~');
                builder.Append(
                    TemporalCode(feature.TemporalProfile.Kind));

                if (feature.TemporalProfile.Kind ==
                        TemporalProfileKind.Mixed &&
                    feature.TemporalProfile.Components.Count > 0)
                {
                    builder.Append('{');
                    builder.Append(
                        string.Join(
                            ",",
                            feature.TemporalProfile.Components
                                .OrderBy(item => (int)item)
                                .Select(TemporalCode)));
                    builder.Append('}');
                }
            }
        }

        builder.Append('}');
    }

    private static void AppendObjective(
        StringBuilder builder,
        ObjectiveSignature value)
    {
        builder.Append("gamma{");

        if (value.State == FeatureState.Unknown)
        {
            builder.Append('?');
            builder.Append('}');
            return;
        }

        builder.Append(StateCode(value.State));
        builder.Append(",SENSE=");
        builder.Append(value.Sense.ToString());
        builder.Append(",AGG=");
        builder.Append(value.Aggregation.ToString());

        if (value.Components.Count > 0)
        {
            builder.Append(",OBJ={");
            builder.Append(
                string.Join(
                    ",",
                    value.Components
                        .OrderBy(item => (int)item)
                        .Select(item => item.ToString())));
            builder.Append('}');
        }

        builder.Append('}');
    }

    private static void AppendSize(
        StringBuilder builder,
        InstanceSizeSignature value)
    {
        builder.Append("sigma{");
        builder.AppendFormat(
            CultureInfo.InvariantCulture,
            "T={0},I={1},P={2},WC={3},WH={4},SUP={5},DC={6},TR={7},BOM={8},DEPTH={9}",
            value.Periods,
            value.Items,
            value.Plants,
            value.WorkCenters,
            value.Warehouses,
            value.Suppliers,
            value.DistributionCenters,
            value.TransportResources,
            value.BomRelationships,
            value.MaximumBomDepth);
        builder.Append('}');
    }

    internal static string StateCode(FeatureState value) =>
        value switch
        {
            FeatureState.Absent => "0",
            FeatureState.Present => "1",
            FeatureState.NotApplicable => "NA",
            FeatureState.Mixed => "MIX",
            _ => "?"
        };

    internal static string TemporalCode(
        TemporalProfileKind value) =>
        value switch
        {
            TemporalProfileKind.Zero => "Z",
            TemporalProfileKind.Constant => "C",
            TemporalProfileKind.NonIncreasing => "NI",
            TemporalProfileKind.NonDecreasing => "ND",
            TemporalProfileKind.General => "G",
            TemporalProfileKind.Periodic => "PER",
            TemporalProfileKind.Mixed => "MIX",
            TemporalProfileKind.NotApplicable => "NA",
            _ => "?"
        };

    private static string HorizonCode(
        PlanningHorizonKind value) =>
        value switch
        {
            PlanningHorizonKind.Finite => "F",
            PlanningHorizonKind.Infinite => "INF",
            PlanningHorizonKind.Rolling => "RH",
            _ => "?"
        };

    private static string TimeCode(TimeModelKind value) =>
        value switch
        {
            TimeModelKind.Discrete => "DT",
            TimeModelKind.Continuous => "CT",
            TimeModelKind.Hybrid => "HYB",
            _ => "?"
        };

    private static string BucketCode(BucketStructureKind value) =>
        value switch
        {
            BucketStructureKind.NotApplicable => "NA",
            BucketStructureKind.BigBucket => "BB",
            BucketStructureKind.SmallBucket => "SB",
            BucketStructureKind.Hybrid => "HYB",
            BucketStructureKind.MacroMicro => "MM",
            _ => "?"
        };

    private static string InformationCode(
        InformationStructureKind value) =>
        value switch
        {
            InformationStructureKind.Deterministic => "DET",
            InformationStructureKind.Stochastic => "STO",
            InformationStructureKind.Robust => "ROB",
            InformationStructureKind.Fuzzy => "FUZ",
            InformationStructureKind.Hybrid => "HYB",
            _ => "?"
        };

    private static string DemandCode(DemandPatternKind value) =>
        value switch
        {
            DemandPatternKind.Stationary => "STA",
            DemandPatternKind.Dynamic => "DYN",
            DemandPatternKind.Endogenous => "END",
            DemandPatternKind.Mixed => "MIX",
            _ => "?"
        };

    private static string DemandSourceCode(DemandSourceKind value) =>
        value switch
        {
            DemandSourceKind.Exogenous => "EXO",
            DemandSourceKind.Endogenous => "ENDO",
            DemandSourceKind.Mixed => "MIX",
            _ => "?"
        };

    private static string CardinalityCode(CardinalityKind value) =>
        value switch
        {
            CardinalityKind.None => "0",
            CardinalityKind.Single => "1",
            CardinalityKind.Multiple => "m",
            _ => "?"
        };
}
