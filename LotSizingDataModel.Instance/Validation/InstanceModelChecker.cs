using LotSizingDataModel.Core.Validation;

namespace LotSizingDataModel.Instance.Validation;

/// <summary>
/// Checks whether a <see cref="LotSizingInstance"/> represents
/// a structurally valid and semantically meaningful lot-sizing
/// model.
/// </summary>
/// <remarks>
/// Structural and referential validation remains owned by
/// <see cref="SupplyChainValidator"/>. This checker composes those
/// results and adds instance-level semantic diagnostics.
///
/// This service does not solve the mathematical model and does not
/// determine exact feasibility. Those concerns belong to a separate
/// feasibility analyzer.
/// </remarks>
public sealed class InstanceModelChecker
{
    private readonly SupplyChainValidator _structuralValidator;

    /// <summary>
    /// Initializes a checker using the canonical Core structural
    /// validator.
    /// </summary>
    public InstanceModelChecker()
        : this(new SupplyChainValidator())
    {
    }

    /// <summary>
    /// Initializes a checker with an explicit structural validator.
    /// </summary>
    public InstanceModelChecker(
        SupplyChainValidator structuralValidator)
    {
        _structuralValidator =
            structuralValidator ??
            throw new ArgumentNullException(
                nameof(structuralValidator));
    }

    /// <summary>
    /// Checks one lot-sizing instance.
    /// </summary>
    public InstanceModelCheckResult Check(
        LotSizingInstance instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        var diagnostics =
            new List<InstanceDiagnostic>();

        AppendStructuralDiagnostics(
            instance,
            diagnostics);

        AppendInstanceIdentityDiagnostics(
            instance,
            diagnostics);

        AppendDemandDiagnostics(
            instance,
            diagnostics);

        AppendKnownResultDiagnostics(
            instance,
            diagnostics);

        return new InstanceModelCheckResult(
            diagnostics);
    }

    private void AppendStructuralDiagnostics(
        LotSizingInstance instance,
        ICollection<InstanceDiagnostic> diagnostics)
    {
        IReadOnlyList<
            SupplyChainValidator.ValidationIssue> issues =
                _structuralValidator.Validate(
                    instance.SupplyChain);

        foreach (
            SupplyChainValidator.ValidationIssue issue
            in issues)
        {
            InstanceDiagnosticSeverity severity =
                issue.Severity ==
                SupplyChainValidator.ValidationSeverity.Error
                    ? InstanceDiagnosticSeverity.Error
                    : InstanceDiagnosticSeverity.Warning;

            diagnostics.Add(
                new InstanceDiagnostic(
                    code: issue.Code,
                    severity: severity,
                    path:
                        "instance." +
                        issue.Path,
                    message: issue.Message,
                    suggestedAction:
                        "Correct the underlying supply-chain data " +
                        "before classification or solving."));
        }
    }

    private static void AppendInstanceIdentityDiagnostics(
        LotSizingInstance instance,
        ICollection<InstanceDiagnostic> diagnostics)
    {
        if (instance.HasInstanceId)
        {
            return;
        }

        diagnostics.Add(
            new InstanceDiagnostic(
                code: "LSDM-SEM-001",
                severity:
                    InstanceDiagnosticSeverity.Warning,
                path: "instance.instanceId",
                message:
                    "The instance has no stable identifier.",
                suggestedAction:
                    "Assign a stable instance identifier before " +
                    "publishing or exchanging the instance."));
    }

    private static void AppendDemandDiagnostics(
        LotSizingInstance instance,
        ICollection<InstanceDiagnostic> diagnostics)
    {
        if (instance.SupplyChain.Demands.Count == 0)
        {
            diagnostics.Add(
                new InstanceDiagnostic(
                    code: "LSDM-SEM-010",
                    severity:
                        InstanceDiagnosticSeverity.Warning,
                    path: "instance.supplyChain.demands",
                    message:
                        "The instance contains no external demand record.",
                    suggestedAction:
                        "Confirm that a demand-free planning problem is " +
                        "intentional."));
            return;
        }

        bool hasPositiveDemand = false;

        foreach (var demand in instance.SupplyChain.Demands)
        {
            for (
                int period = 1;
                period <= demand.PlanningHorizon;
                period++)
            {
                if (demand.GetQuantity(period) > 0.0)
                {
                    hasPositiveDemand = true;
                    break;
                }
            }

            if (hasPositiveDemand)
            {
                break;
            }
        }

        if (!hasPositiveDemand)
        {
            diagnostics.Add(
                new InstanceDiagnostic(
                    code: "LSDM-SEM-011",
                    severity:
                        InstanceDiagnosticSeverity.Warning,
                    path: "instance.supplyChain.demands",
                    message:
                        "Demand records exist but all demand quantities " +
                        "are zero.",
                    suggestedAction:
                        "Confirm that the zero-demand instance is " +
                        "intentional."));
        }
    }

    private static void AppendKnownResultDiagnostics(
        LotSizingInstance instance,
        ICollection<InstanceDiagnostic> diagnostics)
    {
        if (!instance.HasBestKnownResultId ||
            instance.HasBestKnownResult)
        {
            return;
        }

        diagnostics.Add(
            new InstanceDiagnostic(
                code: "LSDM-SEM-020",
                severity:
                    InstanceDiagnosticSeverity.Warning,
                path: "instance.bestKnownResultId",
                message:
                    "The selected best-known-result identifier does not " +
                    "refer to an existing known result.",
                values:
                    new Dictionary<string, string>(
                        StringComparer.Ordinal)
                    {
                        ["bestKnownResultId"] =
                            instance.BestKnownResultId
                    },
                suggestedAction:
                    "Select an existing known result or clear the " +
                    "best-known-result identifier."));
    }
}
