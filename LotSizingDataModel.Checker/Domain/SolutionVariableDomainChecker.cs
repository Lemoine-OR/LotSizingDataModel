using LotSizingDataModel.Checker.Common;
using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Contracts;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;

namespace LotSizingDataModel.Checker.Domain;

/// <summary>
/// Performs generic numerical-domain validation of all decision values
/// stored in a <see cref="LotSizingSolution"/>.
/// </summary>
/// <remarks>
/// <para>
/// This checker does not verify mathematical constraints. It verifies only
/// the intrinsic numerical domain of each solution decision: finite values,
/// non-negativity, binary values, and non-negative integer counters.
/// </para>
/// <para>
/// Small negative floating-point residuals within
/// <see cref="SolutionCheckOptions.ZeroTolerance"/> are accepted as numerical
/// zero. Binary integer series are accepted only when their values are 0 or 1.
/// </para>
/// </remarks>
public sealed class SolutionVariableDomainChecker :
    ISolutionVariableDomainChecker
{
    /// <inheritdoc/>
    public SolutionCheckResult Check(
        LotSizingSolution solution,
        SolutionCheckOptions options)
    {
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(options);

        options.EnsureValid();

        var result =
            new SolutionCheckResult
            {
                Level =
                    SolutionCheckLevel.Feasibility,
                IsStructurallyValid =
                    true,
                AreVariableDomainsValid =
                    true,
                IsFeasible =
                    false,
                IsObjectiveConsistent =
                    false
            };

        try
        {
            CheckProductionDecisions(
                solution,
                options,
                result);

            CheckInventoryDecisions(
                solution,
                options,
                result);

            CheckTransportDecisions(
                solution,
                options,
                result);

            CheckPurchaseDecisions(
                solution,
                options,
                result);

            CheckDistributionDecisions(
                solution,
                options,
                result);

            CheckWorkCenterCapacityDecisions(
                solution,
                options,
                result);

            CheckWarehouseCapacityDecisions(
                solution,
                options,
                result);

            CheckTransportResourceCapacityDecisions(
                solution,
                options,
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
                        "Variable-domain checking failed: " +
                        exception.Message
                });
        }

        result.AreVariableDomainsValid =
            !result.Issues.Any(
                issue =>
                    issue.Severity ==
                        SolutionCheckSeverity.Error);

        return result;
    }

    private static void CheckProductionDecisions(
        LotSizingSolution solution,
        SolutionCheckOptions options,
        SolutionCheckResult result)
    {
        foreach (
            ProductionDecision decision
            in solution.ProductionDecisions)
        {
            string key =
                $"production|routing={decision.RoutingId}";

            CheckNonNegativeFiniteSeries(
                decision.Quantities,
                key,
                "quantities",
                options,
                result);

            CheckBinarySeries(
                decision.Setups,
                key,
                "setups",
                result);

            CheckNonNegativeIntegerSeries(
                decision.LotMultipleCounts,
                key,
                "lotMultipleCounts",
                result);
        }
    }

    private static void CheckInventoryDecisions(
        LotSizingSolution solution,
        SolutionCheckOptions options,
        SolutionCheckResult result)
    {
        foreach (
            InventoryDecision decision
            in solution.InventoryDecisions)
        {
            string key =
                $"inventory|item={decision.ItemId}";

            CheckNonNegativeFiniteSeries(
                decision.Levels,
                key,
                "levels",
                options,
                result);

            CheckNonNegativeFiniteSeries(
                decision.SafetyStockViolations,
                key,
                "safetyStockViolations",
                options,
                result);

            CheckBinarySeries(
                decision.Setups,
                key,
                "setups",
                result);

            CheckNonNegativeFiniteSeries(
                decision.AdditionalCapacityUsed,
                key,
                "additionalCapacityUsed",
                options,
                result);
        }
    }

    private static void CheckTransportDecisions(
        LotSizingSolution solution,
        SolutionCheckOptions options,
        SolutionCheckResult result)
    {
        foreach (
            TransportDecision decision
            in solution.TransportDecisions)
        {
            string key =
                $"transport|item={decision.ItemId}" +
                $"|resource={decision.TransportResourceId}";

            CheckNonNegativeFiniteSeries(
                decision.TransportedQuantities,
                key,
                "transportedQuantities",
                options,
                result);

            CheckBinarySeries(
                decision.Setups,
                key,
                "setups",
                result);

            CheckNonNegativeFiniteSeries(
                decision.AdditionalCapacityUsed,
                key,
                "additionalCapacityUsed",
                options,
                result);
        }
    }

    private static void CheckPurchaseDecisions(
        LotSizingSolution solution,
        SolutionCheckOptions options,
        SolutionCheckResult result)
    {
        foreach (
            PurchaseDecision decision
            in solution.PurchaseDecisions)
        {
            string key =
                $"purchase|supplier={decision.SupplierId}" +
                $"|item={decision.ItemId}";

            CheckNonNegativeFiniteSeries(
                decision.PurchasedQuantities,
                key,
                "purchasedQuantities",
                options,
                result);
        }
    }

    private static void CheckDistributionDecisions(
        LotSizingSolution solution,
        SolutionCheckOptions options,
        SolutionCheckResult result)
    {
        foreach (
            DistributionDecision decision
            in solution.DistributionDecisions)
        {
            string key =
                $"distribution|center={decision.DistributionCenterId}" +
                $"|item={decision.ItemId}";

            CheckNonNegativeFiniteSeries(
                decision.DeliveredQuantities,
                key,
                "deliveredQuantities",
                options,
                result);

            CheckNonNegativeFiniteSeries(
                decision.BacklogLevels,
                key,
                "backlogLevels",
                options,
                result);

            CheckNonNegativeFiniteSeries(
                decision.ShortageQuantities,
                key,
                "shortageQuantities",
                options,
                result);
        }
    }

    private static void CheckWorkCenterCapacityDecisions(
        LotSizingSolution solution,
        SolutionCheckOptions options,
        SolutionCheckResult result)
    {
        foreach (
            WorkCenterCapacityDecision decision
            in solution.WorkCenterCapacityDecisions)
        {
            string key =
                $"workCenterCapacity" +
                $"|plant={decision.WorkCenter.PlantId}" +
                $"|workCenter={decision.WorkCenter.WorkCenterId}";

            CheckBinarySeries(
                decision.Activations,
                key,
                "activations",
                result);

            CheckNonNegativeFiniteSeries(
                decision.AdditionalCapacityUsed,
                key,
                "additionalCapacityUsed",
                options,
                result);
        }
    }

    private static void CheckWarehouseCapacityDecisions(
        LotSizingSolution solution,
        SolutionCheckOptions options,
        SolutionCheckResult result)
    {
        foreach (
            WarehouseCapacityDecision decision
            in solution.WarehouseCapacityDecisions)
        {
            string key =
                "warehouseCapacity";

            CheckBinarySeries(
                decision.Activations,
                key,
                "activations",
                result);

            CheckNonNegativeFiniteSeries(
                decision.AdditionalCapacityUsed,
                key,
                "additionalCapacityUsed",
                options,
                result);
        }
    }

    private static void CheckTransportResourceCapacityDecisions(
        LotSizingSolution solution,
        SolutionCheckOptions options,
        SolutionCheckResult result)
    {
        foreach (
            TransportResourceCapacityDecision decision
            in solution.TransportResourceCapacityDecisions)
        {
            string key =
                $"transportResourceCapacity" +
                $"|resource={decision.TransportResourceId}";

            CheckBinarySeries(
                decision.Activations,
                key,
                "activations",
                result);

            CheckNonNegativeFiniteSeries(
                decision.AdditionalCapacityUsed,
                key,
                "additionalCapacityUsed",
                options,
                result);
        }
    }

    private static void CheckNonNegativeFiniteSeries(
        DoubleTimeSeries series,
        string domainKey,
        string seriesName,
        SolutionCheckOptions options,
        SolutionCheckResult result)
    {
        ArgumentNullException.ThrowIfNull(series);

        for (int period = 1;
             period <= series.PeriodCount;
             period++)
        {
            double value =
                series[period];

            if (!double.IsFinite(value))
            {
                AddDomainError(
                    domainKey,
                    seriesName,
                    period,
                    value,
                    "Value must be finite.",
                    result);

                continue;
            }

            if (value < -options.ZeroTolerance)
            {
                AddDomainError(
                    domainKey,
                    seriesName,
                    period,
                    value,
                    "Value must be non-negative.",
                    result);
            }
        }
    }

    private static void CheckBinarySeries(
        IntegerTimeSeries series,
        string domainKey,
        string seriesName,
        SolutionCheckResult result)
    {
        ArgumentNullException.ThrowIfNull(series);

        for (int period = 1;
             period <= series.PeriodCount;
             period++)
        {
            int value =
                series[period];

            if (value is not 0 and not 1)
            {
                AddDomainError(
                    domainKey,
                    seriesName,
                    period,
                    value,
                    "Binary value must be 0 or 1.",
                    result);
            }
        }
    }

    private static void CheckNonNegativeIntegerSeries(
        IntegerTimeSeries series,
        string domainKey,
        string seriesName,
        SolutionCheckResult result)
    {
        ArgumentNullException.ThrowIfNull(series);

        for (int period = 1;
             period <= series.PeriodCount;
             period++)
        {
            int value =
                series[period];

            if (value < 0)
            {
                AddDomainError(
                    domainKey,
                    seriesName,
                    period,
                    value,
                    "Integer value must be non-negative.",
                    result);
            }
        }
    }

    private static void AddDomainError(
        string domainKey,
        string seriesName,
        int period,
        double actualValue,
        string explanation,
        SolutionCheckResult result)
    {
        result.AddIssue(
            new SolutionCheckIssue
            {
                Severity =
                    SolutionCheckSeverity.Error,

                Kind =
                    SolutionCheckIssueKind.VariableDomain,

                DomainKey =
                    $"{domainKey}|period={period}",

                ActualValue =
                    actualValue,

                Message =
                    $"{seriesName}[{period}] = " +
                    $"{actualValue:G17}. {explanation}"
            });
    }
}
