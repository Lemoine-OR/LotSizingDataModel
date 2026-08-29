using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Descriptors.Temporal;
using LotSizingDataModel.Instance.Notation.Matching;

namespace LotSizingDataModel.Instance.Historical.BitranYanasse;

/// <summary>
/// Maps the historical Bitran-Yanasse temporal classification to the portion
/// of the LotSizingDataModel universal specification that can be represented
/// exactly by notation scheme v1.
/// </summary>
/// <remarks>
/// The 1982 classification concerns the capacitated single-item lot-size
/// problem and classifies the temporal behavior of setup cost, holding cost,
/// production cost and capacity.
///
/// Universal notation v1 does not yet encode those four parameterized temporal
/// patterns. Consequently this mapper preserves the full historical profile
/// separately and reports Partial coverage instead of silently dropping or
/// reinterpreting the four dimensions.
/// </remarks>
public sealed class BitranYanasseHistoricalMapper
{
    private static readonly string[]
        TemporalDimensionsNotYetRepresented =
        {
            "setupCostPattern",
            "holdingCostPattern",
            "productionCostPattern",
            "capacityPattern"
        };

    /// <summary>
    /// Gets the universal v1 domain specification shared by the classical
    /// Bitran-Yanasse problem family, excluding temporal-pattern qualifiers
    /// that v1 cannot yet encode.
    /// </summary>
    public static UniversalProblemSpecification
        ClassicalDomainSpecification { get; } =
            UniversalProblemSpecification.Parse(
                "1,SL,Net:UNK | Dem,Det,Prod,Cap:P | Obj:Econ");

    public BitranYanasseHistoricalMapping Map(
        BitranYanasseTemporalProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return new BitranYanasseHistoricalMapping(
            profile,
            ClassicalDomainSpecification,
            HistoricalMappingCoverage.Partial,
            TemporalDimensionsNotYetRepresented,
            applicability: null);
    }

    public BitranYanasseHistoricalMapping Map(
        BitranYanasseTemporalProfile profile,
        LotSizingProblemDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(descriptor);

        return new BitranYanasseHistoricalMapping(
            profile,
            ClassicalDomainSpecification,
            HistoricalMappingCoverage.Partial,
            TemporalDimensionsNotYetRepresented,
            AssessApplicability(descriptor));
    }

    public BitranYanasseApplicabilityAssessment
        AssessApplicability(
            LotSizingProblemDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        var failedRequirements =
            new List<string>();

        var extensions =
            new List<string>();

        if (descriptor.Structure.ItemCount <= 0 ||
            descriptor.Time.PlanningHorizon <= 0)
        {
            return new BitranYanasseApplicabilityAssessment(
                BitranYanasseApplicabilityKind.Incomplete,
                new[]
                {
                    "positiveItemCountAndPlanningHorizon"
                },
                Array.Empty<string>());
        }

        if (descriptor.Structure.ItemCount != 1)
        {
            failedRequirements.Add("singleItem");
        }

        if (descriptor.Structure.HasProductStructure)
        {
            failedRequirements.Add("singleLevel");
        }

        if (!descriptor.Demand.HasDemand)
        {
            failedRequirements.Add("demand");
        }

        if (!descriptor.Demand.IsDeterministic)
        {
            failedRequirements.Add("deterministicDemand");
        }

        if (!descriptor.Production.HasProduction)
        {
            failedRequirements.Add("production");
        }

        if (!descriptor.Capacity.HasProductionCapacity)
        {
            failedRequirements.Add("productionCapacity");
        }

        if (descriptor.ObjectiveFinance.HasMultipleObjectives)
        {
            failedRequirements.Add("singleEconomicObjective");
        }

        if (failedRequirements.Count > 0)
        {
            return new BitranYanasseApplicabilityAssessment(
                BitranYanasseApplicabilityKind.NotApplicable,
                failedRequirements,
                Array.Empty<string>());
        }

        AddExtension(
            descriptor.Setup.HasSetupTimes,
            "setupTimes",
            extensions);

        AddExtension(
            descriptor.Setup.HasStartUpCosts,
            "startupCosts",
            extensions);

        AddExtension(
            descriptor.Production.HasLeadTimes,
            "productionLeadTimes",
            extensions);

        AddExtension(
            descriptor.Production.HasLotSizeRestrictions,
            "lotSizeRestrictions",
            extensions);

        AddExtension(
            descriptor.Capacity.HasAdditionalCapacity,
            "additionalCapacity",
            extensions);

        AddExtension(
            descriptor.Capacity.HasSupplierCapacity,
            "supplierCapacity",
            extensions);

        AddExtension(
            descriptor.Capacity.HasTransportCapacity,
            "transportCapacity",
            extensions);

        AddExtension(
            descriptor.Capacity.HasWarehouseCapacity,
            "warehouseCapacity",
            extensions);

        AddExtension(
            descriptor.InventoryService.HasSafetyStockRequirements,
            "safetyStock",
            extensions);

        AddExtension(
            descriptor.InventoryService.HasBacklogging,
            "backlogging",
            extensions);

        AddExtension(
            descriptor.InventoryService.HasLostSales,
            "lostSales",
            extensions);

        AddExtension(
            descriptor.Procurement.HasPurchasing,
            "purchasing",
            extensions);

        AddExtension(
            descriptor.TransportationDistribution.HasTransportation,
            "transportation",
            extensions);

        AddExtension(
            descriptor.TransportationDistribution.HasDistribution,
            "distribution",
            extensions);

        AddExtension(
            descriptor.ObjectiveFinance.HasFinancialConstraints,
            "financialConstraints",
            extensions);

        BitranYanasseApplicabilityKind kind =
            extensions.Count == 0
                ? BitranYanasseApplicabilityKind.ExactHistoricalDomain
                : BitranYanasseApplicabilityKind.ExtendedButProjectable;

        return new BitranYanasseApplicabilityAssessment(
            kind,
            Array.Empty<string>(),
            extensions);
    }

    private static void AddExtension(
        bool condition,
        string code,
        ICollection<string> extensions)
    {
        if (condition)
        {
            extensions.Add(code);
        }
    }
}
