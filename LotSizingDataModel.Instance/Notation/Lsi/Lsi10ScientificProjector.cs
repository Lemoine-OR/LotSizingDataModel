using System.Globalization;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Descriptors.Network;
using LotSizingDataModel.Instance.Descriptors.Temporal;

namespace LotSizingDataModel.Instance.Notation.Lsi;

/// <summary>
/// Projects the stable 1.2.x descriptor / Universal Notation model to LSI/1.0.
/// </summary>
public sealed class Lsi10ScientificProjector
{
    public Lsi10Projection Project(
        LotSizingProblemDescriptor descriptor,
        UniversalLotSizingNotation universalNotation)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(universalNotation);

        string planning =
            RenderPlanning(descriptor);

        string alpha =
            RenderAlpha(descriptor);

        string beta =
            RenderBeta(universalNotation);

        string gamma =
            RenderGamma(universalNotation);

        IReadOnlyDictionary<string, string> dimensions =
            BuildDimensions(descriptor);

        string sigma =
            string.Join(
                ",",
                dimensions.Select(
                    pair => pair.Key + "=" + pair.Value));

        string canonical =
            "LSI/1.0: pi{" +
            planning +
            "} | alpha{" +
            alpha +
            "} | beta{" +
            beta +
            "} | gamma{" +
            gamma +
            "} @ sigma{" +
            sigma +
            "}";

        return new Lsi10Projection(
            canonical,
            universalNotation.Render(),
            ProjectLegacyProblemFamily(descriptor),
            dimensions);
    }

    private static string RenderPlanning(
        LotSizingProblemDescriptor descriptor)
    {
        string demand =
            !descriptor.Demand.HasDemand
                ? "NA"
                : descriptor.Demand.IsTimeVarying
                    ? "DYN"
                    : "STA";

        string information =
            !descriptor.Demand.HasDemand
                ? "NA"
                : descriptor.Demand.IsDeterministic
                    ? "DET"
                    : "?";

        string bucket =
            descriptor.Scheduling.BucketMode switch
            {
                Core.DecisionModel.Scheduling
                    .SchedulingBucketMode.BigBucket => "BB",
                Core.DecisionModel.Scheduling
                    .SchedulingBucketMode.SmallBucket => "SB",
                Core.DecisionModel.Scheduling
                    .SchedulingBucketMode.MacroMicro => "MM",
                _ => "NA"
            };

        return string.Join(
            ";",
            new[]
            {
                "H=F",
                "TM=DT",
                "BK=" + bucket,
                "INF=" + information,
                "DEM=" + demand,
                "DEM.SRC=?"
            });
    }

    private static string RenderAlpha(
        LotSizingProblemDescriptor descriptor)
    {
        string itemCardinality =
            descriptor.Structure.ItemCount switch
            {
                1 => "1",
                > 1 => "M",
                _ => "?"
            };

        string level =
            descriptor.Structure
                .ProductStructureRelationshipCount > 0
                ? "ML"
                : "SL";

        string productStructure =
            descriptor.Structure.ProductStructureType switch
            {
                ProductStructureType.IndependentItems => "IND",
                ProductStructureType.Serial => "SER",
                ProductStructureType.Assembly => "ASM",
                ProductStructureType.Arborescent => "ARB",
                ProductStructureType.General => "GEN",
                _ => "?"
            };

        string network =
            descriptor.SupplyNetwork.ForwardNetwork.Topology switch
            {
                SupplyNetworkTopologyType.Independent => "IND",
                SupplyNetworkTopologyType.Serial => "SER",
                SupplyNetworkTopologyType.Convergent => "CONV",
                SupplyNetworkTopologyType.Divergent => "DIV",
                SupplyNetworkTopologyType.Tree => "TREE",
                SupplyNetworkTopologyType.General => "GEN",
                _ => "?"
            };

        return string.Join(
            ";",
            new[]
            {
                "I=" + itemCardinality,
                "L=" + level,
                "PS=" + productStructure,
                "NET=" + network,
                "SITE=" +
                    (descriptor.Structure.IsMultiSite ? "MULTI" : "SINGLE"),
                "WC=" +
                    descriptor.Structure.WorkCenterCount
                        .ToString(CultureInfo.InvariantCulture)
            });
    }

    private static string RenderBeta(
        UniversalLotSizingNotation notation)
    {
        var tokens = new List<string>();

        foreach (UniversalNotationFeature feature
                 in notation.Beta.Features.OrderBy(
                     feature => (int)feature))
        {
            string? token =
                MapFeature(feature);

            if (token is not null)
            {
                tokens.Add(token + "=1");
            }
        }

        foreach (UniversalTemporalQualifier qualifier
                 in notation.Beta.TemporalQualifiers.OrderBy(
                     qualifier => (int)qualifier.Parameter))
        {
            string? parameter =
                MapTemporalParameter(
                    qualifier.Parameter);

            if (parameter is null)
            {
                continue;
            }

            tokens.Add(
                "rho(" +
                parameter +
                ")=" +
                MapTemporalPattern(
                    qualifier.Pattern));
        }

        return tokens.Count == 0
            ? "NONE"
            : string.Join(";", tokens);
    }

    private static string? MapFeature(
        UniversalNotationFeature feature)
    {
        return feature switch
        {
            UniversalNotationFeature.Demand => "DEM",
            UniversalNotationFeature.Production => "PROD",
            UniversalNotationFeature.ProductionCapacity => "CAP.P",
            UniversalNotationFeature.SharedProductionCapacity => "CAP.P.SH",
            UniversalNotationFeature.SupplierCapacity => "CAP.S",
            UniversalNotationFeature.TransportCapacity => "CAP.T",
            UniversalNotationFeature.WarehouseCapacity => "CAP.W",
            UniversalNotationFeature.SetupCost => "SET.C",
            UniversalNotationFeature.SetupTime => "SET.T",
            UniversalNotationFeature.StartUpCost => "SET.SU.C",
            UniversalNotationFeature.StartUpTime => "SET.SU.T",
            UniversalNotationFeature.MinimumLotSize => "LOT.MIN",
            UniversalNotationFeature.MaximumLotSize => "LOT.MAX",
            UniversalNotationFeature.LotSizeMultiple => "LOT.MUL",
            UniversalNotationFeature.ProductionLeadTime => "LT.P",
            UniversalNotationFeature.InitialInventory => "INV.I0",
            UniversalNotationFeature.SafetyStock => "INV.SS",
            UniversalNotationFeature.Backlogging => "SHORT.BO",
            UniversalNotationFeature.LostSales => "SHORT.LS",
            UniversalNotationFeature.Purchasing => "SRC.BUY",
            UniversalNotationFeature.SupplierLeadTime => "LT.S",
            UniversalNotationFeature.Transportation => "TRANS",
            UniversalNotationFeature.TransportLeadTime => "LT.T",
            UniversalNotationFeature.Distribution => "DIST",
            UniversalNotationFeature.FinancialConstraint => "FIN",
            UniversalNotationFeature.IntegratedScheduling => "SCH",
            UniversalNotationFeature.BigBucketScheduling => "SCH.BB",
            UniversalNotationFeature.SmallBucketScheduling => "SCH.SB",
            UniversalNotationFeature.MacroMicroScheduling => "SCH.MM",
            UniversalNotationFeature.InitialSetupState => "SET.INIT",
            UniversalNotationFeature.SetupCarryOver => "SET.CO",
            UniversalNotationFeature.SequenceDependentChangeoverTime =>
                "SET.SD.T",
            UniversalNotationFeature.SequenceDependentChangeoverCost =>
                "SET.SD.C",
            UniversalNotationFeature.MaximumSetupCount => "SCH.MAXSET",
            UniversalNotationFeature.GroupingConstraint => "LOT.GRP",
            UniversalNotationFeature.ProductionSetupFamily => "SET.FAM",
            UniversalNotationFeature.ProductionSetupFamilyTime => "SET.FAM.T",
            UniversalNotationFeature.AdditionalProductionCapacity =>
                "CAP.ADD.P",
            UniversalNotationFeature.AdditionalWarehouseCapacity =>
                "CAP.ADD.W",
            UniversalNotationFeature.AdditionalTransportCapacity =>
                "CAP.ADD.T",
            _ => null
        };
    }

    private static string? MapTemporalParameter(
        UniversalTemporalParameter parameter)
    {
        return parameter switch
        {
            UniversalTemporalParameter.Demand => "DEM",
            UniversalTemporalParameter.SetupCost => "SET.C",
            UniversalTemporalParameter.HoldingCost => "HOLD.C",
            UniversalTemporalParameter.ProductionCost => "PROD.C",
            UniversalTemporalParameter.ProductionCapacity => "CAP.P",
            UniversalTemporalParameter.MinimumLotSize => "LOT.MIN",
            UniversalTemporalParameter.StartUpTime => "SET.SU.T",
            UniversalTemporalParameter.MaximumLotSize => "LOT.MAX",
            UniversalTemporalParameter.SupplierCapacity => "CAP.S",
            _ => null
        };
    }

    private static string MapTemporalPattern(
        TemporalPatternType pattern)
    {
        return pattern switch
        {
            TemporalPatternType.Zero => "Z",
            TemporalPatternType.Constant => "C",
            TemporalPatternType.NonIncreasing => "NI",
            TemporalPatternType.NonDecreasing => "ND",
            TemporalPatternType.General => "G",
            _ => "?"
        };
    }

    private static string RenderGamma(
        UniversalLotSizingNotation notation)
    {
        string objective =
            notation.Gamma.Objective switch
            {
                UniversalObjectiveKind.Economic => "ECON",
                UniversalObjectiveKind.MultipleObjectives => "MULTI",
                UniversalObjectiveKind.Financial => "FIN",
                UniversalObjectiveKind.Sustainability => "SUST",
                UniversalObjectiveKind.ServiceLevel => "SERVICE",
                _ => "?"
            };

        return "S=MIN;AGG=" +
               (notation.Gamma.Objective ==
                UniversalObjectiveKind.MultipleObjectives
                   ? "MULTI"
                   : "SINGLE") +
               ";OBJ=" +
               objective;
    }

    private static IReadOnlyDictionary<string, string>
        BuildDimensions(
            LotSizingProblemDescriptor descriptor)
    {
        return new Dictionary<string, string>(
            StringComparer.Ordinal)
        {
            ["T"] =
                descriptor.Time.PlanningHorizon
                    .ToString(CultureInfo.InvariantCulture),
            ["I"] =
                descriptor.Structure.ItemCount
                    .ToString(CultureInfo.InvariantCulture),
            ["P"] =
                descriptor.Structure.PlantCount
                    .ToString(CultureInfo.InvariantCulture),
            ["WC"] =
                descriptor.Structure.WorkCenterCount
                    .ToString(CultureInfo.InvariantCulture),
            ["WH"] =
                descriptor.Structure.WarehouseCount
                    .ToString(CultureInfo.InvariantCulture),
            ["SUP"] =
                descriptor.Structure.SupplierCount
                    .ToString(CultureInfo.InvariantCulture),
            ["DC"] =
                descriptor.Structure.DistributionCenterCount
                    .ToString(CultureInfo.InvariantCulture),
            ["TR"] =
                descriptor.Structure.TransportResourceCount
                    .ToString(CultureInfo.InvariantCulture),
            ["BOM"] =
                descriptor.Structure.ProductStructureRelationshipCount
                    .ToString(CultureInfo.InvariantCulture),
            ["DEPTH"] =
                descriptor.Structure.MaximumProductStructureDepth
                    .ToString(CultureInfo.InvariantCulture)
        };
    }

    private static string ProjectLegacyProblemFamily(
        LotSizingProblemDescriptor descriptor)
    {
        bool capacitated =
            descriptor.Capacity.HasProductionCapacity;

        bool multiLevel =
            descriptor.Structure
                .ProductStructureRelationshipCount > 0;

        if (descriptor.Structure.ItemCount == 1 &&
            !multiLevel)
        {
            return capacitated ? "LS-C" : "LS-U";
        }

        if (descriptor.Structure.ItemCount > 1 &&
            !multiLevel)
        {
            return capacitated ? "CLSP" : "LS-U";
        }

        if (descriptor.Structure.ItemCount > 1 &&
            multiLevel)
        {
            return capacitated ? "MLCLSP" : "MLLP";
        }

        return "?";
    }
}
