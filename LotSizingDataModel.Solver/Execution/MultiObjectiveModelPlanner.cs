using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Resolves executable objective criteria and constructs weighted/lexicographic
/// mathematical models.
/// </summary>
public static class MultiObjectiveModelPlanner
{
    public static IReadOnlyList<ExecutableObjectiveCriterion>
        ResolveCriteria(
            LotSizingInstance instance,
            FinancialExecutionModelContext context)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(context);

        OptimizationObjectivePolicy? policy =
            instance.SupplyChain.ObjectivePolicy;

        if (policy is null)
        {
            return
            [
                CreateCriterion(
                    OptimizationObjectiveKind.Economic,
                    1.0,
                    0,
                    0.0,
                    context)
            ];
        }

        if (policy.AggregationMode ==
            ObjectiveAggregationMode.Single)
        {
            return
            [
                CreateCriterion(
                    policy.PrimaryObjectiveKind,
                    1.0,
                    0,
                    0.0,
                    context)
            ];
        }

        if (policy.AggregationMode is not
                ObjectiveAggregationMode.WeightedSum and not
                ObjectiveAggregationMode.Lexicographic)
        {
            throw new NotSupportedException(
                $"Objective aggregation mode '{policy.AggregationMode}' is not executable.");
        }

        if (!policy.HasExecutableCriterionSpecifications)
        {
            throw new InvalidOperationException(
                "WeightedSum/Lexicographic execution requires explicit ExecutionCriteria.");
        }

        if (!policy.HasUniqueExecutableCriterionKinds)
        {
            throw new InvalidOperationException(
                "Executable objective criterion kinds must be unique.");
        }

        if (policy.AggregationMode ==
                ObjectiveAggregationMode.Lexicographic &&
            !policy.HasUniqueLexicographicPriorities)
        {
            throw new InvalidOperationException(
                "Lexicographic objective priorities must be unique.");
        }

        IEnumerable<ObjectiveCriterionExecutionSpecification>
            specifications =
                policy.ExecutionCriteria;

        if (policy.AggregationMode ==
            ObjectiveAggregationMode.Lexicographic)
        {
            specifications =
                specifications
                    .OrderBy(
                        specification =>
                            specification.Priority);
        }

        return specifications
            .Select(
                specification =>
                    CreateCriterion(
                        specification.Kind,
                        specification.Weight,
                        specification.Priority,
                        specification.AbsoluteTolerance,
                        context))
            .ToArray();
    }

    public static MathematicalModel CreateWeightedSumModel(
        FinancialExecutionModelContext context,
        IReadOnlyList<ExecutableObjectiveCriterion> criteria)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(criteria);

        if (criteria.Count == 0)
        {
            throw new InvalidOperationException(
                "WeightedSum requires at least one criterion.");
        }

        MathematicalModel model =
            context.Model.Clone();

        var weighted =
            new LinearExpression();

        foreach (ExecutableObjectiveCriterion criterion in criteria)
        {
            LinearExpression term =
                criterion.Expression.Clone();

            term.MultiplyBy(
                criterion.Weight);

            weighted.Add(term);
        }

        model.Objective =
            new MathematicalObjective(
                "weightedSum",
                ObjectiveSense.Minimize,
                weighted)
            {
                Description =
                    "Explicit weighted sum of executable objective criteria."
            };

        model.EnsureValid();
        return model;
    }

    public static MathematicalModel CreateLexicographicStageModel(
        FinancialExecutionModelContext context,
        ExecutableObjectiveCriterion criterion,
        IEnumerable<(ExecutableObjectiveCriterion Criterion, double Value)>
            preserved)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(criterion);
        ArgumentNullException.ThrowIfNull(preserved);

        MathematicalModel model =
            context.Model.Clone();

        int nextConstraintId =
            model.Constraints.Count == 0
                ? 1
                : model.Constraints.Max(constraint => constraint.Id) + 1;

        foreach (var previous in preserved)
        {
            LinearExpression expression =
                previous.Criterion.Expression.Clone();

            double rhs =
                previous.Value +
                previous.Criterion.AbsoluteTolerance -
                expression.Constant;

            expression.Constant = 0.0;

            model.AddConstraint(
                new LinearConstraint(
                    nextConstraintId++,
                    $"lexicographicPreserve_{previous.Criterion.Kind}",
                    expression,
                    MathematicalConstraintSense.LessThanOrEqual,
                    rhs)
                {
                    Description =
                        "Preserves an already optimized lexicographic criterion."
                });
        }

        model.Objective =
            new MathematicalObjective(
                $"lexicographic_{criterion.Kind}",
                ObjectiveSense.Minimize,
                criterion.Expression.Clone())
            {
                Description =
                    "One exact lexicographic optimization stage."
            };

        model.EnsureValid();
        return model;
    }

    private static ExecutableObjectiveCriterion CreateCriterion(
        OptimizationObjectiveKind kind,
        double weight,
        int priority,
        double absoluteTolerance,
        FinancialExecutionModelContext context)
    {
        LinearExpression expression =
            kind switch
            {
                OptimizationObjectiveKind.Economic =>
                    context.EconomicCriterion.Clone(),

                OptimizationObjectiveKind.Financial
                    when context.FinancialCriterion is not null =>
                    context.FinancialCriterion.Clone(),

                OptimizationObjectiveKind.Financial =>
                    throw new NotSupportedException(
                        "Financial objective requires CashFlowPolicy."),

                OptimizationObjectiveKind.Sustainability =>
                    throw new NotSupportedException(
                        "Sustainability objective remains unsupported until real sustainability data and criterion semantics exist."),

                _ =>
                    throw new NotSupportedException(
                        $"Objective kind '{kind}' is not executable.")
            };

        return new ExecutableObjectiveCriterion
        {
            Kind = kind,
            Expression = expression,
            Weight = weight,
            Priority = priority,
            AbsoluteTolerance = absoluteTolerance
        };
    }
}
