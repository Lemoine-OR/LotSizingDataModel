using LotSizingDataModel.Checker.Common;
using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Contracts;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solution.Validation;

namespace LotSizingDataModel.Checker.Structural;

/// <summary>
/// Performs solver-independent structural validation of a
/// <see cref="LotSizingSolution"/> against a
/// <see cref="LotSizingInstance"/>.
/// </summary>
/// <remarks>
/// <para>
/// Existing validation logic from LotSizingDataModel.Solution is reused
/// instead of being duplicated.
/// </para>
/// <para>
/// In addition, this checker creates the expected zero-valued solution
/// structure through <see cref="LotSizingSolution.CreateFor"/> and compares
/// its decision keys with the candidate solution. This detects missing
/// decisions as well as unexpected decisions.
/// </para>
/// <para>
/// Numerical-domain errors are intentionally left for the domain checker
/// introduced in the next package.
/// </para>
/// </remarks>
public sealed class SolutionStructuralChecker :
    ISolutionStructuralChecker
{
    /// <inheritdoc/>
    public SolutionCheckResult Check(
        LotSizingInstance instance,
        LotSizingSolution solution,
        SolutionCheckOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(options);

        options.EnsureValid();

        var result =
            new SolutionCheckResult
            {
                Level =
                    SolutionCheckLevel.Structural,
                IsStructurallyValid =
                    true,
                AreVariableDomainsValid =
                    false,
                IsFeasible =
                    false,
                IsObjectiveConsistent =
                    false
            };

        try
        {
            AddFilteredExistingValidationIssues(
                instance,
                solution,
                options,
                result);

            CompareExpectedDecisionStructure(
                instance,
                solution,
                result);
        }
        catch (Exception exception)
        {
            result.AddIssue(
                new SolutionCheckIssue
                {
                    Severity =
                        SolutionCheckSeverity.Error,
                    Kind =
                        SolutionCheckIssueKind.CheckFailure,
                    Message =
                        "Structural solution checking failed: " +
                        exception.Message
                });
        }

        result.IsStructurallyValid =
            !result.Issues.Any(
                issue =>
                    issue.Severity ==
                        SolutionCheckSeverity.Error);

        return result;
    }

    private static void AddFilteredExistingValidationIssues(
        LotSizingInstance instance,
        LotSizingSolution solution,
        SolutionCheckOptions options,
        SolutionCheckResult result)
    {
        var validator =
            new LotSizingSolutionValidator(
                options.ZeroTolerance);

        IReadOnlyList<SolutionValidationIssue> sourceIssues =
            validator.Validate(
                solution,
                instance.SupplyChain);

        foreach (
            SolutionValidationIssue sourceIssue
            in sourceIssues)
        {
            if (!IsStructuralIssue(
                    sourceIssue.Code))
            {
                continue;
            }

            result.AddIssue(
                new SolutionCheckIssue
                {
                    Severity =
                        sourceIssue.Severity ==
                            SolutionValidationSeverity.Error
                            ? SolutionCheckSeverity.Error
                            : SolutionCheckSeverity.Warning,

                    Kind =
                        SolutionCheckIssueKind.Structural,

                    DomainKey =
                        string.IsNullOrWhiteSpace(
                            sourceIssue.Path)
                            ? null
                            : sourceIssue.Path,

                    Message =
                        $"{sourceIssue.Code}: " +
                        sourceIssue.Message
                });
        }
    }

    private static bool IsStructuralIssue(
        string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        /*
         * SOL001-SOL005 describe solution identity, horizon,
         * presence of decisions and declared completeness.
         * SOL006-SOL007 concern evaluation semantics and are
         * deliberately not treated as structural issues here.
         */
        if (code is
            "SOL001" or
            "SOL002" or
            "SOL003" or
            "SOL004" or
            "SOL005" or
            "INS001" or
            "DEC001")
        {
            return true;
        }

        /*
         * Duplicate-decision codes end in 02.
         * Internal numerical validity codes ending in 01
         * are handled by the dedicated domain checker.
         */
        if (code.StartsWith(
                "DEC",
                StringComparison.Ordinal) &&
            code.EndsWith(
                "02",
                StringComparison.Ordinal))
        {
            return true;
        }

        /*
         * REFxxx codes verify that item, routing, warehouse,
         * supplier, distribution-center, work-center and
         * transport-resource references exist in the instance.
         */
        return code.StartsWith(
            "REF",
            StringComparison.Ordinal);
    }

    private static void CompareExpectedDecisionStructure(
        LotSizingInstance instance,
        LotSizingSolution solution,
        SolutionCheckResult result)
    {
        LotSizingSolution expected =
            LotSizingSolution.CreateFor(
                instance.SupplyChain,
                instanceIdentifier:
                    solution.InstanceIdentifier,
                name:
                    "Expected checker structure",
                instanceFingerprint:
                    solution.InstanceFingerprint,
                validateSupplyChain:
                    true);

        CompareKeySets(
            "production",
            expected.ProductionDecisions.Select(
                GetProductionKey),
            solution.ProductionDecisions.Select(
                GetProductionKey),
            result);

        CompareKeySets(
            "inventory",
            expected.InventoryDecisions.Select(
                GetInventoryKey),
            solution.InventoryDecisions.Select(
                GetInventoryKey),
            result);

        CompareKeySets(
            "transport",
            expected.TransportDecisions.Select(
                GetTransportKey),
            solution.TransportDecisions.Select(
                GetTransportKey),
            result);

        CompareKeySets(
            "purchase",
            expected.PurchaseDecisions.Select(
                GetPurchaseKey),
            solution.PurchaseDecisions.Select(
                GetPurchaseKey),
            result);

        CompareKeySets(
            "distribution",
            expected.DistributionDecisions.Select(
                GetDistributionKey),
            solution.DistributionDecisions.Select(
                GetDistributionKey),
            result);

        /*
         * Resource-capacity decision families are deliberately not
         * compared against LotSizingSolution.CreateFor here.
         *
         * CreateFor materializes one capacity-decision container for
         * each physical work center / warehouse / transport resource,
         * even when the corresponding capacity extension is not part
         * of the mathematical problem. Such containers are therefore
         * optional at the structural level.
         *
         * When a generated formulation actually contains a work-center,
         * warehouse or transport-resource capacity variable, the
         * MathematicalSolutionValueProjector is the authoritative layer:
         * it requires the matching decision and reports a
         * MissingVariableValue issue when it is absent. This keeps
         * structural checking aligned with the variables that really
         * exist in the selected formulation instead of with the mere
         * existence of a physical resource.
         */
    }

    private static void CompareKeySets(
        string familyName,
        IEnumerable<string> expectedKeys,
        IEnumerable<string> actualKeys,
        SolutionCheckResult result)
    {
        HashSet<string> expected =
            expectedKeys.ToHashSet(
                StringComparer.Ordinal);

        HashSet<string> actual =
            actualKeys.ToHashSet(
                StringComparer.Ordinal);

        foreach (
            string missingKey
            in expected
                .Except(
                    actual,
                    StringComparer.Ordinal)
                .OrderBy(
                    key => key,
                    StringComparer.Ordinal))
        {
            result.AddIssue(
                new SolutionCheckIssue
                {
                    Severity =
                        SolutionCheckSeverity.Error,

                    Kind =
                        SolutionCheckIssueKind.Structural,

                    DomainKey =
                        missingKey,

                    Message =
                        $"Missing {familyName} decision " +
                        $"'{missingKey}'."
                });
        }

        foreach (
            string unexpectedKey
            in actual
                .Except(
                    expected,
                    StringComparer.Ordinal)
                .OrderBy(
                    key => key,
                    StringComparer.Ordinal))
        {
            result.AddIssue(
                new SolutionCheckIssue
                {
                    Severity =
                        SolutionCheckSeverity.Error,

                    Kind =
                        SolutionCheckIssueKind.Structural,

                    DomainKey =
                        unexpectedKey,

                    Message =
                        $"Unexpected {familyName} decision " +
                        $"'{unexpectedKey}'."
                });
        }
    }

    private static string GetProductionKey(
        ProductionDecision decision)
    {
        return
            $"production|routing={decision.RoutingId}";
    }

    private static string GetInventoryKey(
        InventoryDecision decision)
    {
        return
            $"inventory|item={decision.ItemId}" +
            $"|warehouse={GetWarehouseKey(decision.Warehouse)}";
    }

    private static string GetTransportKey(
        TransportDecision decision)
    {
        return
            $"transport|item={decision.ItemId}" +
            $"|resource={decision.TransportResourceId}" +
            $"|origin={GetWarehouseKey(decision.Origin)}" +
            $"|destination={GetWarehouseKey(decision.Destination)}";
    }

    private static string GetPurchaseKey(
        PurchaseDecision decision)
    {
        return
            $"purchase|supplier={decision.SupplierId}" +
            $"|item={decision.ItemId}" +
            $"|destination=" +
            $"{GetWarehouseKey(decision.DestinationWarehouse)}";
    }

    private static string GetDistributionKey(
        DistributionDecision decision)
    {
        return
            $"distribution|center={decision.DistributionCenterId}" +
            $"|item={decision.ItemId}" +
            $"|warehouse={GetWarehouseKey(decision.Warehouse)}";
    }

    private static string GetWarehouseKey(
        LotSizingDataModel.Core.PhysicalModel.WarehouseReference warehouse)
    {
        return
            $"{warehouse.Kind}:{warehouse.ReferenceId}";
    }
}
