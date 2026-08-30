using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds one operating-expenditure budget constraint per planning period.
/// </summary>
/// <remarks>
/// The constraint reuses the already assembled economic objective expression.
/// Only positive objective coefficients associated with variables whose
/// canonical domain key contains the current period are included.
/// Negative revenue coefficients and non-period-indexed positive terms are
/// intentionally excluded from this periodic operating envelope.
/// </remarks>
public sealed class
    PeriodicOperatingExpenditureBudgetConstraintFamilyBuilder :
        StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId =>
        "periodicOperatingExpenditureBudget";

    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return
            instance.SupplyChain
                .PeriodicOperatingExpenditureBudget
            is not null;
    }

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        var budget =
            instance.SupplyChain
                .PeriodicOperatingExpenditureBudget;

        if (budget is null)
        {
            return ValueTask.CompletedTask;
        }

        MathematicalObjective objective =
            context.Model.Objective;

        if (
            objective.Sense !=
            ObjectiveSense.Minimize)
        {
            throw new InvalidOperationException(
                "Periodic operating expenditure budget requires the " +
                "standard minimized economic objective.");
        }

        for (
            int period = 1;
            period <= instance.PlanningHorizon;
            period++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var expression =
                new LinearExpressionBuilder();

            foreach (
                LinearTerm term
                in objective.Expression.Terms)
            {
                if (
                    term.Coefficient <=
                    options.StructuralZeroTolerance)
                {
                    continue;
                }

                MathematicalVariable variable =
                    context.Model.FindVariableById(
                        term.VariableId) ??
                    throw new InvalidOperationException(
                        $"Objective term references unknown variable " +
                        $"'{term.VariableId}'.");

                if (
                    !MathematicalDomainKey.TryParse(
                        variable.DomainKey,
                        out MathematicalDomainKey? domainKey) ||
                    domainKey is null ||
                    !domainKey.TryGetInt32(
                        MathematicalDomainKeySegment.Period,
                        out int variablePeriod) ||
                    variablePeriod != period)
                {
                    continue;
                }

                expression.Add(
                    variable,
                    term.Coefficient);
            }

            AddConstraint(
                context,
                $"periodicOperatingExpenditureBudget_t{period}",
                expression.Build(),
                MathematicalConstraintSense.LessThanOrEqual,
                budget.GetBudget(period),
                description:
                    "Maximum period-indexed positive economic expenditure.");
        }

        return ValueTask.CompletedTask;
    }
}
