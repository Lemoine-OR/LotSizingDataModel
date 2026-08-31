using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Builds standard production start-up event variables.
/// </summary>
/// <remarks>
/// For the standard formulation, a start-up event is the exact 0-to-1
/// transition of the standard routing setup binary. The pre-horizon setup
/// state is not represented by the standard formulation, therefore period 1
/// uses the conventional zero predecessor.
/// </remarks>
public sealed class ProductionStartUpVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    public override string FamilyId =>
        MathematicalDecisionCategory.AuxiliaryProductionStartUp;

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

    protected override ValueTask BuildFamilyAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (ProductionRouting routing
                 in instance.SupplyChain.ProductionRoutings)
        {
            bool hasStartUpCost =
                routing.WorkCenters.Any(
                    reference =>
                        instance.SupplyChain.ProductionCharacteristics.Any(
                            characteristic =>
                                characteristic.ItemId == routing.ItemId &&
                                characteristic.WorkCenter.PlantId == reference.PlantId &&
                                characteristic.WorkCenter.WorkCenterId == reference.WorkCenterId &&
                                characteristic.StartUpCost is not null));

            if (!hasStartUpCost)
            {
                continue;
            }

            for (int period = 1;
                 period <= instance.PlanningHorizon;
                 period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                AddBinaryVariable(
                    context,
                    $"productionStartUp_r{routing.Id}_t{period}",
                    ProductionStartUpDomainKeyFactory.CreateStandardKey(
                        routing.Id,
                        period),
                    "Production start-up occurrence, distinct from setup state.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
