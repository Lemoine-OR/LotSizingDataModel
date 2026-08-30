using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class GlspChangeoverDefinitionConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId => "glspChangeoverDefinition";

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
        var microPeriods = profile.EnumerateMicroPeriods().ToArray();

        for (int index = 1; index < microPeriods.Length; index++)
        {
            var previous = microPeriods[index - 1];
            var current = microPeriods[index];

            foreach (ProductionRouting from in routings)
            {
                foreach (ProductionRouting to in routings)
                {
                    if (from.ItemId == to.ItemId) continue;

                    var z = GetVariable(context,
                        GlspFormulationVariableKeyFactory.CreateChangeoverKey(
                            plantId, workCenter.Id, from.ItemId, to.ItemId, current));
                    var yPrevious = GetVariable(context,
                        GlspFormulationVariableKeyFactory.CreateMicroSetupStateKey(
                            plantId, workCenter.Id, from.Id, from.ItemId, previous));
                    var yCurrent = GetVariable(context,
                        GlspFormulationVariableKeyFactory.CreateMicroSetupStateKey(
                            plantId, workCenter.Id, to.Id, to.ItemId, current));

                    string suffix = $"_i{from.ItemId}_j{to.ItemId}_t{current.MacroPeriod}_s{current.MicroPeriodIndex}";

                    AddConstraint(
                        context,
                        "glspChangeoverLower" + suffix,
                        new LinearExpressionBuilder().Add(z).Subtract(yPrevious).Subtract(yCurrent).Build(),
                        MathematicalConstraintSense.GreaterThanOrEqual,
                        -1.0);
                    AddConstraint(
                        context,
                        "glspChangeoverUpperPrevious" + suffix,
                        new LinearExpressionBuilder().Add(z).Subtract(yPrevious).Build(),
                        MathematicalConstraintSense.LessThanOrEqual,
                        0.0);
                    AddConstraint(
                        context,
                        "glspChangeoverUpperCurrent" + suffix,
                        new LinearExpressionBuilder().Add(z).Subtract(yCurrent).Build(),
                        MathematicalConstraintSense.LessThanOrEqual,
                        0.0);
                }
            }
        }

        return ValueTask.CompletedTask;
    }
}
