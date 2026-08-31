using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Notation;
namespace LotSizingDataModel.Instance.Tests.Notation;
public sealed class AdvancedSchedulingNotationTests
{
    [Fact]
    public void ForbiddenCarryOverAndGrouping_RenderExplicitTokens()
    {
        var d=new LotSizingProblemDescriptor{Production=new ProductionDescriptor{HasProduction=true,HasGroupingConstraints=true},Scheduling=new SchedulingDescriptor{HasIntegratedScheduling=true,BucketMode=SchedulingBucketMode.SmallBucket,SetupCarryOverPolicy=SetupCarryOverPolicy.Forbidden}};
        string notation=new UniversalNotationGenerator().Generate(d).Render();
        Assert.Contains("SCO:0",notation,StringComparison.Ordinal);
        Assert.Contains("Group",notation,StringComparison.Ordinal);
    }
}
