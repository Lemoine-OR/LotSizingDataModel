using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds global warehouse activation variables.
/// </summary>
public sealed class WarehouseActivationVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the warehouse activation variable-family identifier.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory.WarehouseActivation;

    /// <summary>
    /// Determines whether warehouse activation variables are
    /// enabled.
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
            options.IncludeResourceActivation &&
            StandardFormulationResourceEnumerator
                .EnumerateWarehouses(
                    instance.SupplyChain)
                .Any(
                    entry =>
                        entry.Warehouse.FixedUsageCost is not null);
    }

    /// <summary>
    /// Builds binary warehouse activation variables.
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
            if (entry.Warehouse.FixedUsageCost is null)
            {
                continue;
            }

            for (
                int period = 1;
                period <= instance.PlanningHorizon;
                period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var keyBuilder =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory
                            .WarehouseActivation);

                StandardFormulationDomainKeyFactory.AddWarehouse(
                    keyBuilder,
                    entry.Reference);

                string domainKey =
                    keyBuilder
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                AddBinaryVariable(
                    context,
                    $"YH_w{entry.Reference.ReferenceId}_t{period}",
                    domainKey,
                    $"Activation of warehouse " +
                    $"{entry.Reference.ReferenceId} in period " +
                    $"{period}.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
