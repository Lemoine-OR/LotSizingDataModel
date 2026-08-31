using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Instance.ProblemClasses;

namespace LotSizingDataModel.Solver.Formulation.Scientific;

/// <summary>
/// Scientific capability profiles for existing mathematical formulations.
/// </summary>
public static class MathematicalFormulationScientificCatalog
{
    private static readonly LotSizingProblemClassExtensionKind[]
        SmallBucketKnownUnsupportedExtensions =
            new[]
            {
                LotSizingProblemClassExtensionKind.StartUpCosts,
                LotSizingProblemClassExtensionKind.StartUpTimes,
                LotSizingProblemClassExtensionKind.ProductionLeadTimes,
                LotSizingProblemClassExtensionKind.MinimumLotSize,
                LotSizingProblemClassExtensionKind.MaximumLotSize,
                LotSizingProblemClassExtensionKind.LotSizeMultiple,
                LotSizingProblemClassExtensionKind.MultiSite,
                LotSizingProblemClassExtensionKind.MultipleObjectives,
                LotSizingProblemClassExtensionKind.BigBucketScheduling,
                LotSizingProblemClassExtensionKind.MacroMicroScheduling,
                LotSizingProblemClassExtensionKind.SequenceDependentChangeoverTimes,
                LotSizingProblemClassExtensionKind.SequenceDependentChangeoverCosts
            };


    public static MathematicalFormulationScientificProfile Standard { get; } =
        new(
            formulationId:
                StandardLotSizingFormulation.StandardFormulationId,
            formulationFamily:
                "Standard solver-independent MILP",
            supportedProblemClasses:
                new[]
                {
                    CanonicalLotSizingProblemClassId
                        .SingleItemUncapacitatedLotSizing,
                    CanonicalLotSizingProblemClassId
                        .SingleItemCapacitatedLotSizing,
                    CanonicalLotSizingProblemClassId
                        .MultiItemUncapacitatedLotSizing,
                    CanonicalLotSizingProblemClassId
                        .MultiItemCapacitatedLotSizing,
                    CanonicalLotSizingProblemClassId
                        .UncapacitatedMultiLevelLotSizing,
                    CanonicalLotSizingProblemClassId
                        .MultiLevelCapacitatedLotSizing
                },
            supportedExtensions:
                new[]
                {
                    LotSizingProblemClassExtensionKind.InitialInventory,
                    LotSizingProblemClassExtensionKind.SafetyStock,
                    LotSizingProblemClassExtensionKind.Backlogging,
                    LotSizingProblemClassExtensionKind.SetupTimes,
                    LotSizingProblemClassExtensionKind.ProductionLeadTimes,
                    LotSizingProblemClassExtensionKind.MinimumLotSize,
                    LotSizingProblemClassExtensionKind.MaximumLotSize,
                    LotSizingProblemClassExtensionKind.LotSizeMultiple,
                    LotSizingProblemClassExtensionKind
                        .AdditionalProductionCapacity,
                    LotSizingProblemClassExtensionKind
                        .AdditionalWarehouseCapacity,
                    LotSizingProblemClassExtensionKind
                        .AdditionalTransportCapacity,
                    LotSizingProblemClassExtensionKind.Purchasing,
                    LotSizingProblemClassExtensionKind.SupplierCapacity,
                    LotSizingProblemClassExtensionKind.SupplierLeadTime,
                    LotSizingProblemClassExtensionKind.Transportation,
                    LotSizingProblemClassExtensionKind.TransportCapacity,
                    LotSizingProblemClassExtensionKind.TransportLeadTime,
                    LotSizingProblemClassExtensionKind.Distribution,
                    LotSizingProblemClassExtensionKind.WarehouseCapacity,
                    LotSizingProblemClassExtensionKind.MultiSite,
                    LotSizingProblemClassExtensionKind.FinancialConstraints,
                    LotSizingProblemClassExtensionKind.GroupingConstraint
                },
            knownUnsupportedExtensions:
                new[]
                {
                    LotSizingProblemClassExtensionKind.StartUpCosts,
                    LotSizingProblemClassExtensionKind.StartUpTimes,
                    LotSizingProblemClassExtensionKind.MultipleObjectives,
                    LotSizingProblemClassExtensionKind.IntegratedScheduling,
                    LotSizingProblemClassExtensionKind.BigBucketScheduling,
                    LotSizingProblemClassExtensionKind.SmallBucketScheduling,
                    LotSizingProblemClassExtensionKind.MacroMicroScheduling,
                    LotSizingProblemClassExtensionKind.InitialSetupState,
                    LotSizingProblemClassExtensionKind.SetupCarryOver,
                    LotSizingProblemClassExtensionKind
                        .SequenceDependentChangeoverTimes,
                    LotSizingProblemClassExtensionKind
                        .SequenceDependentChangeoverCosts,
                    LotSizingProblemClassExtensionKind.MaximumSetupCount
                },
            supportedObjectiveKinds:
                new[]
                {
                    OptimizationObjectiveKind.Economic
                },
            evidence:
                new[]
                {
                    "StandardLotSizingFormulationFactory variable/objective/" +
                    "constraint builder registry",
                    "InventoryBalanceConstraintFamilyBuilder lead-time and " +
                    "initial-inventory semantics",
                    "WorkCenterCapacityConstraintFamilyBuilder setup-time " +
                    "semantics",
                    "SafetyStockConstraintFamilyBuilder",
                    "BacklogVariableFamilyBuilder",
                    "MinimumLotSizeConstraintFamilyBuilder",
                    "MaximumLotSizeConstraintFamilyBuilder",
                    "LotSizeMultipleConstraintFamilyBuilder",
                    "ProcurementVariableFamilyBuilder",
                    "SupplierCapacityConstraintFamilyBuilder",
                    "TransportVariableFamilyBuilder",
                    "WarehouseCapacityConstraintFamilyBuilder",
                    "PeriodicOperatingExpenditureBudgetConstraintFamilyBuilder"
                });

    public static MathematicalFormulationScientificProfile
        DlspSmallBucket { get; } =
            new(
                formulationId:
                    SmallBucketSchedulingFormulation.DlspFormulationId,
                formulationFamily:
                    "Executable single-resource DLSP small-bucket MILP",
                supportedProblemClasses:
                    new[]
                    {
                        CanonicalLotSizingProblemClassId
                            .DiscreteLotSizingAndScheduling
                    },
                supportedExtensions:
                    new[]
                    {
                        LotSizingProblemClassExtensionKind.InitialInventory,
                        LotSizingProblemClassExtensionKind.SafetyStock,
                        LotSizingProblemClassExtensionKind.Backlogging,
                        LotSizingProblemClassExtensionKind
                            .AdditionalWarehouseCapacity,
                        LotSizingProblemClassExtensionKind
                            .AdditionalTransportCapacity,
                        LotSizingProblemClassExtensionKind.Purchasing,
                        LotSizingProblemClassExtensionKind.SupplierCapacity,
                        LotSizingProblemClassExtensionKind.SupplierLeadTime,
                        LotSizingProblemClassExtensionKind.Transportation,
                        LotSizingProblemClassExtensionKind.TransportCapacity,
                        LotSizingProblemClassExtensionKind.TransportLeadTime,
                        LotSizingProblemClassExtensionKind.Distribution,
                        LotSizingProblemClassExtensionKind.WarehouseCapacity,
                        LotSizingProblemClassExtensionKind.FinancialConstraints,
                        LotSizingProblemClassExtensionKind.SetupCarryOver,
                    LotSizingProblemClassExtensionKind.InitialSetupState,
                    LotSizingProblemClassExtensionKind.MaximumSetupCount,
                    LotSizingProblemClassExtensionKind.GroupingConstraint,
                    LotSizingProblemClassExtensionKind.SetupCarryOverForbidden
                },
                knownUnsupportedExtensions:
                    SmallBucketKnownUnsupportedExtensions
                        .Concat(new[]
                        {
                            LotSizingProblemClassExtensionKind.SetupTimes,
                            LotSizingProblemClassExtensionKind.AdditionalProductionCapacity
                        })
                        .ToArray(),
                supportedObjectiveKinds:
                    new[]
                    {
                        OptimizationObjectiveKind.Economic
                    },
                evidence:
                    new[]
                    {
                        "SmallBucketSetupStateVariableFamilyBuilder",
                        "SmallBucketProductionActivationVariableFamilyBuilder",
                        "SmallBucketSetupStartVariableFamilyBuilder",
                        "SmallBucketSingleSetupStateConstraintFamilyBuilder",
                        "SmallBucketSetupStartDefinitionConstraintFamilyBuilder",
                        "SmallBucketSchedulingCapacityConstraintFamilyBuilder",
                        "SmallBucketSetupCountConstraintFamilyBuilder",
                        "SmallBucketGroupingConstraintFamilyBuilder",
                        "SmallBucketProductionStateConstraintFamilyBuilder",
                        "SmallBucketProducedItemCountConstraintFamilyBuilder",
                        "SmallBucketSetupStartCostObjectiveTermBuilder"
                    });

    public static MathematicalFormulationScientificProfile
        CslpSmallBucket { get; } =
            new(
                formulationId:
                    SmallBucketSchedulingFormulation.CslpFormulationId,
                formulationFamily:
                    "Executable single-resource CSLP small-bucket MILP",
                supportedProblemClasses:
                    new[]
                    {
                        CanonicalLotSizingProblemClassId
                            .ContinuousSetupLotSizing
                    },
                supportedExtensions:
                    new[]
                    {
                        LotSizingProblemClassExtensionKind.InitialInventory,
                        LotSizingProblemClassExtensionKind.SafetyStock,
                        LotSizingProblemClassExtensionKind.Backlogging,
                        LotSizingProblemClassExtensionKind
                            .AdditionalWarehouseCapacity,
                        LotSizingProblemClassExtensionKind
                            .AdditionalTransportCapacity,
                        LotSizingProblemClassExtensionKind.Purchasing,
                        LotSizingProblemClassExtensionKind.SupplierCapacity,
                        LotSizingProblemClassExtensionKind.SupplierLeadTime,
                        LotSizingProblemClassExtensionKind.Transportation,
                        LotSizingProblemClassExtensionKind.TransportCapacity,
                        LotSizingProblemClassExtensionKind.TransportLeadTime,
                        LotSizingProblemClassExtensionKind.Distribution,
                        LotSizingProblemClassExtensionKind.WarehouseCapacity,
                        LotSizingProblemClassExtensionKind.FinancialConstraints,
                        LotSizingProblemClassExtensionKind.SetupCarryOver,
                    LotSizingProblemClassExtensionKind.SetupTimes,
                    LotSizingProblemClassExtensionKind.AdditionalProductionCapacity,
                    LotSizingProblemClassExtensionKind.InitialSetupState,
                    LotSizingProblemClassExtensionKind.MaximumSetupCount,
                    LotSizingProblemClassExtensionKind.GroupingConstraint,
                    LotSizingProblemClassExtensionKind.SetupCarryOverForbidden
                },
                knownUnsupportedExtensions:
                    SmallBucketKnownUnsupportedExtensions,
                supportedObjectiveKinds:
                    new[]
                    {
                        OptimizationObjectiveKind.Economic
                    },
                evidence:
                    new[]
                    {
                        "SmallBucketSetupStateVariableFamilyBuilder",
                        "SmallBucketSetupStartVariableFamilyBuilder",
                        "SmallBucketSingleSetupStateConstraintFamilyBuilder",
                        "SmallBucketSetupStartDefinitionConstraintFamilyBuilder",
                        "SmallBucketSchedulingCapacityConstraintFamilyBuilder",
                        "SmallBucketSetupCountConstraintFamilyBuilder",
                        "SmallBucketGroupingConstraintFamilyBuilder",
                        "SmallBucketProductionStateConstraintFamilyBuilder",
                        "SmallBucketProducedItemCountConstraintFamilyBuilder",
                        "SmallBucketSetupStartCostObjectiveTermBuilder"
                    });

    public static MathematicalFormulationScientificProfile
        PlspSmallBucket { get; } =
            new(
                formulationId:
                    SmallBucketSchedulingFormulation.PlspFormulationId,
                formulationFamily:
                    "Executable single-resource PLSP small-bucket MILP",
                supportedProblemClasses:
                    new[]
                    {
                        CanonicalLotSizingProblemClassId
                            .ProportionalLotSizingAndScheduling
                    },
                supportedExtensions:
                    new[]
                    {
                        LotSizingProblemClassExtensionKind.InitialInventory,
                        LotSizingProblemClassExtensionKind.SafetyStock,
                        LotSizingProblemClassExtensionKind.Backlogging,
                        LotSizingProblemClassExtensionKind
                            .AdditionalWarehouseCapacity,
                        LotSizingProblemClassExtensionKind
                            .AdditionalTransportCapacity,
                        LotSizingProblemClassExtensionKind.Purchasing,
                        LotSizingProblemClassExtensionKind.SupplierCapacity,
                        LotSizingProblemClassExtensionKind.SupplierLeadTime,
                        LotSizingProblemClassExtensionKind.Transportation,
                        LotSizingProblemClassExtensionKind.TransportCapacity,
                        LotSizingProblemClassExtensionKind.TransportLeadTime,
                        LotSizingProblemClassExtensionKind.Distribution,
                        LotSizingProblemClassExtensionKind.WarehouseCapacity,
                        LotSizingProblemClassExtensionKind.FinancialConstraints,
                        LotSizingProblemClassExtensionKind.SetupCarryOver,
                    LotSizingProblemClassExtensionKind.SetupTimes,
                    LotSizingProblemClassExtensionKind.AdditionalProductionCapacity,
                    LotSizingProblemClassExtensionKind.InitialSetupState,
                    LotSizingProblemClassExtensionKind.GroupingConstraint
                },
                knownUnsupportedExtensions:
                    SmallBucketKnownUnsupportedExtensions
                        .Concat(new[]
                        {
                            LotSizingProblemClassExtensionKind.SetupCarryOverForbidden
                        })
                        .ToArray(),
                supportedObjectiveKinds:
                    new[]
                    {
                        OptimizationObjectiveKind.Economic
                    },
                evidence:
                    new[]
                    {
                        "SmallBucketSetupStateVariableFamilyBuilder",
                        "SmallBucketProductionActivationVariableFamilyBuilder",
                        "SmallBucketSetupStartVariableFamilyBuilder",
                        "PlspSingleSetupStateConstraintFamilyBuilder",
                        "SmallBucketSetupStartDefinitionConstraintFamilyBuilder",
                        "SmallBucketSchedulingCapacityConstraintFamilyBuilder",
                        "SmallBucketSetupCountConstraintFamilyBuilder",
                        "SmallBucketGroupingConstraintFamilyBuilder",
                        "SmallBucketProductionStateConstraintFamilyBuilder",
                        "SmallBucketProducedItemCountConstraintFamilyBuilder",
                        "PlspSetupTransitionLimitConstraintFamilyBuilder",
                        "SmallBucketSetupStartCostObjectiveTermBuilder"
                    });

    public static MathematicalFormulationScientificProfile
        GlspMacroMicro { get; } =
            new(
                formulationId:
                    GlspSchedulingFormulation.FormulationIdValue,
                formulationFamily:
                    "Executable single-resource GLSP macro/micro MILP",
                supportedProblemClasses:
                    new[]
                    {
                        CanonicalLotSizingProblemClassId
                            .GeneralLotSizingAndScheduling
                    },
                supportedExtensions:
                    new[]
                    {
                        LotSizingProblemClassExtensionKind.InitialInventory,
                        LotSizingProblemClassExtensionKind.SafetyStock,
                        LotSizingProblemClassExtensionKind.Backlogging,
                        LotSizingProblemClassExtensionKind.AdditionalWarehouseCapacity,
                        LotSizingProblemClassExtensionKind.AdditionalTransportCapacity,
                        LotSizingProblemClassExtensionKind.Purchasing,
                        LotSizingProblemClassExtensionKind.SupplierCapacity,
                        LotSizingProblemClassExtensionKind.SupplierLeadTime,
                        LotSizingProblemClassExtensionKind.Transportation,
                        LotSizingProblemClassExtensionKind.TransportCapacity,
                        LotSizingProblemClassExtensionKind.TransportLeadTime,
                        LotSizingProblemClassExtensionKind.Distribution,
                        LotSizingProblemClassExtensionKind.WarehouseCapacity,
                        LotSizingProblemClassExtensionKind.ProductionLeadTimes,
                        LotSizingProblemClassExtensionKind.SetupCarryOver,
                        LotSizingProblemClassExtensionKind.SequenceDependentChangeoverTimes,
                        LotSizingProblemClassExtensionKind.SequenceDependentChangeoverCosts,
                    LotSizingProblemClassExtensionKind.SetupTimes,
                    LotSizingProblemClassExtensionKind.AdditionalProductionCapacity,
                    LotSizingProblemClassExtensionKind.InitialSetupState,
                    LotSizingProblemClassExtensionKind.MaximumSetupCount,
                    LotSizingProblemClassExtensionKind.MaximumProducedItemCount,
                    LotSizingProblemClassExtensionKind.GroupingConstraint,
                    LotSizingProblemClassExtensionKind.SetupCarryOverForbidden
                },
                knownUnsupportedExtensions:
                    new[]
                    {

                        LotSizingProblemClassExtensionKind.StartUpCosts,
                        LotSizingProblemClassExtensionKind.StartUpTimes,
                        LotSizingProblemClassExtensionKind.MinimumLotSize,
                        LotSizingProblemClassExtensionKind.MaximumLotSize,
                        LotSizingProblemClassExtensionKind.LotSizeMultiple,

                        LotSizingProblemClassExtensionKind.MultiSite,
                        LotSizingProblemClassExtensionKind.MultipleObjectives,
                        LotSizingProblemClassExtensionKind.FinancialConstraints,
                        LotSizingProblemClassExtensionKind.BigBucketScheduling,
                        LotSizingProblemClassExtensionKind.SmallBucketScheduling,

                    },
                supportedObjectiveKinds:
                    new[]
                    {
                        OptimizationObjectiveKind.Economic
                    },
                evidence:
                    new[]
                    {
                        "GlspMicroProductionVariableFamilyBuilder",
                        "GlspMicroSetupStateVariableFamilyBuilder",
                        "GlspChangeoverVariableFamilyBuilder",
                        "GlspAggregateProductionConstraintFamilyBuilder",
                        "GlspSingleSetupStateConstraintFamilyBuilder",
                        "GlspMicroProductionLinkConstraintFamilyBuilder",
                        "GlspChangeoverDefinitionConstraintFamilyBuilder",
                        "GlspSetupStartVariableFamilyBuilder",
                        "GlspSetupStartDefinitionConstraintFamilyBuilder",
                        "GlspSetupStartCostObjectiveTermBuilder",
                        "GlspSetupCountConstraintFamilyBuilder",
                        "GlspGroupingConstraintFamilyBuilder",
                        "GlspProducedItemCountConstraintFamilyBuilder",
                        "GlspMacroCapacityConstraintFamilyBuilder",
                        "GlspChangeoverCostObjectiveTermBuilder",
                        "GlspMicroPeriodSetupStateDecisionMapper",
                        "MathematicalSolutionValueProjector"
                    });

    public static IReadOnlyList<MathematicalFormulationScientificProfile>
        All { get; } =
            new[]
            {
                Standard,
                DlspSmallBucket,
                CslpSmallBucket,
                PlspSmallBucket,
                GlspMacroMicro
            };

    public static bool TryGet(
        string formulationId,
        out MathematicalFormulationScientificProfile? profile)
    {
        profile =
            All.FirstOrDefault(
                candidate =>
                    candidate.FormulationId.Equals(
                        formulationId?.Trim(),
                        StringComparison.OrdinalIgnoreCase));

        return profile is not null;
    }

    public static MathematicalFormulationScientificProfile? Find(
        string formulationId)
    {
        TryGet(formulationId, out var profile);
        return profile;
    }
}
