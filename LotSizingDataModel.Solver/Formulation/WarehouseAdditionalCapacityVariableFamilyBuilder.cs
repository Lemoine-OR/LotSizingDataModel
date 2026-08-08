using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds global additional warehouse-capacity variables.
/// </summary>
public sealed class WarehouseAdditionalCapacityVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the warehouse additional-capacity variable-family
    /// identifier.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory.WarehouseAdditionalCapacity;

    /// <summary>
    /// Determines whether additional warehouse-capacity
    /// variables are enabled.
    /// </summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        ArgumentNullException.ThrowIfNull(
            options);

        return
            options.IncludeAdditionalCapacity &&
            StandardFormulationResourceEnumerator
                .EnumerateWarehouses(
                    instance.SupplyChain)
                .Any(
                    entry =>
                        entry.Warehouse.AdditionalCapacity is not null);
    }

    /// <summary>
    /// Builds additional warehouse-capacity variables.
    /// </summary>
    protected override ValueTask BuildFamilyAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (
            var entry
            in StandardFormulationResourceEnumerator
                .EnumerateWarehouses(
                    instance.SupplyChain))
        {
            if (entry.Warehouse.AdditionalCapacity is null)
            {
                continue;
            }

            for (
                int period = 1;
                period <= instance.PlanningHorizon;
                period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                double upperBound =
                    entry.Warehouse.AdditionalCapacity[period];

                if (IsStructurallyZero(
                        upperBound,
                        options))
                {
                    continue;
                }

                var keyBuilder =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory
                            .WarehouseAdditionalCapacity);

                StandardFormulationDomainKeyFactory.AddWarehouse(
                    keyBuilder,
                    entry.Reference);

                string domainKey =
                    keyBuilder
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                AddNonNegativeContinuousVariable(
                    context,
                    $"OH_w{entry.Reference.ReferenceId}_t{period}",
                    domainKey,
                    upperBound,
                    $"Additional capacity of warehouse " +
                    $"{entry.Reference.ReferenceId} in period " +
                    $"{period}.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
