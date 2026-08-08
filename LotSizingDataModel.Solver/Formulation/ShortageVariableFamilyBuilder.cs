using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds lost-sales or shortage variables for sourcing
/// relationships that explicitly define shortage constraints.
/// </summary>
public sealed class ShortageVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the shortage variable-family identifier.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory.Shortage;

    /// <summary>
    /// Determines whether shortage variables are enabled.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        ArgumentNullException.ThrowIfNull(
            options);

        if (!options.IncludeShortage)
        {
            return false;
        }

        foreach (
            DistributionCenterSourcing sourcing
            in instance.SupplyChain.DistributionCenterSourcings)
        {
            if (sourcing.ShortageConstraint is not null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Builds shortage variables.
    /// </summary>
    protected override ValueTask BuildFamilyAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        int planningHorizon =
            instance.PlanningHorizon;

        foreach (
            DistributionCenterSourcing sourcing
            in instance.SupplyChain.DistributionCenterSourcings)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (sourcing.ShortageConstraint is null)
            {
                continue;
            }

            for (
                int period = 1;
                period <= planningHorizon;
                period++)
            {
                double upperBound =
                    sourcing.ShortageConstraint[period];

                if (IsStructurallyZero(
                        upperBound,
                        options))
                {
                    continue;
                }

                var keyBuilder =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory.Shortage)
                        .Add(
                            MathematicalDomainKeySegment.DistributionCenter,
                            sourcing.DistributionCenterId)
                        .Add(
                            MathematicalDomainKeySegment.Item,
                            sourcing.ItemId);

                StandardFormulationDomainKeyFactory.AddWarehouse(
                    keyBuilder,
                    sourcing.Warehouse);

                string domainKey =
                    keyBuilder
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                AddNonNegativeContinuousVariable(
                    context,
                    $"S_c{sourcing.DistributionCenterId}" +
                    $"_i{sourcing.ItemId}_w" +
                    $"{sourcing.Warehouse.ReferenceId}_t{period}",
                    domainKey,
                    upperBound,
                    $"Shortage quantity for distribution center " +
                    $"{sourcing.DistributionCenterId}, item " +
                    $"{sourcing.ItemId}, period {period}.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
