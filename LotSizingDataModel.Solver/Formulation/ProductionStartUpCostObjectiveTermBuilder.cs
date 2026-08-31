using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Adds standard production start-up costs.
/// </summary>
public sealed class ProductionStartUpCostObjectiveTermBuilder :
    StandardLotSizingObjectiveTermBuilderBase
{
    public override string TermFamilyId =>
        "productionStartUpCost";

    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return
            options.IncludeProductionSetups &&
            instance.SupplyChain.ProductionCharacteristics.Any(
                characteristic =>
                    characteristic.StartUpCost is not null);
    }

    protected override ValueTask BuildTermsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        LinearExpressionBuilder expressionBuilder,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (ProductionRouting routing
                 in instance.SupplyChain.ProductionRoutings)
        {
            for (int period = 1;
                 period <= instance.PlanningHorizon;
                 period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                double cost =
                    routing.WorkCenters.Sum(
                        reference =>
                            instance.SupplyChain.ProductionCharacteristics
                                .Where(
                                    characteristic =>
                                        characteristic.ItemId == routing.ItemId &&
                                        characteristic.WorkCenter.PlantId == reference.PlantId &&
                                        characteristic.WorkCenter.WorkCenterId == reference.WorkCenterId)
                                .Select(
                                    characteristic =>
                                        characteristic.StartUpCost?[period] ?? 0.0)
                                .SingleOrDefault());

                if (cost == 0.0)
                {
                    continue;
                }

                AddCostTerm(
                    context,
                    expressionBuilder,
                    ProductionStartUpDomainKeyFactory.CreateStandardKey(
                        routing.Id,
                        period),
                    cost);
            }
        }

        return ValueTask.CompletedTask;
    }
}
