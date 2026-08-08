using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds global work-center activation variables.
/// </summary>
public sealed class WorkCenterActivationVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    /// <summary>
    /// Gets the work-center activation variable-family
    /// identifier.
    /// </summary>
    public override string FamilyId =>
        MathematicalDecisionCategory.WorkCenterActivation;

    /// <summary>
    /// Determines whether work-center activation variables are
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
                .EnumerateWorkCenters(
                    instance.SupplyChain)
                .Any(
                    entry =>
                        entry.WorkCenter.FixedUsageCost is not null);
    }

    /// <summary>
    /// Builds binary work-center activation variables.
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
            if (entry.WorkCenter.FixedUsageCost is null)
            {
                continue;
            }

            for (
                int period = 1;
                period <= instance.PlanningHorizon;
                period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string domainKey =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory
                            .WorkCenterActivation)
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

                AddBinaryVariable(
                    context,
                    $"YW_p{entry.PlantId}_w" +
                    $"{entry.WorkCenter.Id}_t{period}",
                    domainKey,
                    $"Activation of work center " +
                    $"{entry.WorkCenter.Id} in plant " +
                    $"{entry.PlantId}, period {period}.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
