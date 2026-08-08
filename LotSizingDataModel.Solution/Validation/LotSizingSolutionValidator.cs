using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Core;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Solution.Common;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solution.Evaluation;

namespace LotSizingDataModel.Solution.Validation;

/// <summary>
/// Identifies the severity of a solution-validation issue.
/// </summary>
public enum SolutionValidationSeverity
{
    /// <summary>
    /// The issue does not necessarily make the solution invalid,
    /// but should be reviewed.
    /// </summary>
    Warning,

    /// <summary>
    /// The issue makes the solution structurally invalid
    /// or incompatible with its associated instance.
    /// </summary>
    Error
}

/// <summary>
/// Describes one issue detected while validating
/// a lot-sizing solution.
/// </summary>
public sealed class SolutionValidationIssue
{
    /// <summary>
    /// Initializes a solution-validation issue.
    /// </summary>
    /// <param name="severity">
    /// Severity of the issue.
    /// </param>
    /// <param name="code">
    /// Stable validation code.
    /// </param>
    /// <param name="path">
    /// Logical path of the affected value.
    /// </param>
    /// <param name="message">
    /// Human-readable description of the issue.
    /// </param>
    public SolutionValidationIssue(
        SolutionValidationSeverity severity,
        string code,
        string path,
        string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Severity = severity;
        Code = code.Trim();
        Path = path?.Trim() ?? string.Empty;
        Message = message.Trim();
    }

    /// <summary>
    /// Gets the severity of the issue.
    /// </summary>
    public SolutionValidationSeverity Severity { get; }

    /// <summary>
    /// Gets the stable validation code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the logical path of the affected value.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the human-readable description of the issue.
    /// </summary>
    public string Message { get; }

    /// <inheritdoc/>
    public override string ToString()
    {
        string pathDescription =
            string.IsNullOrWhiteSpace(Path)
                ? string.Empty
                : $" {Path}:";

        return
            $"[{Severity}] {Code}{pathDescription} {Message}";
    }
}

/// <summary>
/// Validates the structure and instance compatibility
/// of a lot-sizing solution.
/// </summary>
/// <remarks>
/// This validator checks identifiers, planning horizons,
/// duplicate decision keys, decision-value domains,
/// evaluation consistency and references to a
/// <see cref="SupplyChain"/> instance.
///
/// It does not independently recompute material balances,
/// capacity consumption or objective values.
/// </remarks>
public sealed class LotSizingSolutionValidator
{
    private readonly double _numericalTolerance;

    /// <summary>
    /// Initializes a solution validator.
    /// </summary>
    /// <param name="numericalTolerance">
    /// Non-negative finite tolerance used when comparing
    /// numerical evaluation values with zero.
    /// </param>
    public LotSizingSolutionValidator(
        double numericalTolerance = 1e-9)
    {
        if (!double.IsFinite(numericalTolerance) ||
            numericalTolerance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(numericalTolerance),
                numericalTolerance,
                "The numerical tolerance must be finite " +
                "and non-negative.");
        }

        _numericalTolerance =
            numericalTolerance;
    }

    /// <summary>
    /// Gets the numerical tolerance used by the validator.
    /// </summary>
    public double NumericalTolerance =>
        _numericalTolerance;

    /// <summary>
    /// Validates the internal structure of a solution.
    /// </summary>
    /// <param name="solution">
    /// Solution to validate.
    /// </param>
    /// <returns>
    /// Detected warnings and errors.
    /// </returns>
    public IReadOnlyList<SolutionValidationIssue> Validate(
        LotSizingSolution solution)
    {
        ArgumentNullException.ThrowIfNull(solution);

        var issues =
            new List<SolutionValidationIssue>();

        ValidateGeneralInformation(
            solution,
            issues);

        ValidateGenerationMetadata(
            solution,
            issues);

        ValidateEvaluation(
            solution,
            issues);

        ValidateDecisions(
            solution,
            issues);

        return issues;
    }

    /// <summary>
    /// Validates a solution and its compatibility with
    /// a supply-chain instance.
    /// </summary>
    /// <param name="solution">
    /// Solution to validate.
    /// </param>
    /// <param name="supplyChain">
    /// Supply-chain instance associated with the solution.
    /// </param>
    /// <returns>
    /// Detected warnings and errors.
    /// </returns>
    public IReadOnlyList<SolutionValidationIssue> Validate(
        LotSizingSolution solution,
        SupplyChain supplyChain)
    {
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(supplyChain);

        var issues =
            Validate(solution).ToList();

        ValidateAgainstSupplyChain(
            solution,
            supplyChain,
            issues);

        return issues;
    }

    /// <summary>
    /// Determines whether a solution contains no
    /// internal validation errors.
    /// </summary>
    /// <param name="solution">
    /// Solution to validate.
    /// </param>
    /// <returns>
    /// True when no error is detected; otherwise, false.
    /// </returns>
    public bool IsValid(
        LotSizingSolution solution)
    {
        return !Validate(solution).Any(
            issue =>
                issue.Severity ==
                SolutionValidationSeverity.Error);
    }

    /// <summary>
    /// Determines whether a solution is structurally valid
    /// and compatible with a supply-chain instance.
    /// </summary>
    /// <param name="solution">
    /// Solution to validate.
    /// </param>
    /// <param name="supplyChain">
    /// Associated supply-chain instance.
    /// </param>
    /// <returns>
    /// True when no error is detected; otherwise, false.
    /// </returns>
    public bool IsValid(
        LotSizingSolution solution,
        SupplyChain supplyChain)
    {
        return !Validate(
                solution,
                supplyChain)
            .Any(
                issue =>
                    issue.Severity ==
                    SolutionValidationSeverity.Error);
    }

    /// <summary>
    /// Validates a solution and throws an exception
    /// when at least one error is detected.
    /// </summary>
    /// <param name="solution">
    /// Solution to validate.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the solution contains at least one error.
    /// </exception>
    public void ThrowIfInvalid(
        LotSizingSolution solution)
    {
        ThrowIfErrors(
            Validate(solution));
    }

    /// <summary>
    /// Validates a solution against a supply-chain instance
    /// and throws an exception when an error is detected.
    /// </summary>
    /// <param name="solution">
    /// Solution to validate.
    /// </param>
    /// <param name="supplyChain">
    /// Associated supply-chain instance.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the solution contains at least one error.
    /// </exception>
    public void ThrowIfInvalid(
        LotSizingSolution solution,
        SupplyChain supplyChain)
    {
        ThrowIfErrors(
            Validate(
                solution,
                supplyChain));
    }

    private static void ValidateGeneralInformation(
        LotSizingSolution solution,
        ICollection<SolutionValidationIssue> issues)
    {
        if (solution.Id == Guid.Empty)
        {
            AddError(
                issues,
                "SOL001",
                "id",
                "The solution identifier cannot be empty.");
        }

        if (solution.PlanningHorizon <= 0)
        {
            AddError(
                issues,
                "SOL002",
                "planningHorizon",
                "The planning horizon must be strictly positive.");
        }

        if (string.IsNullOrWhiteSpace(
                solution.InstanceIdentifier))
        {
            AddWarning(
                issues,
                "SOL003",
                "instanceIdentifier",
                "No instance identifier is recorded.");
        }

        if (!solution.HasDecisions)
        {
            AddError(
                issues,
                "SOL004",
                "decisions",
                "The solution does not contain any " +
                "decision object.");
        }

        if (solution.Completeness ==
            SolutionCompleteness.Unknown)
        {
            AddWarning(
                issues,
                "SOL005",
                "completeness",
                "The completeness of the solution " +
                "has not been evaluated.");
        }

        if (solution.Completeness ==
                SolutionCompleteness.Partial &&
            solution.Evaluation.FeasibilityStatus ==
                FeasibilityStatus.Feasible)
        {
            AddError(
                issues,
                "SOL006",
                "evaluation.feasibilityStatus",
                "A partial solution cannot be declared " +
                "globally feasible.");
        }

        if (solution.Completeness !=
                SolutionCompleteness.Complete &&
            solution.Evaluation.OptimalityStatus ==
                OptimalityStatus.ProvenOptimal)
        {
            AddError(
                issues,
                "SOL007",
                "evaluation.optimalityStatus",
                "A solution cannot be declared proven optimal " +
                "unless it is complete.");
        }
    }

    private static void ValidateGenerationMetadata(
        LotSizingSolution solution,
        ICollection<SolutionValidationIssue> issues)
    {
        if (solution.GenerationMetadata.MethodKind ==
            SolutionMethodKind.Unknown)
        {
            AddWarning(
                issues,
                "GEN001",
                "generationMetadata.methodKind",
                "The solution-generation method is unknown.");
        }

        if (solution.GenerationMetadata.TerminationReason ==
            TerminationReason.Unknown)
        {
            AddWarning(
                issues,
                "GEN002",
                "generationMetadata.terminationReason",
                "The execution termination reason is unknown.");
        }

        var duplicateParameterNames =
            solution.GenerationMetadata.Parameters
                .Where(
                    parameter =>
                        !string.IsNullOrWhiteSpace(
                            parameter.Name))
                .GroupBy(
                    parameter =>
                        parameter.Name,
                    StringComparer.OrdinalIgnoreCase)
                .Where(
                    group =>
                        group.Count() > 1)
                .Select(
                    group =>
                        group.Key);

        foreach (string parameterName
                 in duplicateParameterNames)
        {
            AddError(
                issues,
                "GEN003",
                "generationMetadata.parameters",
                $"The algorithm parameter " +
                $"'{parameterName}' is duplicated.");
        }

        for (int index = 0;
             index <
             solution.GenerationMetadata.Parameters.Count;
             index++)
        {
            if (string.IsNullOrWhiteSpace(
                    solution.GenerationMetadata
                        .Parameters[index]
                        .Name))
            {
                AddError(
                    issues,
                    "GEN004",
                    $"generationMetadata.parameters[{index}]",
                    "The algorithm parameter name is empty.");
            }
        }

        if (solution.GenerationMetadata.TerminationReason ==
                TerminationReason.OptimalityProven &&
            solution.Evaluation.OptimalityStatus !=
                OptimalityStatus.ProvenOptimal)
        {
            AddWarning(
                issues,
                "GEN005",
                "generationMetadata.terminationReason",
                "The generation metadata indicates that " +
                "optimality was proven, but the evaluation " +
                "does not declare the solution proven optimal.");
        }
    }

    private void ValidateEvaluation(
        LotSizingSolution solution,
        ICollection<SolutionValidationIssue> issues)
    {
        SolutionEvaluation evaluation =
            solution.Evaluation;

        if (evaluation.OptimalityStatus ==
                OptimalityStatus.ProvenOptimal &&
            evaluation.FeasibilityStatus !=
                FeasibilityStatus.Feasible)
        {
            AddError(
                issues,
                "EVA001",
                "evaluation.optimalityStatus",
                "A proven-optimal solution must be feasible.");
        }

        if (evaluation.OptimalityStatus ==
                OptimalityStatus.ProvenOptimal &&
            !evaluation.ObjectiveValue.HasValue)
        {
            AddError(
                issues,
                "EVA002",
                "evaluation.objectiveValue",
                "A proven-optimal solution must have an " +
                "objective value.");
        }

        if (evaluation.OptimalityStatus ==
                OptimalityStatus.ProvenOptimal &&
            evaluation.AbsoluteGap.HasValue &&
            evaluation.AbsoluteGap.Value >
                NumericalTolerance)
        {
            AddError(
                issues,
                "EVA003",
                "evaluation.absoluteGap",
                "The absolute gap of a proven-optimal solution " +
                "must be zero within the numerical tolerance.");
        }

        if (evaluation.OptimalityStatus ==
                OptimalityStatus.ProvenOptimal &&
            evaluation.RelativeGap.HasValue &&
            evaluation.RelativeGap.Value >
                NumericalTolerance)
        {
            AddError(
                issues,
                "EVA004",
                "evaluation.relativeGap",
                "The relative gap of a proven-optimal solution " +
                "must be zero within the numerical tolerance.");
        }

        if ((evaluation.AbsoluteGap.HasValue ||
             evaluation.RelativeGap.HasValue) &&
            !evaluation.BestBound.HasValue)
        {
            AddError(
                issues,
                "EVA005",
                "evaluation.bestBound",
                "An optimality gap is recorded without " +
                "an objective bound.");
        }

        if (evaluation.BestBound.HasValue &&
            !evaluation.ObjectiveValue.HasValue)
        {
            AddError(
                issues,
                "EVA006",
                "evaluation.objectiveValue",
                "An objective bound is recorded without " +
                "an objective value.");
        }

        if (evaluation.MaximumConstraintViolation.HasValue &&
            evaluation.TotalConstraintViolation.HasValue &&
            evaluation.MaximumConstraintViolation.Value >
                evaluation.TotalConstraintViolation.Value +
                NumericalTolerance)
        {
            AddError(
                issues,
                "EVA007",
                "evaluation.maximumConstraintViolation",
                "The maximum constraint violation cannot " +
                "exceed the total constraint violation.");
        }

        bool hasPositiveViolation =
            evaluation.MaximumConstraintViolation >
                NumericalTolerance ||
            evaluation.TotalConstraintViolation >
                NumericalTolerance ||
            evaluation.ViolatedConstraintCount > 0;

        if (evaluation.FeasibilityStatus ==
                FeasibilityStatus.Feasible &&
            hasPositiveViolation)
        {
            AddError(
                issues,
                "EVA008",
                "evaluation.feasibilityStatus",
                "A solution with recorded constraint " +
                "violations cannot be declared feasible.");
        }

        if (evaluation.FeasibilityStatus ==
                FeasibilityStatus.Feasible &&
            solution.Completeness ==
                SolutionCompleteness.Unknown)
        {
            AddWarning(
                issues,
                "EVA009",
                "completeness",
                "The solution is declared feasible while " +
                "its completeness remains unknown.");
        }
    }

    private static void ValidateDecisions(
        LotSizingSolution solution,
        ICollection<SolutionValidationIssue> issues)
    {
        ValidateProductionDecisions(
            solution,
            issues);

        ValidateInventoryDecisions(
            solution,
            issues);

        ValidateTransportDecisions(
            solution,
            issues);

        ValidatePurchaseDecisions(
            solution,
            issues);

        ValidateDistributionDecisions(
            solution,
            issues);

        ValidateWorkCenterCapacityDecisions(
            solution,
            issues);

        ValidateWarehouseCapacityDecisions(
            solution,
            issues);

        ValidateTransportResourceCapacityDecisions(
            solution,
            issues);
    }

    private static void ValidateProductionDecisions(
        LotSizingSolution solution,
        ICollection<SolutionValidationIssue> issues)
    {
        var seenRoutingIds =
            new HashSet<int>();

        for (int index = 0;
             index < solution.ProductionDecisions.Count;
             index++)
        {
            ProductionDecision decision =
                solution.ProductionDecisions[index];

            string path =
                $"productionDecisions[{index}]";

            ValidateDecisionHorizon(
                decision.PlanningHorizon,
                solution.PlanningHorizon,
                path,
                issues);

            if (!decision.IsInternallyValid)
            {
                AddError(
                    issues,
                    "DEC101",
                    path,
                    "The production decision is not " +
                    "internally valid.");
            }

            if (!seenRoutingIds.Add(
                    decision.RoutingId))
            {
                AddError(
                    issues,
                    "DEC102",
                    path,
                    $"A production decision for routing " +
                    $"{decision.RoutingId} is duplicated.");
            }
        }
    }

    private static void ValidateInventoryDecisions(
        LotSizingSolution solution,
        ICollection<SolutionValidationIssue> issues)
    {
        var seenKeys =
            new HashSet<(
                int ItemId,
                WarehouseReferenceKind Kind,
                int ReferenceId)>();

        for (int index = 0;
             index < solution.InventoryDecisions.Count;
             index++)
        {
            InventoryDecision decision =
                solution.InventoryDecisions[index];

            string path =
                $"inventoryDecisions[{index}]";

            ValidateDecisionHorizon(
                decision.PlanningHorizon,
                solution.PlanningHorizon,
                path,
                issues);

            if (!decision.IsInternallyValid)
            {
                AddError(
                    issues,
                    "DEC201",
                    path,
                    "The inventory decision is not " +
                    "internally valid.");
            }

            var key =
                (
                    decision.ItemId,
                    decision.Warehouse.Kind,
                    decision.Warehouse.ReferenceId
                );

            if (!seenKeys.Add(key))
            {
                AddError(
                    issues,
                    "DEC202",
                    path,
                    "The item-and-warehouse inventory " +
                    "decision key is duplicated.");
            }
        }
    }

    private static void ValidateTransportDecisions(
        LotSizingSolution solution,
        ICollection<SolutionValidationIssue> issues)
    {
        var seenKeys =
            new HashSet<(
                int ItemId,
                int ResourceId,
                WarehouseReferenceKind OriginKind,
                int OriginId,
                WarehouseReferenceKind DestinationKind,
                int DestinationId)>();

        for (int index = 0;
             index < solution.TransportDecisions.Count;
             index++)
        {
            TransportDecision decision =
                solution.TransportDecisions[index];

            string path =
                $"transportDecisions[{index}]";

            ValidateDecisionHorizon(
                decision.PlanningHorizon,
                solution.PlanningHorizon,
                path,
                issues);

            if (!decision.IsInternallyValid)
            {
                AddError(
                    issues,
                    "DEC301",
                    path,
                    "The transport decision is not " +
                    "internally valid.");
            }

            var key =
                (
                    decision.ItemId,
                    decision.TransportResourceId,
                    decision.Origin.Kind,
                    decision.Origin.ReferenceId,
                    decision.Destination.Kind,
                    decision.Destination.ReferenceId
                );

            if (!seenKeys.Add(key))
            {
                AddError(
                    issues,
                    "DEC302",
                    path,
                    "The item-resource-lane transport " +
                    "decision key is duplicated.");
            }
        }
    }

    private static void ValidatePurchaseDecisions(
        LotSizingSolution solution,
        ICollection<SolutionValidationIssue> issues)
    {
        var seenKeys =
            new HashSet<(
                int SupplierId,
                int ItemId,
                WarehouseReferenceKind Kind,
                int ReferenceId)>();

        for (int index = 0;
             index < solution.PurchaseDecisions.Count;
             index++)
        {
            PurchaseDecision decision =
                solution.PurchaseDecisions[index];

            string path =
                $"purchaseDecisions[{index}]";

            ValidateDecisionHorizon(
                decision.PlanningHorizon,
                solution.PlanningHorizon,
                path,
                issues);

            if (!decision.IsInternallyValid)
            {
                AddError(
                    issues,
                    "DEC401",
                    path,
                    "The purchase decision is not " +
                    "internally valid.");
            }

            var key =
                (
                    decision.SupplierId,
                    decision.ItemId,
                    decision.DestinationWarehouse.Kind,
                    decision.DestinationWarehouse.ReferenceId
                );

            if (!seenKeys.Add(key))
            {
                AddError(
                    issues,
                    "DEC402",
                    path,
                    "The supplier-item-warehouse purchase " +
                    "decision key is duplicated.");
            }
        }
    }

    private static void ValidateDistributionDecisions(
        LotSizingSolution solution,
        ICollection<SolutionValidationIssue> issues)
    {
        var seenKeys =
            new HashSet<(
                int DistributionCenterId,
                int ItemId,
                WarehouseReferenceKind Kind,
                int ReferenceId)>();

        for (int index = 0;
             index < solution.DistributionDecisions.Count;
             index++)
        {
            DistributionDecision decision =
                solution.DistributionDecisions[index];

            string path =
                $"distributionDecisions[{index}]";

            ValidateDecisionHorizon(
                decision.PlanningHorizon,
                solution.PlanningHorizon,
                path,
                issues);

            if (!decision.IsInternallyValid)
            {
                AddError(
                    issues,
                    "DEC501",
                    path,
                    "The distribution decision is not " +
                    "internally valid.");
            }

            var key =
                (
                    decision.DistributionCenterId,
                    decision.ItemId,
                    decision.Warehouse.Kind,
                    decision.Warehouse.ReferenceId
                );

            if (!seenKeys.Add(key))
            {
                AddError(
                    issues,
                    "DEC502",
                    path,
                    "The distribution-center-item-warehouse " +
                    "decision key is duplicated.");
            }
        }
    }

    private static void ValidateWorkCenterCapacityDecisions(
        LotSizingSolution solution,
        ICollection<SolutionValidationIssue> issues)
    {
        var seenKeys =
            new HashSet<(int PlantId, int WorkCenterId)>();

        for (int index = 0;
             index <
             solution.WorkCenterCapacityDecisions.Count;
             index++)
        {
            WorkCenterCapacityDecision decision =
                solution.WorkCenterCapacityDecisions[index];

            string path =
                $"workCenterCapacityDecisions[{index}]";

            ValidateDecisionHorizon(
                decision.PlanningHorizon,
                solution.PlanningHorizon,
                path,
                issues);

            if (!decision.IsInternallyValid)
            {
                AddError(
                    issues,
                    "DEC601",
                    path,
                    "The work-center capacity decision " +
                    "is not internally valid.");
            }

            var key =
                (
                    decision.WorkCenter.PlantId,
                    decision.WorkCenter.WorkCenterId
                );

            if (!seenKeys.Add(key))
            {
                AddError(
                    issues,
                    "DEC602",
                    path,
                    "The work-center capacity decision " +
                    "key is duplicated.");
            }
        }
    }

    private static void ValidateWarehouseCapacityDecisions(
        LotSizingSolution solution,
        ICollection<SolutionValidationIssue> issues)
    {
        var seenKeys =
            new HashSet<(
                WarehouseReferenceKind Kind,
                int ReferenceId)>();

        for (int index = 0;
             index <
             solution.WarehouseCapacityDecisions.Count;
             index++)
        {
            WarehouseCapacityDecision decision =
                solution.WarehouseCapacityDecisions[index];

            string path =
                $"warehouseCapacityDecisions[{index}]";

            ValidateDecisionHorizon(
                decision.PlanningHorizon,
                solution.PlanningHorizon,
                path,
                issues);

            if (!decision.IsInternallyValid)
            {
                AddError(
                    issues,
                    "DEC701",
                    path,
                    "The warehouse-capacity decision " +
                    "is not internally valid.");
            }

            var key =
                (
                    decision.Warehouse.Kind,
                    decision.Warehouse.ReferenceId
                );

            if (!seenKeys.Add(key))
            {
                AddError(
                    issues,
                    "DEC702",
                    path,
                    "The warehouse-capacity decision " +
                    "key is duplicated.");
            }
        }
    }

    private static void
        ValidateTransportResourceCapacityDecisions(
            LotSizingSolution solution,
            ICollection<SolutionValidationIssue> issues)
    {
        var seenResourceIds =
            new HashSet<int>();

        for (int index = 0;
             index <
             solution.TransportResourceCapacityDecisions.Count;
             index++)
        {
            TransportResourceCapacityDecision decision =
                solution.TransportResourceCapacityDecisions[
                    index];

            string path =
                $"transportResourceCapacityDecisions[{index}]";

            ValidateDecisionHorizon(
                decision.PlanningHorizon,
                solution.PlanningHorizon,
                path,
                issues);

            if (!decision.IsInternallyValid)
            {
                AddError(
                    issues,
                    "DEC801",
                    path,
                    "The transport-resource capacity decision " +
                    "is not internally valid.");
            }

            if (!seenResourceIds.Add(
                    decision.TransportResourceId))
            {
                AddError(
                    issues,
                    "DEC802",
                    path,
                    "The transport-resource capacity decision " +
                    "key is duplicated.");
            }
        }
    }

    private static void ValidateAgainstSupplyChain(
        LotSizingSolution solution,
        SupplyChain supplyChain,
        ICollection<SolutionValidationIssue> issues)
    {
        if (solution.PlanningHorizon !=
            supplyChain.PlanningHorizon)
        {
            AddError(
                issues,
                "INS001",
                "planningHorizon",
                $"The solution horizon " +
                $"({solution.PlanningHorizon}) differs from " +
                $"the instance horizon " +
                $"({supplyChain.PlanningHorizon}).");
        }

        ValidateProductionReferences(
            solution,
            supplyChain,
            issues);

        ValidateInventoryReferences(
            solution,
            supplyChain,
            issues);

        ValidateTransportReferences(
            solution,
            supplyChain,
            issues);

        ValidatePurchaseReferences(
            solution,
            supplyChain,
            issues);

        ValidateDistributionReferences(
            solution,
            supplyChain,
            issues);

        ValidateCapacityReferences(
            solution,
            supplyChain,
            issues);
    }

    private static void ValidateProductionReferences(
        LotSizingSolution solution,
        SupplyChain supplyChain,
        ICollection<SolutionValidationIssue> issues)
    {
        for (int index = 0;
             index < solution.ProductionDecisions.Count;
             index++)
        {
            ProductionDecision decision =
                solution.ProductionDecisions[index];

            bool routingExists =
                supplyChain.ProductionRoutings.Any(
                    routing =>
                        routing.Id ==
                        decision.RoutingId);

            if (!routingExists)
            {
                AddError(
                    issues,
                    "REF101",
                    $"productionDecisions[{index}].routingId",
                    $"Production routing " +
                    $"{decision.RoutingId} does not exist " +
                    "in the supply-chain instance.");
            }
        }
    }

    private static void ValidateInventoryReferences(
        LotSizingSolution solution,
        SupplyChain supplyChain,
        ICollection<SolutionValidationIssue> issues)
    {
        for (int index = 0;
             index < solution.InventoryDecisions.Count;
             index++)
        {
            InventoryDecision decision =
                solution.InventoryDecisions[index];

            string path =
                $"inventoryDecisions[{index}]";

            ValidateItemReference(
                decision.ItemId,
                supplyChain,
                $"{path}.itemId",
                issues);

            ValidateWarehouseReference(
                decision.Warehouse,
                supplyChain,
                $"{path}.warehouse",
                issues);

            bool inventoryExists =
                supplyChain.Inventories.Any(
                    inventory =>
                        inventory.ItemId ==
                            decision.ItemId &&
                        SameWarehouse(
                            inventory.Warehouse,
                            decision.Warehouse));

            if (!inventoryExists)
            {
                AddError(
                    issues,
                    "REF201",
                    path,
                    "No inventory relationship exists " +
                    "for this item and warehouse.");
            }
        }
    }

    private static void ValidateTransportReferences(
        LotSizingSolution solution,
        SupplyChain supplyChain,
        ICollection<SolutionValidationIssue> issues)
    {
        for (int index = 0;
             index < solution.TransportDecisions.Count;
             index++)
        {
            TransportDecision decision =
                solution.TransportDecisions[index];

            string path =
                $"transportDecisions[{index}]";

            ValidateItemReference(
                decision.ItemId,
                supplyChain,
                $"{path}.itemId",
                issues);

            ValidateWarehouseReference(
                decision.Origin,
                supplyChain,
                $"{path}.origin",
                issues);

            ValidateWarehouseReference(
                decision.Destination,
                supplyChain,
                $"{path}.destination",
                issues);

            var resource =
                supplyChain.TransportResources
                    .FirstOrDefault(
                        current =>
                            current.Id ==
                            decision.TransportResourceId);

            if (resource is null)
            {
                AddError(
                    issues,
                    "REF301",
                    $"{path}.transportResourceId",
                    $"Transport resource " +
                    $"{decision.TransportResourceId} " +
                    "does not exist.");

                continue;
            }

            bool characteristicExists =
                supplyChain.TransportCharacteristics.Any(
                    characteristic =>
                        characteristic.ItemId ==
                            decision.ItemId &&
                        characteristic.TransportResourceId ==
                            decision.TransportResourceId);

            if (!characteristicExists)
            {
                AddError(
                    issues,
                    "REF302",
                    path,
                    "No transport characteristic exists " +
                    "for this item and transport resource.");
            }

            bool laneExists =
                resource.Lanes.Any(
                    lane =>
                        SameWarehouse(
                            lane.Origin,
                            decision.Origin) &&
                        SameWarehouse(
                            lane.Destination,
                            decision.Destination));

            if (!laneExists)
            {
                AddError(
                    issues,
                    "REF303",
                    path,
                    "The directed transport lane does not " +
                    "exist for this transport resource.");
            }
        }
    }

    private static void ValidatePurchaseReferences(
        LotSizingSolution solution,
        SupplyChain supplyChain,
        ICollection<SolutionValidationIssue> issues)
    {
        for (int index = 0;
             index < solution.PurchaseDecisions.Count;
             index++)
        {
            PurchaseDecision decision =
                solution.PurchaseDecisions[index];

            string path =
                $"purchaseDecisions[{index}]";

            ValidateItemReference(
                decision.ItemId,
                supplyChain,
                $"{path}.itemId",
                issues);

            ValidateWarehouseReference(
                decision.DestinationWarehouse,
                supplyChain,
                $"{path}.destinationWarehouse",
                issues);

            bool supplierExists =
                supplyChain.Suppliers.Any(
                    supplier =>
                        supplier.Id ==
                        decision.SupplierId);

            if (!supplierExists)
            {
                AddError(
                    issues,
                    "REF401",
                    $"{path}.supplierId",
                    $"Supplier {decision.SupplierId} " +
                    "does not exist.");
            }

            bool deliveryExists =
                supplyChain.SupplierDeliveries.Any(
                    delivery =>
                        delivery.SupplierId ==
                            decision.SupplierId &&
                        delivery.ItemId ==
                            decision.ItemId &&
                        SameWarehouse(
                            delivery.Warehouse,
                            decision.DestinationWarehouse));

            if (!deliveryExists)
            {
                AddError(
                    issues,
                    "REF402",
                    path,
                    "No supplier-delivery relationship exists " +
                    "for this supplier, item and warehouse.");
            }
        }
    }

    private static void ValidateDistributionReferences(
        LotSizingSolution solution,
        SupplyChain supplyChain,
        ICollection<SolutionValidationIssue> issues)
    {
        for (int index = 0;
             index < solution.DistributionDecisions.Count;
             index++)
        {
            DistributionDecision decision =
                solution.DistributionDecisions[index];

            string path =
                $"distributionDecisions[{index}]";

            ValidateItemReference(
                decision.ItemId,
                supplyChain,
                $"{path}.itemId",
                issues);

            ValidateWarehouseReference(
                decision.Warehouse,
                supplyChain,
                $"{path}.warehouse",
                issues);

            bool distributionCenterExists =
                supplyChain.DistributionCenters.Any(
                    distributionCenter =>
                        distributionCenter.Id ==
                        decision.DistributionCenterId);

            if (!distributionCenterExists)
            {
                AddError(
                    issues,
                    "REF501",
                    $"{path}.distributionCenterId",
                    $"Distribution center " +
                    $"{decision.DistributionCenterId} " +
                    "does not exist.");
            }

            bool sourcingExists =
                supplyChain.DistributionCenterSourcings.Any(
                    sourcing =>
                        sourcing.DistributionCenterId ==
                            decision.DistributionCenterId &&
                        sourcing.ItemId ==
                            decision.ItemId &&
                        SameWarehouse(
                            sourcing.Warehouse,
                            decision.Warehouse));

            if (!sourcingExists)
            {
                AddError(
                    issues,
                    "REF502",
                    path,
                    "No distribution-center sourcing " +
                    "relationship exists for this center, " +
                    "item and warehouse.");
            }
        }
    }

    private static void ValidateCapacityReferences(
        LotSizingSolution solution,
        SupplyChain supplyChain,
        ICollection<SolutionValidationIssue> issues)
    {
        for (int index = 0;
             index <
             solution.WorkCenterCapacityDecisions.Count;
             index++)
        {
            WorkCenterCapacityDecision decision =
                solution.WorkCenterCapacityDecisions[index];

            bool workCenterExists =
                supplyChain.Plants.Any(
                    plant =>
                        plant.Id ==
                            decision.WorkCenter.PlantId &&
                        plant.WorkCenters.Any(
                            workCenter =>
                                workCenter.Id ==
                                decision.WorkCenter
                                    .WorkCenterId));

            if (!workCenterExists)
            {
                AddError(
                    issues,
                    "REF601",
                    $"workCenterCapacityDecisions[{index}]" +
                    ".workCenter",
                    $"Work center " +
                    $"{decision.WorkCenter.PlantId}:" +
                    $"{decision.WorkCenter.WorkCenterId} " +
                    "does not exist.");
            }
        }

        for (int index = 0;
             index <
             solution.WarehouseCapacityDecisions.Count;
             index++)
        {
            WarehouseCapacityDecision decision =
                solution.WarehouseCapacityDecisions[index];

            ValidateWarehouseReference(
                decision.Warehouse,
                supplyChain,
                $"warehouseCapacityDecisions[{index}]" +
                ".warehouse",
                issues);
        }

        for (int index = 0;
             index <
             solution.TransportResourceCapacityDecisions.Count;
             index++)
        {
            TransportResourceCapacityDecision decision =
                solution.TransportResourceCapacityDecisions[
                    index];

            bool resourceExists =
                supplyChain.TransportResources.Any(
                    resource =>
                        resource.Id ==
                        decision.TransportResourceId);

            if (!resourceExists)
            {
                AddError(
                    issues,
                    "REF701",
                    $"transportResourceCapacityDecisions" +
                    $"[{index}].transportResourceId",
                    $"Transport resource " +
                    $"{decision.TransportResourceId} " +
                    "does not exist.");
            }
        }
    }

    private static void ValidateItemReference(
        int itemId,
        SupplyChain supplyChain,
        string path,
        ICollection<SolutionValidationIssue> issues)
    {
        bool itemExists =
            supplyChain.Items.Any(
                item =>
                    item.Id == itemId);

        if (!itemExists)
        {
            AddError(
                issues,
                "REF001",
                path,
                $"Item {itemId} does not exist.");
        }
    }

    private static void ValidateWarehouseReference(
        WarehouseReference warehouse,
        SupplyChain supplyChain,
        string path,
        ICollection<SolutionValidationIssue> issues)
    {
        if (!WarehouseExists(
                warehouse,
                supplyChain))
        {
            AddError(
                issues,
                "REF002",
                path,
                $"Warehouse {warehouse.Kind}:" +
                $"{warehouse.ReferenceId} does not exist.");
        }
    }

    private static bool WarehouseExists(
        WarehouseReference warehouse,
        SupplyChain supplyChain)
    {
        return warehouse.Kind switch
        {
            WarehouseReferenceKind.StandaloneWarehouse =>
                supplyChain.StandaloneWarehouses.Any(
                    current =>
                        current.Id ==
                        warehouse.ReferenceId),

            WarehouseReferenceKind.PlantWarehouse =>
                supplyChain.Plants.Any(
                    plant =>
                        plant.Id ==
                        warehouse.ReferenceId),

            _ => false
        };
    }

    private static void ValidateDecisionHorizon(
        int decisionHorizon,
        int solutionHorizon,
        string path,
        ICollection<SolutionValidationIssue> issues)
    {
        if (decisionHorizon != solutionHorizon)
        {
            AddError(
                issues,
                "DEC001",
                path,
                $"The decision horizon ({decisionHorizon}) " +
                $"differs from the solution horizon " +
                $"({solutionHorizon}).");
        }
    }

    private static bool SameWarehouse(
        WarehouseReference first,
        WarehouseReference second)
    {
        return first.Kind ==
                   second.Kind &&
               first.ReferenceId ==
                   second.ReferenceId;
    }

    private static void ThrowIfErrors(
        IEnumerable<SolutionValidationIssue> issues)
    {
        SolutionValidationIssue[] errors =
            issues
                .Where(
                    issue =>
                        issue.Severity ==
                        SolutionValidationSeverity.Error)
                .ToArray();

        if (errors.Length == 0)
        {
            return;
        }

        string message =
            "The lot-sizing solution is invalid:" +
            Environment.NewLine +
            string.Join(
                Environment.NewLine,
                errors.Select(
                    error =>
                        $"- {error}"));

        throw new InvalidOperationException(
            message);
    }

    private static void AddError(
        ICollection<SolutionValidationIssue> issues,
        string code,
        string path,
        string message)
    {
        issues.Add(
            new SolutionValidationIssue(
                SolutionValidationSeverity.Error,
                code,
                path,
                message));
    }

    private static void AddWarning(
        ICollection<SolutionValidationIssue> issues,
        string code,
        string path,
        string message)
    {
        issues.Add(
            new SolutionValidationIssue(
                SolutionValidationSeverity.Warning,
                code,
                path,
                message));
    }
}