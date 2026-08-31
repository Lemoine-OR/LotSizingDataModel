using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.ClosedLoop;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Adds explicit return allocation, recovery and disposal
/// decisions to a standard forward mathematical model.
/// </summary>
public sealed class ClosedLoopSupplyNetworkModelDecorator
{
    private const double CoefficientTolerance =
        1.0e-10;

    public MathematicalModel Apply(
        LotSizingInstance instance,
        MathematicalModel sourceModel)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        instance.EnsureClosedLoopValid();

        return Apply(
            instance.ClosedLoopReturnStreams,
            sourceModel);
    }

    public MathematicalModel Apply(
        IReadOnlyList<ClosedLoopReturnStream> streams,
        MathematicalModel sourceModel)
    {
        ArgumentNullException.ThrowIfNull(
            streams);

        ArgumentNullException.ThrowIfNull(
            sourceModel);

        sourceModel.EnsureValid();

        if (streams.Count == 0)
        {
            return sourceModel.Clone();
        }

        foreach (ClosedLoopReturnStream stream
                 in streams)
        {
            ArgumentNullException.ThrowIfNull(
                stream);

            stream.EnsureValid();
        }

        MathematicalModel result =
            sourceModel.Clone();

        int nextVariableId =
            result.Variables.Count == 0
                ? 1
                : result.Variables.Max(
                      variable =>
                          variable.Id) + 1;

        int nextConstraintId =
            result.Constraints.Count == 0
                ? 1
                : result.Constraints.Max(
                      constraint =>
                          constraint.Id) + 1;

        foreach (ClosedLoopReturnStream stream
                 in streams.OrderBy(
                     candidate =>
                         candidate.Id))
        {
            for (int period = 1;
                 period <= stream.PlanningHorizon;
                 period++)
            {
                double returnedQuantity =
                    stream.ReturnQuantity[period];

                var recoveryVariable =
                    new MathematicalVariable(
                        nextVariableId++,
                        $"clr_s{stream.Id}_t{period}",
                        MathematicalVariableType.Continuous,
                        0.0,
                        returnedQuantity)
                    {
                        DomainKey =
                            ClosedLoopVariableKeyFactory
                                .CreateRecoveryKey(
                                    stream.Id,
                                    stream.ItemId,
                                    stream.DistributionCenterId,
                                    period),

                        Description =
                            $"Returned quantity of stream {stream.Id} sent to recovery in period {period}."
                    };

                var disposalVariable =
                    new MathematicalVariable(
                        nextVariableId++,
                        $"cld_s{stream.Id}_t{period}",
                        MathematicalVariableType.Continuous,
                        0.0,
                        returnedQuantity)
                    {
                        DomainKey =
                            ClosedLoopVariableKeyFactory
                                .CreateDisposalKey(
                                    stream.Id,
                                    stream.ItemId,
                                    stream.DistributionCenterId,
                                    period),

                        Description =
                            $"Returned quantity of stream {stream.Id} disposed in period {period}."
                    };

                result.AddVariable(
                    recoveryVariable);

                result.AddVariable(
                    disposalVariable);

                var allocationExpression =
                    new LinearExpression();

                allocationExpression.AddTerm(
                    recoveryVariable.Id,
                    1.0);

                allocationExpression.AddTerm(
                    disposalVariable.Id,
                    1.0);

                result.AddConstraint(
                    new LinearConstraint(
                        nextConstraintId++,
                        $"closedLoopReturnAllocation_s{stream.Id}_t{period}",
                        allocationExpression,
                        MathematicalConstraintSense.Equal,
                        returnedQuantity));

                if (stream.RecoveryCapacity is not null)
                {
                    var capacityExpression =
                        new LinearExpression();

                    capacityExpression.AddTerm(
                        recoveryVariable.Id,
                        1.0);

                    result.AddConstraint(
                        new LinearConstraint(
                            nextConstraintId++,
                            $"closedLoopRecoveryCapacity_s{stream.Id}_t{period}",
                            capacityExpression,
                            MathematicalConstraintSense.LessThanOrEqual,
                            stream.RecoveryCapacity[period]));
                }

                result.Objective.Expression.AddConstant(
                    stream.CollectionUnitCost[period] *
                    returnedQuantity);

                result.Objective.Expression.AddTerm(
                    recoveryVariable.Id,
                    stream.RecoveryUnitCost[period]);

                result.Objective.Expression.AddTerm(
                    disposalVariable.Id,
                    stream.DisposalUnitCost[period]);

                InjectRecoveredInventoryInflow(
                    result,
                    stream,
                    period,
                    recoveryVariable);
            }
        }

        result.Description =
            string.IsNullOrWhiteSpace(
                sourceModel.Description)
                ? "Closed-loop supply-network decorated model."
                : sourceModel.Description.Trim() +
                  " Closed-loop supply-network decorated model.";

        result.EnsureValid();

        return result;
    }

    private static void InjectRecoveredInventoryInflow(
        MathematicalModel model,
        ClosedLoopReturnStream stream,
        int period,
        MathematicalVariable recoveryVariable)
    {
        string inventoryKey =
            StandardFormulationVariableKeyFactory
                .CreateInventoryKey(
                    MathematicalDecisionCategory.Inventory,
                    stream.ItemId,
                    stream.RecoveryWarehouse,
                    period);

        MathematicalVariable[] inventoryVariables =
            model.Variables
                .Where(
                    variable =>
                        string.Equals(
                            variable.DomainKey,
                            inventoryKey,
                            StringComparison.Ordinal))
                .ToArray();

        if (inventoryVariables.Length != 1)
        {
            throw new InvalidOperationException(
                $"Closed-loop stream '{stream.Id}' expected exactly one target inventory variable in period {period}; found {inventoryVariables.Length}.");
        }

        int inventoryVariableId =
            inventoryVariables[0].Id;

        LinearConstraint[] balanceCandidates =
            model.Constraints
                .Where(
                    constraint =>
                        constraint.IsEnabled &&
                        constraint.Sense ==
                            MathematicalConstraintSense.Equal &&
                        constraint.Name.StartsWith(
                            "inventoryBalance_",
                            StringComparison.Ordinal) &&
                        HasUnitCurrentInventoryTerm(
                            constraint,
                            inventoryVariableId))
                .ToArray();

        if (balanceCandidates.Length != 1)
        {
            throw new InvalidOperationException(
                $"Closed-loop stream '{stream.Id}' expected exactly one standard inventory balance in period {period}; found {balanceCandidates.Length}.");
        }

        balanceCandidates[0]
            .LeftHandSide
            .AddTerm(
                recoveryVariable.Id,
                -stream.RecoveryYield);
    }

    private static bool HasUnitCurrentInventoryTerm(
        LinearConstraint constraint,
        int inventoryVariableId)
    {
        double coefficient =
            constraint.LeftHandSide.Terms
                .Where(
                    term =>
                        term.VariableId ==
                        inventoryVariableId)
                .Sum(
                    term =>
                        term.Coefficient);

        return Math.Abs(
                   coefficient -
                   1.0) <=
               CoefficientTolerance;
    }
}
