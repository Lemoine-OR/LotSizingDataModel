using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Descriptors.Network;

namespace LotSizingDataModel.Instance.Notation;

/// <summary>
/// Generates notation scheme version 1 from typed problem descriptors.
/// </summary>
public sealed class UniversalNotationGenerator
{
    public UniversalLotSizingNotation Generate(
        LotSizingProblemDescriptor descriptor,
        UniversalObjectiveKind objective =
            UniversalObjectiveKind.Economic)
    {
        return Generate(
            descriptor,
            UniversalDerivedSemantics.Empty,
            objective);
    }

    /// <summary>
    /// Backward-compatible temporal-only enrichment overload.
    /// </summary>
    public UniversalLotSizingNotation Generate(
        LotSizingProblemDescriptor descriptor,
        IEnumerable<UniversalTemporalQualifier> temporalQualifiers,
        UniversalObjectiveKind objective =
            UniversalObjectiveKind.Economic)
    {
        ArgumentNullException.ThrowIfNull(temporalQualifiers);

        return Generate(
            descriptor,
            new UniversalDerivedSemantics(
                temporalQualifiers:
                    temporalQualifiers),
            objective);
    }

    /// <summary>
    /// Generates notation enriched with explicit derived analyses.
    /// </summary>
    public UniversalLotSizingNotation Generate(
        LotSizingProblemDescriptor descriptor,
        UniversalDerivedSemantics derivedSemantics,
        UniversalObjectiveKind objective =
            UniversalObjectiveKind.Economic)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(derivedSemantics);

        UniversalItemCardinality itemCardinality =
            descriptor.Structure.ItemCount switch
            {
                1 => UniversalItemCardinality.Single,
                > 1 => UniversalItemCardinality.Multiple,
                _ => UniversalItemCardinality.Unknown
            };

        UniversalProblemLevel problemLevel =
            descriptor.Structure.ProductStructureRelationshipCount switch
            {
                0 => UniversalProblemLevel.SingleLevel,
                > 0 => UniversalProblemLevel.MultiLevel,
                _ => UniversalProblemLevel.Unknown
            };

        var network =
            new UniversalNetworkNotation
            {
                Coupling = descriptor.SupplyNetwork.Coupling,
                ForwardTopology =
                    descriptor.SupplyNetwork.ForwardNetwork.Topology,
                ReverseTopology =
                    descriptor.SupplyNetwork.ReverseNetwork?.Topology,
                EchelonCount =
                    descriptor.SupplyNetwork.ForwardNetwork.EchelonCount,
                HasCycles =
                    descriptor.SupplyNetwork.ForwardNetwork.HasCycles,
                HasMultiSourcing =
                    descriptor.SupplyNetwork.HasMultiSourcing,
                HasTransshipment =
                    descriptor.SupplyNetwork.HasTransshipment
            };

        return new UniversalLotSizingNotation(
            alpha:
                new UniversalNotationAlpha
                {
                    ItemCardinality = itemCardinality,
                    ProblemLevel = problemLevel,
                    ProductStructureType =
                        descriptor.Structure.ProductStructureType,
                    Network = network
                },
            beta:
                new UniversalNotationBeta(
                    ExtractFeatures(descriptor),
                    derivedSemantics.TemporalQualifiers,
                    derivedSemantics.SatisfiedConditions),
            gamma:
                new UniversalNotationGamma
                {
                    Objective =
                        ResolveObjective(
                            descriptor.ObjectiveFinance,
                            objective)
                });
    }

    private static UniversalObjectiveKind ResolveObjective(
        ObjectiveFinanceDescriptor descriptor,
        UniversalObjectiveKind fallback)
    {
        if (descriptor.HasMultipleObjectives)
        {
            return UniversalObjectiveKind.MultipleObjectives;
        }

        return descriptor.PrimaryObjectiveKind switch
        {
            OptimizationObjectiveKind.Economic =>
                UniversalObjectiveKind.Economic,
            OptimizationObjectiveKind.Financial =>
                UniversalObjectiveKind.Financial,
            OptimizationObjectiveKind.Sustainability =>
                UniversalObjectiveKind.Sustainability,
            OptimizationObjectiveKind.ServiceLevel =>
                UniversalObjectiveKind.ServiceLevel,
            _ => fallback
        };
    }

    private static IEnumerable<UniversalNotationFeature>
        ExtractFeatures(
            LotSizingProblemDescriptor descriptor)
    {
        if (descriptor.Demand.HasDemand)
        {
            yield return UniversalNotationFeature.Demand;
        }

        if (descriptor.Demand.IsDeterministic)
        {
            yield return UniversalNotationFeature.DeterministicDemand;
        }

        if (descriptor.Demand.IsTimeVarying)
        {
            yield return UniversalNotationFeature.TimeVaryingDemand;
        }

        if (descriptor.Production.HasProduction)
        {
            yield return UniversalNotationFeature.Production;
        }

        if (
            descriptor.ProductionCapacityRegime ==
            ProductionCapacityRegime.Uncapacitated)
        {
            yield return
                UniversalNotationFeature.UncapacitatedProduction;
        }

        if (descriptor.Capacity.HasProductionCapacity)
        {
            yield return UniversalNotationFeature.ProductionCapacity;
        }

        if (descriptor.Capacity.HasSharedProductionCapacity)
        {
            yield return UniversalNotationFeature.SharedProductionCapacity;
        }

        if (descriptor.Capacity.HasTimeVaryingProductionCapacity)
        {
            yield return UniversalNotationFeature.TimeVaryingProductionCapacity;
        }

        if (descriptor.Capacity.HasSupplierCapacity)
        {
            yield return UniversalNotationFeature.SupplierCapacity;
        }

        if (descriptor.Capacity.HasTransportCapacity)
        {
            yield return UniversalNotationFeature.TransportCapacity;
        }

        if (descriptor.Capacity.HasWarehouseCapacity)
        {
            yield return UniversalNotationFeature.WarehouseCapacity;
        }

        if (descriptor.Setup.HasSetupCosts)
        {
            yield return UniversalNotationFeature.SetupCost;
        }

        if (descriptor.Setup.HasSetupTimes)
        {
            yield return UniversalNotationFeature.SetupTime;
        }

        if (descriptor.Setup.HasStartUpCosts)
        {
            yield return UniversalNotationFeature.StartUpCost;
        }

        if (descriptor.Setup.HasStartUpTimes)
        {
            yield return UniversalNotationFeature.StartUpTime;
        }

        if (descriptor.Production.HasLeadTimes)
        {
            yield return UniversalNotationFeature.ProductionLeadTime;
        }

        if (descriptor.Production.HasMinimumLotSizes)
        {
            yield return UniversalNotationFeature.MinimumLotSize;
        }

        if (descriptor.Production.HasMaximumLotSizes)
        {
            yield return UniversalNotationFeature.MaximumLotSize;
        }

        if (descriptor.Production.HasLotSizeMultiples)
        {
            yield return UniversalNotationFeature.LotSizeMultiple;
        }

        if (descriptor.Capacity.HasAdditionalProductionCapacity)
        {
            yield return UniversalNotationFeature.AdditionalProductionCapacity;
        }

        if (descriptor.Capacity.HasAdditionalWarehouseCapacity)
        {
            yield return UniversalNotationFeature.AdditionalWarehouseCapacity;
        }

        if (descriptor.Capacity.HasAdditionalTransportCapacity)
        {
            yield return UniversalNotationFeature.AdditionalTransportCapacity;
        }

        if (descriptor.InventoryService.HasInitialInventory)
        {
            yield return UniversalNotationFeature.InitialInventory;
        }

        if (descriptor.InventoryService.HasSafetyStockRequirements)
        {
            yield return UniversalNotationFeature.SafetyStock;
        }

        if (descriptor.InventoryService.HasBacklogging)
        {
            yield return UniversalNotationFeature.Backlogging;
        }

        if (descriptor.InventoryService.HasLostSales)
        {
            yield return UniversalNotationFeature.LostSales;
        }

        if (descriptor.Procurement.HasPurchasing)
        {
            yield return UniversalNotationFeature.Purchasing;
        }

        if (descriptor.Procurement.HasSupplierLeadTimes)
        {
            yield return UniversalNotationFeature.SupplierLeadTime;
        }

        if (descriptor.TransportationDistribution.HasTransportation)
        {
            yield return UniversalNotationFeature.Transportation;
        }

        if (descriptor.TransportationDistribution.HasTransportLeadTimes)
        {
            yield return UniversalNotationFeature.TransportLeadTime;
        }

        if (descriptor.TransportationDistribution.HasDistribution)
        {
            yield return UniversalNotationFeature.Distribution;
        }


        if (descriptor.Scheduling.HasIntegratedScheduling)
        {
            yield return UniversalNotationFeature.IntegratedScheduling;
        }

        if (descriptor.Scheduling.HasBigBucketStructure)
        {
            yield return UniversalNotationFeature.BigBucketScheduling;
        }

        if (descriptor.Scheduling.HasSmallBucketStructure)
        {
            yield return UniversalNotationFeature.SmallBucketScheduling;
        }

        if (descriptor.Scheduling.HasMicroPeriodStructure)
        {
            yield return UniversalNotationFeature.MacroMicroScheduling;
        }

        if (descriptor.Scheduling.HasExplicitMicroPeriodGrid)
        {
            yield return UniversalNotationFeature.ExplicitMicroPeriodGrid;
        }

        if (descriptor.Scheduling.HasVariableLengthMicroPeriods)
        {
            yield return UniversalNotationFeature.VariableLengthMicroPeriods;
        }

        if (descriptor.Scheduling.HasFixedLengthMicroPeriods)
        {
            yield return UniversalNotationFeature.FixedLengthMicroPeriods;
        }

        if (descriptor.Scheduling.HasSingleItemPerMicroPeriod)
        {
            yield return UniversalNotationFeature.SingleItemPerMicroPeriod;
        }

        if (descriptor.Scheduling.HasMultipleItemsPerMicroPeriod)
        {
            yield return UniversalNotationFeature.MultipleItemsPerMicroPeriod;
        }

        if (descriptor.Scheduling.HasVariableMicroPeriodCount)
        {
            yield return UniversalNotationFeature.VariableMicroPeriodCount;
        }

        if (descriptor.Scheduling.HasInitialSetupState)
        {
            yield return UniversalNotationFeature.InitialSetupState;
        }

        if (descriptor.Scheduling.HasSetupCarryOver)
        {
            yield return UniversalNotationFeature.SetupCarryOver;
        }

        if (descriptor.Scheduling.HasSequenceDependentChangeoverTimes)
        {
            yield return
                UniversalNotationFeature.SequenceDependentChangeoverTime;
        }

        if (descriptor.Scheduling.HasSequenceDependentChangeoverCosts)
        {
            yield return
                UniversalNotationFeature.SequenceDependentChangeoverCost;
        }

        if (descriptor.Scheduling.HasMaximumSetupCountConstraints)
        {
            yield return UniversalNotationFeature.MaximumSetupCount;
        }

        if (descriptor.Scheduling.HasSingleSchedulingResource)
        {
            yield return UniversalNotationFeature.SingleSchedulingResource;
        }

        if (descriptor.Scheduling.HasAllOrNothingSmallBucketProduction)
        {
            yield return
                UniversalNotationFeature.SmallBucketAllOrNothingProduction;
        }

        if (descriptor.Scheduling.HasContinuousSmallBucketProduction)
        {
            yield return
                UniversalNotationFeature.SmallBucketContinuousProduction;
        }

        if (descriptor.Scheduling.HasAtMostOneProducedItemPerBucket)
        {
            yield return
                UniversalNotationFeature.AtMostOneProducedItemPerBucket;
        }
        else if (descriptor.Scheduling.HasAtMostTwoProducedItemsPerBucket)
        {
            yield return
                UniversalNotationFeature.AtMostTwoProducedItemsPerBucket;
        }

        if (descriptor.Scheduling.HasAtMostOneSetupTransitionPerBucket)
        {
            yield return
                UniversalNotationFeature.AtMostOneSetupTransitionPerBucket;
        }

        if (descriptor.ObjectiveFinance.HasFinancialConstraints)
        {
            yield return UniversalNotationFeature.FinancialConstraint;
        }
    }
}
