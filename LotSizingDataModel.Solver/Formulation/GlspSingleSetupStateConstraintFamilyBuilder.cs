using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class GlspSingleSetupStateConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId => "glspSingleSetupState";

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        (int plantId, var workCenter, var profile) =
            GlspSchedulingData.GetSchedulingWorkCenter(instance);
        IReadOnlyList<ProductionRouting> routings =
            GlspSchedulingData.GetRoutings(instance, plantId, workCenter.Id);

        foreach (var microPeriod in profile.EnumerateMicroPeriods())
        {
            var expression = new LinearExpressionBuilder();
            foreach (ProductionRouting routing in routings)
            {
                expression.Add(GetVariable(context,
                    GlspFormulationVariableKeyFactory.CreateMicroSetupStateKey(
                        plantId, workCenter.Id, routing.Id, routing.ItemId, microPeriod)));
            }

            AddConstraint(
                context,
                $"glspSingleSetup_t{microPeriod.MacroPeriod}_s{microPeriod.MicroPeriodIndex}",
                expression.Build(),
                MathematicalConstraintSense.Equal,
                1.0,
                description: "Exactly one setup state is active in every GLSP micro-period.");
        }

        return ValueTask.CompletedTask;
    }
}
