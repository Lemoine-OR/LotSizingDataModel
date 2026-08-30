using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// PLSP end-of-bucket state: exactly one setup state is carried from every
/// bucket into the next one.
/// </summary>
public sealed class PlspSingleSetupStateConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId =>
        "plspSingleSetupState";

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        for (
            int period = 1;
            period <= instance.PlanningHorizon;
            period++)
        {
            var expression =
                new LinearExpressionBuilder();

            foreach (
                var routing
                in instance.SupplyChain.ProductionRoutings)
            {
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

                expression.Add(
                    GetVariable(
                        context,
                        key));
            }

            AddConstraint(
                context,
                $"plspSingleSetupState_t{period}",
                expression.Build(),
                MathematicalConstraintSense.Equal,
                1.0,
                description:
                    "Exactly one PLSP setup state is carried out of each bucket.");
        }

        return ValueTask.CompletedTask;
    }
}
