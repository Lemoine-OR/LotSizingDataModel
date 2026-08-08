using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds global additional work-center capacity variables.
/// </summary>
public sealed class WorkCenterAdditionalCapacityVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the work-center additional-capacity variable-family
    /// identifier.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory.WorkCenterAdditionalCapacity;

    /// <summary>
    /// Determines whether additional work-center capacity
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
                .EnumerateWorkCenters(
                    instance.SupplyChain)
                .Any(
                    entry =>
                        entry.WorkCenter.AdditionalCapacity is not null);
    }

    /// <summary>
    /// Builds additional work-center capacity variables.
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
                .EnumerateWorkCenters(
                    instance.SupplyChain))
        {
            if (entry.WorkCenter.AdditionalCapacity is null)
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
                    entry.WorkCenter.AdditionalCapacity[period];

                if (IsStructurallyZero(
                        upperBound,
                        options))
                {
                    continue;
                }

                string domainKey =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory
                            .WorkCenterAdditionalCapacity)
                        .Add(
                            MathematicalDomainKeySegment.Plant,
                            entry.PlantId)
                        .Add(
                            MathematicalDomainKeySegment.WorkCenter,
                            entry.WorkCenter.Id)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                AddNonNegativeContinuousVariable(
                    context,
                    $"OW_p{entry.PlantId}_w" +
                    $"{entry.WorkCenter.Id}_t{period}",
                    domainKey,
                    upperBound,
                    $"Additional capacity of work center " +
                    $"{entry.WorkCenter.Id} in plant " +
                    $"{entry.PlantId}, period {period}.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
