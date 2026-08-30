using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Instance.ProblemClasses;

namespace LotSizingDataModel.Solver.Formulation.Scientific;

/// <summary>
/// Scientific capability profiles for existing mathematical formulations.
/// </summary>
public static class MathematicalFormulationScientificCatalog
{
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
                    LotSizingProblemClassExtensionKind.FinancialConstraints
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

    public static IReadOnlyList<MathematicalFormulationScientificProfile>
        All { get; } =
            new[]
            {
                Standard
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
