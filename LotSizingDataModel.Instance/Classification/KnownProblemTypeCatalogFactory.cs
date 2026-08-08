using System.Collections.Generic;
using LotSizingDataModel.Instance.Common;

namespace LotSizingDataModel.Instance.Classification;

/// <summary>
/// Creates predefined catalogs of known lot-sizing
/// problem-family definitions.
/// </summary>
/// <remarks>
/// The factory centralizes the operational meaning assigned
/// by this library to classical lot-sizing family codes.
///
/// Catalog users may extend the returned catalog with
/// additional rules and problem-family definitions.
/// </remarks>
public static class KnownProblemTypeCatalogFactory
{
    /// <summary>
    /// Gets the name of the standard lot-sizing problem-type
    /// catalog.
    /// </summary>
    public const string StandardCatalogName =
        "Standard lot-sizing problem taxonomy";

    /// <summary>
    /// Gets the current version of the standard catalog
    /// contents and classification semantics.
    /// </summary>
    public const string StandardCatalogVersion =
        "1.0";

    /// <summary>
    /// Gets the standard code used for the uncapacitated
    /// single-item lot-sizing family.
    /// </summary>
    public const string UncapacitatedSingleItemCode =
        "LS-U";

    /// <summary>
    /// Gets the standard code used for the capacitated
    /// single-item lot-sizing family.
    /// </summary>
    public const string CapacitatedSingleItemCode =
        "LS-C";

    /// <summary>
    /// Gets the standard code used for the single-level
    /// multi-item capacitated lot-sizing family.
    /// </summary>
    public const string CapacitatedLotSizingCode =
        "CLSP";

    /// <summary>
    /// Gets the standard code used for the multi-level
    /// uncapacitated lot-sizing family.
    /// </summary>
    public const string MultiLevelLotSizingCode =
        "MLLP";

    /// <summary>
    /// Gets the standard code used for the multi-level
    /// multi-item capacitated lot-sizing family.
    /// </summary>
    public const string MultiLevelCapacitatedLotSizingCode =
        "MLCLSP";

    /// <summary>
    /// Creates the standard catalog of known lot-sizing
    /// problem families.
    /// </summary>
    /// <returns>
    /// A new, independent and structurally valid catalog
    /// containing standard rules and initial problem-family
    /// definitions.
    /// </returns>
    /// <remarks>
    /// The returned catalog initially contains:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <c>LS-U</c>: single-item, single-level and
    /// uncapacitated;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>LS-C</c>: single-item, single-level and
    /// production-capacitated;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>CLSP</c>: multi-item, single-level and
    /// production-capacitated;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>MLLP</c>: multi-item, multi-level and
    /// uncapacitated;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>MLCLSP</c>: multi-item, multi-level and
    /// production-capacitated.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// Each call creates a new catalog. Modifying one returned
    /// catalog therefore does not affect catalogs returned by
    /// later calls.
    /// </remarks>
    public static KnownProblemTypeCatalog
        CreateStandardCatalog()
    {
        var catalog =
            new KnownProblemTypeCatalog(
                StandardCatalogName,
                StandardCatalogVersion)
            {
                Description =
                    "Operational catalog of classical " +
                    "lot-sizing problem families, their " +
                    "defining conditions and commonly " +
                    "recognized extensions."
            };

        AddCommonRules(catalog);
        AddStructuralRules(catalog);
        AddCapacityRules(catalog);
        AddExtensionRules(catalog);

        catalog.AddDefinition(
            CreateUncapacitatedSingleItemDefinition());

        catalog.AddDefinition(
            CreateCapacitatedSingleItemDefinition());

        catalog.AddDefinition(
            CreateCapacitatedLotSizingDefinition());

        catalog.AddDefinition(
            CreateMultiLevelLotSizingDefinition());

        catalog.AddDefinition(
            CreateMultiLevelCapacitatedDefinition());

        catalog.EnsureValid();

        return catalog;
    }

    private static void AddCommonRules(
        KnownProblemTypeCatalog catalog)
    {
        catalog.AddRule(
            CreateBooleanRule(
                ruleCode:
                    "COMMON.HAS_DEMAND",

                featureCode:
                    "hasDemand",

                expectedValue:
                    true,

                description:
                    "The instance contains demand data."));

        catalog.AddRule(
            CreateBooleanRule(
                ruleCode:
                    "COMMON.DETERMINISTIC_DEMAND",

                featureCode:
                    "hasDeterministicDemand",

                expectedValue:
                    true,

                description:
                    "Demand is represented deterministically."));

        catalog.AddRule(
            new KnownProblemRuleDefinition(
                ruleCode:
                    "COMMON.MULTI_PERIOD",

                featureCode:
                    "planningHorizon",

                operatorCode:
                    KnownProblemRuleDefinition
                        .GreaterThanOperator,

                expectedValue:
                    "1",

                description:
                    "The planning horizon contains more " +
                    "than one period."));

        catalog.AddRule(
            CreateBooleanRule(
                ruleCode:
                    "COMMON.HAS_PRODUCTION",

                featureCode:
                    "hasProduction",

                expectedValue:
                    true,

                description:
                    "The instance contains production " +
                    "decisions."));
    }

    private static void AddStructuralRules(
        KnownProblemTypeCatalog catalog)
    {
        catalog.AddRule(
            CreateBooleanRule(
                ruleCode:
                    "STRUCTURE.SINGLE_ITEM",

                featureCode:
                    "isSingleItem",

                expectedValue:
                    true,

                description:
                    "The instance contains exactly one item."));

        catalog.AddRule(
            CreateBooleanRule(
                ruleCode:
                    "STRUCTURE.MULTI_ITEM",

                featureCode:
                    "isMultiItem",

                expectedValue:
                    true,

                description:
                    "The instance contains several items."));

        catalog.AddRule(
            CreateBooleanRule(
                ruleCode:
                    "STRUCTURE.SINGLE_LEVEL",

                featureCode:
                    "isSingleLevel",

                expectedValue:
                    true,

                description:
                    "The items are independent from a " +
                    "bill-of-materials perspective."));

        catalog.AddRule(
            CreateBooleanRule(
                ruleCode:
                    "STRUCTURE.MULTI_LEVEL",

                featureCode:
                    "isMultiLevel",

                expectedValue:
                    true,

                description:
                    "The instance contains at least one " +
                    "bill-of-materials relationship."));
    }

    private static void AddCapacityRules(
        KnownProblemTypeCatalog catalog)
    {
        /*
         * Only production capacity is used here to distinguish
         * classical capacitated and uncapacitated production
         * lot-sizing families.
         *
         * Supplier, transport and warehouse capacities are
         * treated as supply-chain extensions.
         */
        catalog.AddRule(
            CreateBooleanRule(
                ruleCode:
                    "CAPACITY.UNCAPACITATED",

                featureCode:
                    "hasProductionCapacityConstraints",

                expectedValue:
                    false,

                description:
                    "Production is not subject to a capacity " +
                    "constraint."));

        catalog.AddRule(
            CreateBooleanRule(
                ruleCode:
                    "CAPACITY.PRODUCTION_CAPACITATED",

                featureCode:
                    "hasProductionCapacityConstraints",

                expectedValue:
                    true,

                description:
                    "Production is subject to at least one " +
                    "capacity constraint."));

        catalog.AddRule(
            CreateBooleanRule(
                ruleCode:
                    "CAPACITY.SHARED_PRODUCTION",

                featureCode:
                    "hasSharedProductionCapacity",

                expectedValue:
                    true,

                description:
                    "Several production activities share " +
                    "at least one production capacity."));

        catalog.AddRule(
            CreateBooleanRule(
                ruleCode:
                    "CAPACITY.TIME_VARYING_PRODUCTION",

                featureCode:
                    "hasTimeVaryingProductionCapacity",

                expectedValue:
                    true,

                description:
                    "At least one production capacity " +
                    "varies between planning periods."));
    }

    private static void AddExtensionRules(
        KnownProblemTypeCatalog catalog)
    {
        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.SAFETY_STOCK",

            featureCode:
                "hasSafetyStockRequirements",

            description:
                "The instance contains safety-stock " +
                "requirements.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.BACKLOGGING",

            featureCode:
                "hasBacklogging",

            description:
                "Unmet demand may be carried to later " +
                "periods.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.LOST_SALES",

            featureCode:
                "hasLostSales",

            description:
                "Demand may be permanently lost.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.PRODUCTION_LEAD_TIMES",

            featureCode:
                "hasProductionLeadTimes",

            description:
                "Production lead times are represented.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.MINIMUM_LOT_SIZES",

            featureCode:
                "hasMinimumLotSizes",

            description:
                "The instance contains minimum lot-size " +
                "restrictions.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.MAXIMUM_LOT_SIZES",

            featureCode:
                "hasMaximumLotSizes",

            description:
                "The instance contains maximum lot-size " +
                "restrictions.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.LOT_SIZE_MULTIPLES",

            featureCode:
                "hasLotSizeMultiples",

            description:
                "Production quantities must respect " +
                "lot-size multiples.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.SETUP_TIMES",

            featureCode:
                "hasSetupTimes",

            description:
                "Setups consume production capacity.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.START_UP_COSTS",

            featureCode:
                "hasStartUpCosts",

            description:
                "The model distinguishes start-up costs " +
                "from ordinary setup costs.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.ADDITIONAL_CAPACITY",

            featureCode:
                "hasAdditionalCapacity",

            description:
                "Additional production, storage or " +
                "transport capacity can be acquired.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.PURCHASING",

            featureCode:
                "hasPurchasing",

            description:
                "The instance contains external purchasing " +
                "decisions.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.SUPPLIER_CAPACITY",

            featureCode:
                "hasSupplierCapacityConstraints",

            description:
                "Suppliers are subject to capacity " +
                "constraints.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.SUPPLIER_LEAD_TIMES",

            featureCode:
                "hasSupplierLeadTimes",

            description:
                "Supplier delivery lead times are " +
                "represented.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.TRANSPORTATION",

            featureCode:
                "hasTransportation",

            description:
                "The instance contains transportation " +
                "decisions.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.TRANSPORT_CAPACITY",

            featureCode:
                "hasTransportCapacityConstraints",

            description:
                "Transportation is subject to capacity " +
                "constraints.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.TRANSPORT_LEAD_TIMES",

            featureCode:
                "hasTransportLeadTimes",

            description:
                "Transport lead times are represented.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.WAREHOUSE_CAPACITY",

            featureCode:
                "hasWarehouseCapacityConstraints",

            description:
                "Storage is subject to warehouse-capacity " +
                "constraints.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.MULTI_SITE",

            featureCode:
                "isMultiSite",

            description:
                "The instance contains several physical " +
                "production or storage sites.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.FINANCIAL_CONSTRAINTS",

            featureCode:
                "hasFinancialConstraints",

            description:
                "The instance contains explicit financial " +
                "constraints.");

        AddExtensionRule(
            catalog,
            ruleCode:
                "EXT.MULTIPLE_OBJECTIVES",

            featureCode:
                "hasMultipleObjectives",

            description:
                "The instance contains several objective " +
                "criteria.");
    }

    private static KnownProblemTypeDefinition
        CreateUncapacitatedSingleItemDefinition()
    {
        var definition =
            new KnownProblemTypeDefinition(
                code:
                    UncapacitatedSingleItemCode,

                name:
                    "Uncapacitated single-item " +
                    "lot-sizing problem",

                definitionVersion:
                    StandardCatalogVersion)
            {
                Description =
                    "Within this catalog, LS-U denotes a " +
                    "deterministic multi-period, single-item, " +
                    "single-level production lot-sizing " +
                    "problem without production-capacity " +
                    "constraints.",

                DefaultScope =
                    ProblemClassificationScope
                        .CompleteProblem,

                Priority =
                    100,

                ClosestMatchThreshold =
                    0.75
            };

        definition.ReplaceAlternativeCodes(
            new[]
            {
                "LSU",
                "ULS",
                "ULSP"
            });

        definition.ReplaceRequiredRuleCodes(
            new[]
            {
                "COMMON.HAS_DEMAND",
                "COMMON.DETERMINISTIC_DEMAND",
                "COMMON.MULTI_PERIOD",
                "COMMON.HAS_PRODUCTION",
                "STRUCTURE.SINGLE_ITEM",
                "STRUCTURE.SINGLE_LEVEL",
                "CAPACITY.UNCAPACITATED"
            });

        definition.ReplaceExtensionRuleCodes(
            GetStandardExtensionRuleCodes(
                includeProductionLeadTimes:
                    true));

        return definition;
    }

    private static KnownProblemTypeDefinition
        CreateCapacitatedSingleItemDefinition()
    {
        var definition =
            new KnownProblemTypeDefinition(
                code:
                    CapacitatedSingleItemCode,

                name:
                    "Capacitated single-item " +
                    "lot-sizing problem",

                definitionVersion:
                    StandardCatalogVersion)
            {
                Description =
                    "Within this catalog, LS-C denotes a " +
                    "deterministic multi-period, single-item, " +
                    "single-level production lot-sizing " +
                    "problem with production-capacity " +
                    "constraints.",

                DefaultScope =
                    ProblemClassificationScope
                        .CompleteProblem,

                Priority =
                    110,

                ClosestMatchThreshold =
                    0.75
            };

        definition.ReplaceAlternativeCodes(
            new[]
            {
                "LSC",
                "SICLSP"
            });

        definition.ReplaceRequiredRuleCodes(
            new[]
            {
                "COMMON.HAS_DEMAND",
                "COMMON.DETERMINISTIC_DEMAND",
                "COMMON.MULTI_PERIOD",
                "COMMON.HAS_PRODUCTION",
                "STRUCTURE.SINGLE_ITEM",
                "STRUCTURE.SINGLE_LEVEL",
                "CAPACITY.PRODUCTION_CAPACITATED"
            });

        definition.ReplaceOptionalRuleCodes(
            new[]
            {
                "CAPACITY.TIME_VARYING_PRODUCTION"
            });

        definition.ReplaceExtensionRuleCodes(
            GetStandardExtensionRuleCodes(
                includeProductionLeadTimes:
                    true));

        return definition;
    }

    private static KnownProblemTypeDefinition
        CreateCapacitatedLotSizingDefinition()
    {
        var definition =
            new KnownProblemTypeDefinition(
                code:
                    CapacitatedLotSizingCode,

                name:
                    "Capacitated lot-sizing problem",

                definitionVersion:
                    StandardCatalogVersion)
            {
                Description =
                    "Deterministic multi-period, multi-item, " +
                    "single-level production lot-sizing " +
                    "problem with production-capacity " +
                    "constraints.",

                DefaultScope =
                    ProblemClassificationScope
                        .CompleteProblem,

                Priority =
                    120,

                ClosestMatchThreshold =
                    0.75
            };

        definition.ReplaceAlternativeCodes(
            new[]
            {
                "CLSP-SL",
                "SLCLSP"
            });

        definition.ReplaceRequiredRuleCodes(
            new[]
            {
                "COMMON.HAS_DEMAND",
                "COMMON.DETERMINISTIC_DEMAND",
                "COMMON.MULTI_PERIOD",
                "COMMON.HAS_PRODUCTION",
                "STRUCTURE.MULTI_ITEM",
                "STRUCTURE.SINGLE_LEVEL",
                "CAPACITY.PRODUCTION_CAPACITATED"
            });

        definition.ReplaceOptionalRuleCodes(
            new[]
            {
                "CAPACITY.SHARED_PRODUCTION",
                "CAPACITY.TIME_VARYING_PRODUCTION"
            });

        definition.ReplaceExtensionRuleCodes(
            GetStandardExtensionRuleCodes(
                includeProductionLeadTimes:
                    true));

        return definition;
    }

    private static KnownProblemTypeDefinition
        CreateMultiLevelLotSizingDefinition()
    {
        var definition =
            new KnownProblemTypeDefinition(
                code:
                    MultiLevelLotSizingCode,

                name:
                    "Multi-level lot-sizing problem",

                definitionVersion:
                    StandardCatalogVersion)
            {
                Description =
                    "Deterministic multi-period, multi-item, " +
                    "multi-level production lot-sizing " +
                    "problem without production-capacity " +
                    "constraints and with explicit " +
                    "bill-of-materials relationships.",

                DefaultScope =
                    ProblemClassificationScope
                        .CompleteProblem,

                Priority =
                    125,

                ClosestMatchThreshold =
                    0.75
            };

        definition.ReplaceAlternativeCodes(
            new[]
            {
                "MLULSP",
                "MLULS",
                "MLLS"
            });

        definition.ReplaceRequiredRuleCodes(
            new[]
            {
                "COMMON.HAS_DEMAND",
                "COMMON.DETERMINISTIC_DEMAND",
                "COMMON.MULTI_PERIOD",
                "COMMON.HAS_PRODUCTION",
                "STRUCTURE.MULTI_ITEM",
                "STRUCTURE.MULTI_LEVEL",
                "CAPACITY.UNCAPACITATED"
            });

        /*
         * Production lead times are considered part of the
         * multi-level material-flow representation and are
         * not treated as an extension of MLLP.
         */
        definition.ReplaceExtensionRuleCodes(
            GetStandardExtensionRuleCodes(
                includeProductionLeadTimes:
                    false));

        return definition;
    }

    private static KnownProblemTypeDefinition
        CreateMultiLevelCapacitatedDefinition()
    {
        var definition =
            new KnownProblemTypeDefinition(
                code:
                    MultiLevelCapacitatedLotSizingCode,

                name:
                    "Multi-level capacitated " +
                    "lot-sizing problem",

                definitionVersion:
                    StandardCatalogVersion)
            {
                Description =
                    "Deterministic multi-period, multi-item, " +
                    "multi-level production lot-sizing " +
                    "problem with production-capacity " +
                    "constraints and explicit " +
                    "bill-of-materials relationships.",

                DefaultScope =
                    ProblemClassificationScope
                        .CompleteProblem,

                Priority =
                    130,

                ClosestMatchThreshold =
                    0.75
            };

        definition.ReplaceAlternativeCodes(
            new[]
            {
                "ML-CLSP",
                "MLCLP"
            });

        definition.ReplaceRequiredRuleCodes(
            new[]
            {
                "COMMON.HAS_DEMAND",
                "COMMON.DETERMINISTIC_DEMAND",
                "COMMON.MULTI_PERIOD",
                "COMMON.HAS_PRODUCTION",
                "STRUCTURE.MULTI_ITEM",
                "STRUCTURE.MULTI_LEVEL",
                "CAPACITY.PRODUCTION_CAPACITATED"
            });

        definition.ReplaceOptionalRuleCodes(
            new[]
            {
                "CAPACITY.SHARED_PRODUCTION",
                "CAPACITY.TIME_VARYING_PRODUCTION"
            });

        /*
         * Production lead times are considered part of the
         * multi-level material-flow representation and are
         * not treated as an extension of MLCLSP.
         */
        definition.ReplaceExtensionRuleCodes(
            GetStandardExtensionRuleCodes(
                includeProductionLeadTimes:
                    false));

        return definition;
    }

    private static KnownProblemRuleDefinition
        CreateBooleanRule(
            string ruleCode,
            string featureCode,
            bool expectedValue,
            string description)
    {
        return new KnownProblemRuleDefinition(
            ruleCode:
                ruleCode,

            featureCode:
                featureCode,

            operatorCode:
                expectedValue
                    ? KnownProblemRuleDefinition
                        .IsTrueOperator
                    : KnownProblemRuleDefinition
                        .IsFalseOperator,

            description:
                description);
    }

    private static void AddExtensionRule(
        KnownProblemTypeCatalog catalog,
        string ruleCode,
        string featureCode,
        string description)
    {
        KnownProblemRuleDefinition rule =
            CreateBooleanRule(
                ruleCode,
                featureCode,
                expectedValue:
                    true,
                description);

        /*
         * Extension rules are recorded as evidence but do not
         * contribute to the classical-family similarity score.
         */
        rule.Weight =
            0.0;

        catalog.AddRule(
            rule);
    }

    private static IReadOnlyList<string>
        GetStandardExtensionRuleCodes(
            bool includeProductionLeadTimes)
    {
        var ruleCodes =
            new List<string>
            {
                "EXT.SAFETY_STOCK",
                "EXT.BACKLOGGING",
                "EXT.LOST_SALES",
                "EXT.MINIMUM_LOT_SIZES",
                "EXT.MAXIMUM_LOT_SIZES",
                "EXT.LOT_SIZE_MULTIPLES",
                "EXT.SETUP_TIMES",
                "EXT.START_UP_COSTS",
                "EXT.ADDITIONAL_CAPACITY",
                "EXT.PURCHASING",
                "EXT.SUPPLIER_CAPACITY",
                "EXT.SUPPLIER_LEAD_TIMES",
                "EXT.TRANSPORTATION",
                "EXT.TRANSPORT_CAPACITY",
                "EXT.TRANSPORT_LEAD_TIMES",
                "EXT.WAREHOUSE_CAPACITY",
                "EXT.MULTI_SITE",
                "EXT.FINANCIAL_CONSTRAINTS",
                "EXT.MULTIPLE_OBJECTIVES"
            };

        if (includeProductionLeadTimes)
        {
            ruleCodes.Add(
                "EXT.PRODUCTION_LEAD_TIMES");
        }

        return ruleCodes;
    }
}