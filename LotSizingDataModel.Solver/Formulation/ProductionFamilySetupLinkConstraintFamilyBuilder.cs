using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Enforces item setup activation less than or equal to family setup
/// activation for every routing of a member item on the family work center.
/// </summary>
public sealed class ProductionFamilySetupLinkConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId =>
        "productionFamilySetupLink";

    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return instance.SupplyChain.ProductionSetupFamilies.Count > 0;
    }

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (ProductionSetupFamily family
                 in instance.SupplyChain.ProductionSetupFamilies)
        {
            foreach (int itemId in
                     family.MemberItemIds.Distinct())
            {
                ProductionRouting[] routings =
                    instance.SupplyChain.ProductionRoutings
                        .Where(routing =>
                            routing.ItemId == itemId &&
                            routing.WorkCenters.Any(reference =>
                                reference.PlantId ==
                                    family.WorkCenter.PlantId &&
                                reference.WorkCenterId ==
                                    family.WorkCenter.WorkCenterId))
                        .ToArray();

                foreach (ProductionRouting routing in routings)
                {
                    for (int period = 1;
                         period <= instance.PlanningHorizon;
                         period++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        string itemSetupKey =
                            new MathematicalDomainKeyBuilder(
                                MathematicalDecisionCategory.Setup)
                                .Add(
                                    MathematicalDomainKeySegment.Routing,
                                    routing.Id)
                                .Add(
                                    MathematicalDomainKeySegment.Period,
                                    period)
                                .Build();

                        string familySetupKey =
                            new MathematicalDomainKeyBuilder(
                                MathematicalDecisionCategory
                                    .AuxiliaryProductionFamilySetup)
                                .Add(
                                    MathematicalDomainKeySegment.SetupFamily,
                                    family.Id)
                                .Add(
                                    MathematicalDomainKeySegment.Period,
                                    period)
                                .Build();

                        var expression =
                            new LinearExpressionBuilder();

                        expression.Add(
                            context.GetVariable(itemSetupKey),
                            1.0);

                        expression.Add(
                            context.GetVariable(familySetupKey),
                            -1.0);

                        AddConstraint(
                            context,
                            $"productionFamilySetupLink_f{family.Id}" +
                            $"_r{routing.Id}_t{period}",
                            expression.Build(),
                            MathematicalConstraintSense.LessThanOrEqual,
                            0.0,
                            description:
                                "Item setup implies shared family setup.");
                    }
                }
            }
        }

        return ValueTask.CompletedTask;
    }
}
