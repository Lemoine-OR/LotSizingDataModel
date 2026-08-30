using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Mathematical-only variables identifying the start of a setup state.
/// </summary>
public sealed class SmallBucketSetupStartVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    public override string FamilyId =>
        "smallBucketSetupStart";

    protected override ValueTask BuildFamilyAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (
            var routing
            in instance.SupplyChain.ProductionRoutings)
        {
            for (
                int period = 1;
                period <= instance.PlanningHorizon;
                period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string key =
                    new MathematicalDomainKeyBuilder(
                        MathematicalDecisionCategory
                            .AuxiliarySchedulingSetupStart)
                        .Add(
                            MathematicalDomainKeySegment.Routing,
                            routing.Id)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                AddBinaryVariable(
                    context,
                    $"smallBucketSetupStart_r{routing.Id}_t{period}",
                    key,
                    "Mathematical-only setup-state start flag.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
