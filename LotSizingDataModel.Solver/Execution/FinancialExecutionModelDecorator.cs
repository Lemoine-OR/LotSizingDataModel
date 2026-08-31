using System.Reflection;
using LotSizingDataModel.Core.DecisionModel.Finance;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Adds complete-period OPEX coverage and optional cash-flow constraints to a
/// built economic mathematical model.
/// </summary>
public static class FinancialExecutionModelDecorator
{
    public static FinancialExecutionModelContext Decorate(
        LotSizingInstance instance,
        MathematicalModel source)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(source);

        MathematicalModel model =
            source.Clone();

        LinearExpression economic =
            NormalizeToMinimization(
                source.Objective);

        AddPeriodicOperatingExpenditureCoverage(
            instance,
            model,
            economic);

        CashFlowPolicy? cashPolicy =
            instance.SupplyChain.CashFlowPolicy;

        if (cashPolicy is null)
        {
            return new FinancialExecutionModelContext
            {
                Model = model,
                EconomicCriterion = economic,
                FinancialCriterion = null,
                FinancialHorizon = 0
            };
        }

        if (!cashPolicy.HasConsistentPlanningHorizon ||
            cashPolicy.PlanningHorizon != instance.PlanningHorizon)
        {
            throw new InvalidOperationException(
                "CashFlowPolicy must use the instance planning horizon.");
        }

        if (Math.Abs(economic.Constant) > 1.0e-12)
        {
            throw new InvalidOperationException(
                "Cash-flow execution requires a zero constant in the economic objective because an unperiodized constant has no unambiguous cash-flow timing.");
        }

        int financialHorizon =
            instance.PlanningHorizon +
            Math.Max(
                cashPolicy.ReceiptDelayPeriods,
                cashPolicy.DisbursementDelayPeriods);

        var cashVariables =
            AddCashBalanceVariables(
                model,
                cashPolicy,
                financialHorizon);

        AddCashBalanceConstraints(
            model,
            economic,
            cashPolicy,
            instance.PlanningHorizon,
            financialHorizon,
            cashVariables);

        var financial =
            new LinearExpression();

        financial.AddTerm(
            cashVariables[financialHorizon].Id,
            -1.0);

        model.EnsureValid();

        return new FinancialExecutionModelContext
        {
            Model = model,
            EconomicCriterion = economic,
            FinancialCriterion = financial,
            FinancialHorizon = financialHorizon
        };
    }

    private static LinearExpression NormalizeToMinimization(
        MathematicalObjective objective)
    {
        ArgumentNullException.ThrowIfNull(objective);

        LinearExpression expression =
            objective.Expression.Clone();

        if (objective.Sense == ObjectiveSense.Maximize)
        {
            expression.MultiplyBy(-1.0);
        }
        else if (objective.Sense != ObjectiveSense.Minimize)
        {
            throw new InvalidOperationException(
                "Economic objective sense must be Minimize or Maximize.");
        }

        return expression;
    }

    private static Dictionary<int, MathematicalVariable>
        AddCashBalanceVariables(
            MathematicalModel model,
            CashFlowPolicy policy,
            int financialHorizon)
    {
        int nextId =
            model.Variables.Count == 0
                ? 1
                : model.Variables.Max(variable => variable.Id) + 1;

        var result =
            new Dictionary<int, MathematicalVariable>();

        for (int period = 1;
             period <= financialHorizon;
             period++)
        {
            double lowerBound =
                policy.EnforceMinimumCashBalance &&
                period <= policy.PlanningHorizon
                    ? policy.MinimumCashBalance[period]
                    : double.NegativeInfinity;

            var variable =
                new MathematicalVariable
                {
                    Id = nextId++,
                    Name = $"cashBalance_t{period}",
                    DomainKey =
                        new MathematicalDomainKeyBuilder(
                            MathematicalDecisionCategory.CashBalance)
                            .Add(
                                MathematicalDomainKeySegment.Period,
                                period)
                            .Build(),
                    VariableType =
                        MathematicalVariableType.Continuous,
                    LowerBound = lowerBound,
                    UpperBound = double.PositiveInfinity,
                    Description =
                        "Cash balance after all receipts/disbursements due in the period."
                };

            model.AddVariable(variable);
            result.Add(period, variable);
        }

        return result;
    }

    private static void AddCashBalanceConstraints(
        MathematicalModel model,
        LinearExpression economic,
        CashFlowPolicy policy,
        int planningHorizon,
        int financialHorizon,
        IReadOnlyDictionary<int, MathematicalVariable> cashVariables)
    {
        int nextConstraintId =
            model.Constraints.Count == 0
                ? 1
                : model.Constraints.Max(constraint => constraint.Id) + 1;

        var dueTerms =
            new Dictionary<int, List<LinearTerm>>();

        for (int period = 1;
             period <= financialHorizon;
             period++)
        {
            dueTerms[period] = new List<LinearTerm>();
        }

        foreach (LinearTerm term in economic.Terms)
        {
            MathematicalVariable variable =
                model.FindVariableById(term.VariableId)
                ?? throw new InvalidOperationException(
                    $"Unknown economic variable id {term.VariableId}.");

            MathematicalDomainKey key =
                MathematicalDomainKey.Parse(
                    variable.DomainKey);

            if (!key.TryGetInt32(
                    MathematicalDomainKeySegment.Period,
                    out int economicPeriod))
            {
                if (key.Category ==
                    MathematicalDecisionCategory.InitialInventory)
                {
                    economicPeriod = 1;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cash-flow timing is ambiguous for economic variable '{variable.Name}' because its domain key has no period.");
                }
            }

            int delay =
                term.Coefficient < 0.0
                    ? policy.ReceiptDelayPeriods
                    : policy.DisbursementDelayPeriods;

            int duePeriod =
                economicPeriod + delay;

            if (duePeriod < 1 ||
                duePeriod > financialHorizon)
            {
                throw new InvalidOperationException(
                    $"Cash-flow due period {duePeriod} is outside the financial horizon.");
            }

            dueTerms[duePeriod].Add(
                new LinearTerm(
                    term.VariableId,
                    term.Coefficient));
        }

        for (int period = 1;
             period <= financialHorizon;
             period++)
        {
            var expression =
                new LinearExpression();

            expression.AddTerm(
                cashVariables[period].Id,
                1.0);

            if (period > 1)
            {
                expression.AddTerm(
                    cashVariables[period - 1].Id,
                    -1.0);
            }

            foreach (LinearTerm term in dueTerms[period])
            {
                expression.AddTerm(term);
            }

            double fixedNetCashFlow =
                period <= planningHorizon
                    ? policy.FixedNetCashFlow[period]
                    : 0.0;

            double rhs =
                fixedNetCashFlow +
                (period == 1
                    ? policy.InitialCashBalance
                    : 0.0);

            model.AddConstraint(
                new LinearConstraint(
                    nextConstraintId++,
                    $"cashBalance_t{period}",
                    expression,
                    MathematicalConstraintSense.Equal,
                    rhs)
                {
                    DomainKey =
                        $"cashBalance|period={period}",
                    Description =
                        "Cash-flow balance distinct from OPEX budget."
                });
        }
    }

    private static void AddPeriodicOperatingExpenditureCoverage(
        LotSizingInstance instance,
        MathematicalModel model,
        LinearExpression economic)
    {
        object? budget =
            instance.SupplyChain
                .PeriodicOperatingExpenditureBudget;

        if (budget is null)
        {
            return;
        }

        int nextConstraintId =
            model.Constraints.Count == 0
                ? 1
                : model.Constraints.Max(constraint => constraint.Id) + 1;

        for (int period = 1;
             period <= instance.PlanningHorizon;
             period++)
        {
            var expression =
                new LinearExpression();

            foreach (LinearTerm term in economic.Terms)
            {
                if (term.Coefficient <= 0.0)
                {
                    continue;
                }

                MathematicalVariable variable =
                    model.FindVariableById(term.VariableId)
                    ?? throw new InvalidOperationException(
                        $"Unknown economic variable id {term.VariableId}.");

                MathematicalDomainKey key =
                    MathematicalDomainKey.Parse(
                        variable.DomainKey);

                if (!key.TryGetInt32(
                        MathematicalDomainKeySegment.Period,
                        out int termPeriod))
                {
                    if (key.Category ==
                        MathematicalDecisionCategory.InitialInventory)
                    {
                        continue;
                    }

                    throw new InvalidOperationException(
                        $"Periodic OPEX coverage cannot classify positive economic term '{variable.Name}' because it has no period.");
                }

                if (termPeriod == period)
                {
                    expression.AddTerm(term);
                }
            }

            model.AddConstraint(
                new LinearConstraint(
                    nextConstraintId++,
                    $"alpha33PeriodicOpexBudget_t{period}",
                    expression,
                    MathematicalConstraintSense.LessThanOrEqual,
                    ReadPeriodValue(
                        budget,
                        period))
                {
                    DomainKey =
                        $"periodicOperatingExpenditureBudget|period={period}",
                    Description =
                        "Complete positive economic-cost envelope, including scheduling setup/start-up/changeover costs."
                });
        }
    }

    private static double ReadPeriodValue(
        object parameter,
        int period)
    {
        Type type =
            parameter.GetType();

        MethodInfo? getter =
            type.GetMethod(
                "GetValue",
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                types: new[] { typeof(int) },
                modifiers: null);

        if (getter is not null)
        {
            object? value =
                getter.Invoke(
                    parameter,
                    new object[] { period });

            if (value is double number)
            {
                return number;
            }
        }

        PropertyInfo? indexer =
            type.GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public)
                .SingleOrDefault(
                    property =>
                    {
                        ParameterInfo[] indexes =
                            property.GetIndexParameters();

                        return indexes.Length == 1 &&
                               indexes[0].ParameterType ==
                                   typeof(int) &&
                               property.PropertyType ==
                                   typeof(double);
                    });

        if (indexer is not null)
        {
            object? value =
                indexer.GetValue(
                    parameter,
                    new object[] { period });

            if (value is double number)
            {
                return number;
            }
        }

        throw new InvalidOperationException(
            $"Cannot read period {period} from {type.FullName}.");
    }
}
