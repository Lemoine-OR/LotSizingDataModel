using LotSizingDataModel.Instance.Descriptors.Temporal;
using LotSizingDataModel.Instance.Historical.BitranYanasse;
using LotSizingDataModel.Instance.Notation;
using LotSizingDataModel.Instance.Notation.Matching;

namespace LotSizingDataModel.Instance.Historical.Wolsey;

/// <summary>
/// Projects Wolsey's 2002 classification onto the generic universal notation
/// without changing the meaning of unsupported historical dimensions.
/// </summary>
public sealed class WolseyHistoricalMapper
{
    public WolseyHistoricalMapping Map(
        WolseySingleItemClassification classification)
    {
        ArgumentNullException.ThrowIfNull(classification);

        return Map(
            new WolseyExtendedClassification(
                classification));
    }

    public WolseyHistoricalMapping Map(
        WolseyExtendedClassification classification)
    {
        ArgumentNullException.ThrowIfNull(classification);

        var features =
            new List<UniversalNotationFeature>
            {
                UniversalNotationFeature.Demand,
                UniversalNotationFeature.DeterministicDemand,
                UniversalNotationFeature.Production,
                UniversalNotationFeature.SetupCost
            };

        var temporalQualifiers =
            new List<UniversalTemporalQualifier>();

        var semanticConditions =
            new List<UniversalSemanticCondition>();

        var missing =
            new List<string>();

        MapProblemVersion(
            classification.SingleItem.Problem,
            semanticConditions,
            missing);

        MapCapacity(
            classification.SingleItem.Capacity,
            features,
            temporalQualifiers,
            missing);

        MapVariants(
            classification.SingleItem.Variants,
            features,
            temporalQualifiers,
            missing);

        MapExtendedDimensions(
            classification,
            features,
            missing);

        UniversalItemCardinality itemCardinality =
            classification.ItemCount switch
            {
                1 => UniversalItemCardinality.Single,
                > 1 => UniversalItemCardinality.Multiple,
                _ => UniversalItemCardinality.Single
            };

        UniversalProblemLevel level =
            classification.MultiLevel is null
                ? UniversalProblemLevel.SingleLevel
                : UniversalProblemLevel.MultiLevel;

        LotSizingDataModel.Instance.Common.ProductStructureType
            structure =
                classification.MultiLevel?.Structure switch
                {
                    WolseyMultiLevelStructure.G =>
                        LotSizingDataModel.Instance.Common
                            .ProductStructureType.General,
                    WolseyMultiLevelStructure.A =>
                        LotSizingDataModel.Instance.Common
                            .ProductStructureType.Assembly,
                    WolseyMultiLevelStructure.S =>
                        LotSizingDataModel.Instance.Common
                            .ProductStructureType.Serial,
                    _ =>
                        LotSizingDataModel.Instance.Common
                            .ProductStructureType.IndependentItems
                };

        var notation =
            new UniversalLotSizingNotation(
                alpha:
                    new UniversalNotationAlpha
                    {
                        ItemCardinality = itemCardinality,
                        ProblemLevel = level,
                        ProductStructureType = structure,
                        Network =
                            new UniversalNetworkNotation
                            {
                                ForwardTopology =
                                    LotSizingDataModel.Instance
                                        .Descriptors.Network
                                        .SupplyNetworkTopologyType.Unknown
                            }
                    },
                beta:
                    new UniversalNotationBeta(
                        features,
                        temporalQualifiers,
                        semanticConditions),
                gamma:
                    new UniversalNotationGamma
                    {
                        Objective =
                            UniversalObjectiveKind.Economic
                    });

        HistoricalMappingCoverage coverage =
            missing.Count == 0
                ? HistoricalMappingCoverage.Exact
                : HistoricalMappingCoverage.Partial;

        return new WolseyHistoricalMapping(
            classification,
            new UniversalProblemSpecification(
                notation),
            coverage,
            missing);
    }

    private static void MapProblemVersion(
        WolseyProblemVersion problem,
        ICollection<UniversalSemanticCondition> semanticConditions,
        ICollection<string> missing)
    {
        switch (problem)
        {
            case WolseyProblemVersion.LS:
                // General LS domain is represented by the common features.
                break;

            case WolseyProblemVersion.WW:
                semanticConditions.Add(
                    UniversalSemanticCondition
                        .NonSpeculativeProductionHoldingCosts);
                break;

            case WolseyProblemVersion.DLSI:
                semanticConditions.Add(
                    UniversalSemanticCondition
                        .ZeroOrFullCapacityProduction);

                missing.Add(
                    "PROB.DLSI:VariableInitialStockDecision");
                break;

            case WolseyProblemVersion.DLS:
                semanticConditions.Add(
                    UniversalSemanticCondition
                        .ZeroOrFullCapacityProduction);

                missing.Add(
                    "PROB.DLS:NoVariableInitialStockDecision");
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(problem),
                    problem,
                    "Unknown Wolsey PROB field.");
        }
    }

    private static void MapCapacity(
        WolseyCapacityRegime capacity,
        ICollection<UniversalNotationFeature> features,
        ICollection<UniversalTemporalQualifier> temporalQualifiers,
        ICollection<string> missing)
    {
        switch (capacity)
        {
            case WolseyCapacityRegime.C:
                features.Add(
                    UniversalNotationFeature.ProductionCapacity);
                features.Add(
                    UniversalNotationFeature
                        .TimeVaryingProductionCapacity);
                break;

            case WolseyCapacityRegime.CC:
                features.Add(
                    UniversalNotationFeature.ProductionCapacity);
                temporalQualifiers.Add(
                    new UniversalTemporalQualifier(
                        UniversalTemporalParameter
                            .ProductionCapacity,
                        TemporalPatternType.Constant));
                break;

            case WolseyCapacityRegime.U:
                features.Add(
                    UniversalNotationFeature.UncapacitatedProduction);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(capacity),
                    capacity,
                    "Unknown Wolsey CAP field.");
        }
    }

    private static void MapVariants(
        IEnumerable<WolseyVariant> variants,
        ICollection<UniversalNotationFeature> features,
        ICollection<UniversalTemporalQualifier> temporalQualifiers,
        ICollection<string> missing)
    {
        foreach (WolseyVariant variant in variants)
        {
            switch (variant)
            {
                case WolseyVariant.B:
                    features.Add(
                        UniversalNotationFeature.Backlogging);
                    break;

                case WolseyVariant.SC:
                    // Wolsey SC = start-up cost.
                    features.Add(
                        UniversalNotationFeature.StartUpCost);
                    break;

                case WolseyVariant.ST:
                    features.Add(
                        UniversalNotationFeature.StartUpTime);
                    break;

                case WolseyVariant.STConstant:
                    features.Add(
                        UniversalNotationFeature.StartUpTime);
                    temporalQualifiers.Add(
                        new UniversalTemporalQualifier(
                            UniversalTemporalParameter.StartUpTime,
                            TemporalPatternType.Constant));
                    break;

                case WolseyVariant.SL:
                    missing.Add(
                        "VAR.SL:AdditionalSales");
                    break;

                case WolseyVariant.LB:
                    features.Add(
                        UniversalNotationFeature.MinimumLotSize);
                    break;

                case WolseyVariant.LBConstant:
                    features.Add(
                        UniversalNotationFeature.MinimumLotSize);
                    temporalQualifiers.Add(
                        new UniversalTemporalQualifier(
                            UniversalTemporalParameter.MinimumLotSize,
                            TemporalPatternType.Constant));
                    break;

                case WolseyVariant.SS:
                    features.Add(
                        UniversalNotationFeature.SafetyStock);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(variants),
                        variant,
                        "Unknown Wolsey VAR entry.");
            }
        }
    }

    private static void MapExtendedDimensions(
        WolseyExtendedClassification classification,
        ICollection<UniversalNotationFeature> features,
        ICollection<string> missing)
    {
        if (classification.Machines is not null)
        {
            WolseyMachineClassification machines =
                classification.Machines;

            missing.Add(
                $"Machines.NK={machines.MachineCount}");

            missing.Add(
                $"Machines.Mode={machines.MachineMode}");

            missing.Add(
                $"Machines.Bucket={machines.BucketType}");

            if (machines.HasLeadTimes)
            {
                features.Add(
                    UniversalNotationFeature.ProductionLeadTime);
            }

            foreach (
                WolseyMachineFeature feature
                in machines.Features)
            {
                switch (feature)
                {
                    case WolseyMachineFeature.SET:
                        features.Add(
                            UniversalNotationFeature.SetupTime);
                        break;

                    case WolseyMachineFeature.ST:
                        features.Add(
                            UniversalNotationFeature.StartUpTime);
                        break;

                    case WolseyMachineFeature.SQT:
                        missing.Add(
                            "Machines.SQT:SequenceDependentChangeoverTime");
                        break;

                    case WolseyMachineFeature.SQC:
                        missing.Add(
                            "Machines.SQC:SequenceDependentChangeoverCost");
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(classification),
                            feature,
                            "Unknown Wolsey machine feature.");
                }
            }
        }

        if (classification.ItemCount.HasValue)
        {
            // NI is represented by alpha item cardinality, but the exact
            // numerical count is not encoded by universal notation v1.
            if (classification.ItemCount.Value > 1)
            {
                missing.Add(
                    $"NI.ExactCount={classification.ItemCount.Value}");
            }
        }

        if (classification.PeriodCount.HasValue)
        {
            missing.Add(
                $"NT.ExactCount={classification.PeriodCount.Value}");
        }

        if (classification.MultiLevel is not null)
        {
            // G/A/S topology is represented. Exact NL count is not.
            missing.Add(
                $"NL.ExactCount={classification.MultiLevel.LevelCount}");
        }
    }
}
