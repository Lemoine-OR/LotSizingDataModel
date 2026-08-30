using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Binary routing setup-state variables for executable small-bucket models.
/// </summary>
public sealed class SmallBucketSetupStateVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    public override string FamilyId =>
        "smallBucketSetupState";

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
                        MathematicalDecisionCategory.Setup)
                        .Add(
                            MathematicalDomainKeySegment.Routing,
                            routing.Id)
                        .Add(
                            MathematicalDomainKeySegment.Period,
                            period)
                        .Build();

                AddBinaryVariable(
                    context,
                    $"smallBucketSetupState_r{routing.Id}_t{period}",
                    key,
                    "Persistent small-bucket setup state.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
