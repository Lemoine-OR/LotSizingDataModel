using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Defines standard start-up events as exact 0-to-1 setup transitions.
/// </summary>
public sealed class ProductionStartUpDefinitionConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId =>
        "productionStartUpDefinition";

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

    protected override ValueTask BuildConstraintsAsync(
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

                MathematicalVariable startUp =
                    GetVariable(
                        context,
                        ProductionStartUpDomainKeyFactory.CreateStandardKey(
                            routing.Id,
                            period));

                MathematicalVariable setup =
                    GetVariable(
                        context,
                        SetupKey(
                            routing.Id,
                            period));

                string suffix =
                    $"_r{routing.Id}_t{period}";

                if (period == 1)
                {
                    AddConstraint(
                        context,
                        "productionStartUpInitial" + suffix,
                        new LinearExpressionBuilder()
                            .Add(startUp)
                            .Subtract(setup)
                            .Build(),
                        MathematicalConstraintSense.Equal,
                        0.0);

                    continue;
                }

                MathematicalVariable previous =
                    GetVariable(
                        context,
                        SetupKey(
                            routing.Id,
                            period - 1));

                AddConstraint(
                    context,
                    "productionStartUpLower" + suffix,
                    new LinearExpressionBuilder()
                        .Add(startUp)
                        .Subtract(setup)
                        .Add(previous)
                        .Build(),
                    MathematicalConstraintSense.GreaterThanOrEqual,
                    0.0);

                AddConstraint(
                    context,
                    "productionStartUpUpperCurrent" + suffix,
                    new LinearExpressionBuilder()
                        .Add(startUp)
                        .Subtract(setup)
                        .Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    0.0);

                AddConstraint(
                    context,
                    "productionStartUpUpperPrevious" + suffix,
                    new LinearExpressionBuilder()
                        .Add(startUp)
                        .Add(previous)
                        .Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    1.0);
            }
        }

        return ValueTask.CompletedTask;
    }

    private static string SetupKey(
        int routingId,
        int period) =>
        new MathematicalDomainKeyBuilder(
            MathematicalDecisionCategory.Setup)
            .Add(MathematicalDomainKeySegment.Routing, routingId)
            .Add(MathematicalDomainKeySegment.Period, period)
            .Build();
}
