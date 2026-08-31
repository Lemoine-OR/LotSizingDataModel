using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Synchronizes the distinct GLSP start-up variable with the exact
/// micro-period setup-start event.
/// </summary>
public sealed class GlspStartUpDefinitionConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    public override string ConstraintFamilyId =>
        "glspProductionStartUpDefinition";

    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return instance.SupplyChain.ProductionCharacteristics.Any(
            characteristic =>
                characteristic.StartUpCost is not null ||
                characteristic.StartUpTime is not null);
    }

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        var (plantId, workCenter, profile) =
            GlspSchedulingData.GetSchedulingWorkCenter(instance);

        IReadOnlyList<ProductionRouting> routings =
            GlspSchedulingData.GetRoutings(
                instance,
                plantId,
                workCenter.Id);

        var ordered =
            profile.EnumerateMicroPeriods().ToArray();

        for (int index = 0;
             index < ordered.Length;
             index++)
        {
            int fixedFrom =
                GlspSequenceSemantics.GetFixedPredecessorItemId(
                    profile,
                    index);

            bool reset =
                index > 0 &&
                GlspSequenceSemantics.IsResetBoundary(
                    profile,
                    ordered[index - 1],
                    ordered[index]);

            foreach (ProductionRouting routing
                     in routings)
            {
                ProductionCharacteristic characteristic =
                    GlspSchedulingData.GetCharacteristic(
                        instance,
                        routing,
                        plantId,
                        workCenter.Id);

                if (characteristic.StartUpCost is null &&
                    characteristic.StartUpTime is null)
                {
                    continue;
                }

                MathematicalVariable startUp =
                    GetVariable(
                        context,
                        ProductionStartUpDomainKeyFactory.CreateGlspKey(
                            plantId,
                            workCenter.Id,
                            routing.Id,
                            routing.ItemId,
                            ordered[index],
                            fixedFrom,
                            reset));

                MathematicalVariable setupStart =
                    GetVariable(
                        context,
                        GlspFormulationVariableKeyFactory.CreateMicroSetupStartKey(
                            plantId,
                            workCenter.Id,
                            routing.Id,
                            routing.ItemId,
                            ordered[index],
                            fixedFrom,
                            reset));

                AddConstraint(
                    context,
                    $"glspProductionStartUpDefinition_r{routing.Id}_t{ordered[index].MacroPeriod}_s{ordered[index].MicroPeriodIndex}",
                    new LinearExpressionBuilder()
                        .Add(startUp)
                        .Subtract(setupStart)
                        .Build(),
                    MathematicalConstraintSense.Equal,
                    0.0);
            }
        }

        return ValueTask.CompletedTask;
    }
}
