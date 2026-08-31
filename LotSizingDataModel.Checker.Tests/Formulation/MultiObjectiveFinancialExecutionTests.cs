using System.Collections;
using System.Reflection;
using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Finance;
using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Tests.Formulation;

public sealed class MultiObjectiveFinancialExecutionTests
{
    [Fact]
    public void CashFlowDecorator_CreatesBalanceAndTerminalFinancialCriterion()
    {
        var chain =
            new SupplyChain(1)
            {
                CashFlowPolicy =
                    new CashFlowPolicy
                    {
                        InitialCashBalance = 20.0
                    }
            };

        chain.CashFlowPolicy.ResizeTimeSeries(1);

        var instance =
            new LotSizingInstance(
                chain,
                "cash-flow");

        MathematicalModel model =
            CreateEconomicToyModel();

        FinancialExecutionModelContext context =
            FinancialExecutionModelDecorator.Decorate(
                instance,
                model);

        Assert.NotNull(
            context.FinancialCriterion);

        Assert.Single(
            context.Model.Variables,
            variable =>
                variable.DomainKey.StartsWith(
                    MathematicalDecisionCategory.CashBalance + "|",
                    StringComparison.Ordinal));

        Assert.Contains(
            context.Model.Constraints,
            constraint =>
                constraint.Name == "cashBalance_t1");
    }

    [Fact]
    public void WeightedSum_UsesExplicitCriterionWeights()
    {
        var chain =
            new SupplyChain(1)
            {
                CashFlowPolicy =
                    new CashFlowPolicy
                    {
                        InitialCashBalance = 0.0
                    }
            };

        chain.CashFlowPolicy.ResizeTimeSeries(1);

        chain.ObjectivePolicy =
            CreateValidExecutionPolicy(
                ObjectiveAggregationMode.WeightedSum,
                new ObjectiveCriterionExecutionSpecification
                {
                    Kind =
                        OptimizationObjectiveKind.Economic,
                    Weight = 2.0,
                    Priority = 0
                },
                new ObjectiveCriterionExecutionSpecification
                {
                    Kind =
                        OptimizationObjectiveKind.Financial,
                    Weight = 3.0,
                    Priority = 1
                });

        var instance =
            new LotSizingInstance(
                chain,
                "weighted");

        FinancialExecutionModelContext context =
            FinancialExecutionModelDecorator.Decorate(
                instance,
                CreateEconomicToyModel());

        IReadOnlyList<ExecutableObjectiveCriterion> criteria =
            MultiObjectiveModelPlanner.ResolveCriteria(
                instance,
                context);

        MathematicalModel weighted =
            MultiObjectiveModelPlanner.CreateWeightedSumModel(
                context,
                criteria);

        Assert.Equal(
            "weightedSum",
            weighted.Objective.Name);

        Assert.Equal(
            2,
            criteria.Count);
    }

    [Fact]
    public void LexicographicStage_AddsPreservationConstraintWithTolerance()
    {
        var chain =
            new SupplyChain(1)
            {
                CashFlowPolicy =
                    new CashFlowPolicy()
            };

        chain.CashFlowPolicy.ResizeTimeSeries(1);

        chain.ObjectivePolicy =
            CreateValidExecutionPolicy(
                ObjectiveAggregationMode.Lexicographic,
                new ObjectiveCriterionExecutionSpecification
                {
                    Kind =
                        OptimizationObjectiveKind.Economic,
                    Priority = 0,
                    AbsoluteTolerance = 0.25
                },
                new ObjectiveCriterionExecutionSpecification
                {
                    Kind =
                        OptimizationObjectiveKind.Financial,
                    Priority = 1,
                    AbsoluteTolerance = 0.0
                });

        var instance =
            new LotSizingInstance(
                chain,
                "lexicographic");

        FinancialExecutionModelContext context =
            FinancialExecutionModelDecorator.Decorate(
                instance,
                CreateEconomicToyModel());

        IReadOnlyList<ExecutableObjectiveCriterion> criteria =
            MultiObjectiveModelPlanner.ResolveCriteria(
                instance,
                context);

        MathematicalModel stage2 =
            MultiObjectiveModelPlanner.CreateLexicographicStageModel(
                context,
                criteria[1],
                new[]
                {
                    (criteria[0], 10.0)
                });

        Assert.Contains(
            stage2.Constraints,
            constraint =>
                constraint.Name ==
                "lexicographicPreserve_Economic");
    }

    [Fact]
    public void SustainabilityAndServiceLevel_RemainExplicitlyUnsupported()
    {
        var context =
            new FinancialExecutionModelContext
            {
                Model = CreateEconomicToyModel(),
                EconomicCriterion =
                    CreateEconomicToyModel()
                        .Objective.Expression.Clone(),
                FinancialCriterion = null
            };

        foreach (OptimizationObjectiveKind unsupported in new[]
                 {
                     OptimizationObjectiveKind.Sustainability,
                     OptimizationObjectiveKind.ServiceLevel
                 })
        {
            var chain = new SupplyChain(1);
            chain.ObjectivePolicy =
                CreateValidExecutionPolicy(
                    ObjectiveAggregationMode.WeightedSum,
                    new ObjectiveCriterionExecutionSpecification
                    {
                        Kind =
                            unsupported,
                        Weight = 1.0,
                        Priority = 0
                    },
                    new ObjectiveCriterionExecutionSpecification
                    {
                        Kind =
                            OptimizationObjectiveKind.Economic,
                        Weight = 1.0,
                        Priority = 1
                    });

            var instance =
                new LotSizingInstance(
                    chain,
                    unsupported.ToString());

            Assert.Throws<NotSupportedException>(
                () =>
                    MultiObjectiveModelPlanner.ResolveCriteria(
                        instance,
                        context));
        }
    }

    private static OptimizationObjectivePolicy
        CreateValidExecutionPolicy(
            ObjectiveAggregationMode aggregationMode,
            params ObjectiveCriterionExecutionSpecification[] specifications)
    {
        if (specifications.Length == 0)
        {
            throw new InvalidOperationException(
                "At least one execution criterion is required.");
        }

        if (aggregationMode ==
                ObjectiveAggregationMode.WeightedSum &&
            specifications.Length < 2)
        {
            throw new InvalidOperationException(
                "A generated WeightedSum fixture requires at least two execution criteria before native policy construction.");
        }

        var policy =
            new OptimizationObjectivePolicy
            {
                AggregationMode =
                    aggregationMode
            };

        foreach (ObjectiveCriterionExecutionSpecification specification
                 in specifications)
        {
            policy.ExecutionCriteria.Add(
                specification);
        }

        SynchronizeNativeCriteria(
            policy,
            specifications);

        // This is deliberately executed before the policy is attached to
        // SupplyChain.ObjectivePolicy. The SupplyChain setter validates the
        // child immediately, so the fixture must already be valid here.
        policy.EnsureValid();

        return policy;
    }

    private static void SynchronizeNativeCriteria(
        OptimizationObjectivePolicy policy,
        IReadOnlyList<ObjectiveCriterionExecutionSpecification> specifications)
    {
        Type policyType =
            policy.GetType();

        Type criterionType =
            policyType.Assembly
                .GetTypes()
                .Single(
                    type =>
                        type.Name ==
                        "OptimizationObjectiveCriterion");

        MethodInfo? addMethod =
            policyType.GetMethods(
                    BindingFlags.Instance |
                    BindingFlags.Public)
                .Where(
                    method =>
                        method.GetParameters().Length == 1 &&
                        method.GetParameters()[0].ParameterType ==
                            criterionType)
                .OrderByDescending(
                    method =>
                        method.Name.StartsWith(
                            "Add",
                            StringComparison.Ordinal))
                .FirstOrDefault();

        object? collection = null;
        MethodInfo? collectionAdd = null;

        if (addMethod is null)
        {
            PropertyInfo[] collectionCandidates =
                policyType.GetProperties(
                        BindingFlags.Instance |
                        BindingFlags.Public)
                    .Where(
                        property =>
                        {
                            Type propertyType =
                                property.PropertyType;

                            if (!property.CanRead)
                            {
                                return false;
                            }

                            if (propertyType.IsGenericType &&
                                propertyType.GetGenericArguments().Length == 1 &&
                                propertyType.GetGenericArguments()[0] ==
                                    criterionType)
                            {
                                return true;
                            }

                            return false;
                        })
                    .ToArray();

            if (collectionCandidates.Length != 1)
            {
                throw new InvalidOperationException(
                    "Cannot resolve the unique native OptimizationObjectiveCriterion collection on OptimizationObjectivePolicy.");
            }

            collection =
                collectionCandidates[0].GetValue(
                    policy)
                ?? throw new InvalidOperationException(
                    "The native objective-criterion collection is null.");

            collectionAdd =
                collection.GetType().GetMethod(
                    "Add",
                    new[]
                    {
                        criterionType
                    });

            if (collectionAdd is null)
            {
                throw new InvalidOperationException(
                    "The native objective-criterion collection does not expose Add(OptimizationObjectiveCriterion).");
            }
        }

        foreach (ObjectiveCriterionExecutionSpecification specification
                 in specifications)
        {
            object criterion =
                CreateNativeCriterion(
                    criterionType,
                    specification);

            if (addMethod is not null)
            {
                addMethod.Invoke(
                    policy,
                    new[]
                    {
                        criterion
                    });
            }
            else
            {
                collectionAdd!.Invoke(
                    collection,
                    new[]
                    {
                        criterion
                    });
            }
        }
    }

    private static object CreateNativeCriterion(
        Type criterionType,
        ObjectiveCriterionExecutionSpecification specification)
    {
        object? criterion = null;

        foreach (ConstructorInfo constructor
                 in criterionType.GetConstructors()
                     .OrderBy(
                         candidate =>
                             candidate.GetParameters().Length))
        {
            ParameterInfo[] parameters =
                constructor.GetParameters();

            object?[] arguments =
                new object?[parameters.Length];

            bool supported = true;

            for (int index = 0;
                 index < parameters.Length;
                 index++)
            {
                ParameterInfo parameter =
                    parameters[index];

                if (parameter.ParameterType ==
                    typeof(OptimizationObjectiveKind))
                {
                    arguments[index] =
                        specification.Kind;
                }
                else if (parameter.ParameterType ==
                         typeof(double))
                {
                    arguments[index] =
                        parameter.Name?.Contains(
                            "weight",
                            StringComparison.OrdinalIgnoreCase) == true
                            ? specification.Weight
                            : 0.0;
                }
                else if (parameter.ParameterType ==
                         typeof(int))
                {
                    arguments[index] =
                        specification.Priority;
                }
                else if (parameter.ParameterType ==
                         typeof(bool))
                {
                    arguments[index] =
                        true;
                }
                else if (parameter.HasDefaultValue)
                {
                    arguments[index] =
                        parameter.DefaultValue;
                }
                else
                {
                    supported = false;
                    break;
                }
            }

            if (!supported)
            {
                continue;
            }

            try
            {
                criterion =
                    constructor.Invoke(
                        arguments);

                break;
            }
            catch (TargetInvocationException)
            {
                // Try the next source-supported constructor shape.
            }
        }

        criterion ??=
            Activator.CreateInstance(
                criterionType);

        if (criterion is null)
        {
            throw new InvalidOperationException(
                "Cannot construct OptimizationObjectiveCriterion from the baseline API.");
        }

        SetWritableEnumProperty(
            criterion,
            typeof(OptimizationObjectiveKind),
            specification.Kind);

        SetWritableNamedProperty(
            criterion,
            "Weight",
            specification.Weight);

        SetWritableNamedProperty(
            criterion,
            "Priority",
            specification.Priority);

        foreach (PropertyInfo property
                 in criterionType.GetProperties(
                     BindingFlags.Instance |
                     BindingFlags.Public))
        {
            if (property.CanWrite &&
                property.PropertyType == typeof(bool) &&
                property.Name.Contains(
                    "Enabled",
                    StringComparison.OrdinalIgnoreCase))
            {
                property.SetValue(
                    criterion,
                    true);
            }
        }

        MethodInfo? ensureValid =
            criterionType.GetMethod(
                "EnsureValid",
                BindingFlags.Instance |
                BindingFlags.Public,
                binder: null,
                Type.EmptyTypes,
                modifiers: null);

        ensureValid?.Invoke(
            criterion,
            null);

        return criterion;
    }

    private static void SetWritableEnumProperty(
        object target,
        Type enumType,
        object value)
    {
        PropertyInfo[] candidates =
            target.GetType()
                .GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public)
                .Where(
                    property =>
                        property.CanWrite &&
                        property.PropertyType ==
                            enumType)
                .ToArray();

        if (candidates.Length == 1)
        {
            candidates[0].SetValue(
                target,
                value);
        }
    }

    private static void SetWritableNamedProperty(
        object target,
        string propertyName,
        object value)
    {
        PropertyInfo? property =
            target.GetType()
                .GetProperty(
                    propertyName,
                    BindingFlags.Instance |
                    BindingFlags.Public);

        if (property?.CanWrite == true)
        {
            property.SetValue(
                target,
                value);
        }
    }

    private static MathematicalModel CreateEconomicToyModel()
    {
        var model =
            new MathematicalModel
            {
                Name = "economic-toy"
            };

        model.Variables.Add(
            new MathematicalVariable
            {
                Id = 1,
                Name = "costDecision",
                DomainKey =
                    "production|routing=1|period=1",
                VariableType =
                    MathematicalVariableType.Continuous,
                LowerBound = 0.0,
                UpperBound =
                    double.PositiveInfinity
            });

        var expression =
            new LinearExpression();

        expression.AddTerm(
            1,
            5.0);

        model.Objective =
            new MathematicalObjective(
                "economic",
                ObjectiveSense.Minimize,
                expression);

        model.EnsureValid();
        return model;
    }
}
