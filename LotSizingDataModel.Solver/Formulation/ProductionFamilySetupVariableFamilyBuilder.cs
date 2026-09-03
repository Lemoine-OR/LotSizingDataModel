using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class ProductionFamilySetupVariableFamilyBuilder :
    StandardLotSizingVariableFamilyBuilderBase
{
    public override string FamilyId =>
        MathematicalDecisionCategory.AuxiliaryProductionFamilySetup;

    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        if (instance.SupplyChain.ProductionSetupFamilies.Count == 0)
        {
            return false;
        }

        if (!options.IncludeProductionSetups)
        {
            throw new InvalidOperationException(
                "Production-family setup semantics require item-level production setups.");
        }

        return true;
    }

    protected override ValueTask BuildFamilyAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (ProductionSetupFamily family
                 in instance.SupplyChain.ProductionSetupFamilies)
        {
            for (int period = 1;
                 period <= instance.PlanningHorizon;
                 period++)
            {
                string domainKey =
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

                AddBinaryVariable(
                    context,
                    $"wf_f{family.Id}_t{period}",
                    domainKey,
                    "Shared production-family setup activation.");
            }
        }

        return ValueTask.CompletedTask;
    }
}
