using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Execution;
namespace LotSizingDataModel.Solver.Mapping;
public sealed class AuxiliaryMacroProductionActivationDecisionMapper : MathematicalDecisionMapperBase
{
    public override string Category=>MathematicalDecisionCategory.AuxiliaryMacroProductionActivation;
    protected override void MapValue(MathematicalSolutionMappingContext context,LotSizingSolution solution,MathematicalDomainKey domainKey,MathematicalVariableValue variableValue){ArgumentNullException.ThrowIfNull(context);ArgumentNullException.ThrowIfNull(solution);ArgumentNullException.ThrowIfNull(domainKey);ArgumentNullException.ThrowIfNull(variableValue);}
}
