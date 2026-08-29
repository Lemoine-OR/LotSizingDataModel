using LotSizingDataModel.Instance.Descriptors.Classical;
using LotSizingDataModel.Instance.Descriptors.Temporal;
using LotSizingDataModel.Instance.Notation;

namespace LotSizingDataModel.Instance.Tests.Descriptors.Classical;

public sealed class ZeroOrFullCapacityProductionAnalyzerTests
{
    [Fact]
    public void EqualPositiveLowerBoundsAndCapacities_AreSatisfied()
    {
        ZeroOrFullCapacityProductionAnalysis analysis =
            new ZeroOrFullCapacityProductionAnalyzer()
                .Analyze(
                    minimumLotSizes:
                        new[] { 10.0, 12.0, 8.0 },
                    productionCapacities:
                        new[] { 10.0, 12.0, 8.0 });

        Assert.Equal(
            UniversalConditionState.Satisfied,
            analysis.State);

        Assert.True(analysis.IsSatisfied);
    }

    [Fact]
    public void LowerBoundBelowCapacity_IsNotSatisfied()
    {
        ZeroOrFullCapacityProductionAnalysis analysis =
            new ZeroOrFullCapacityProductionAnalyzer()
                .Analyze(
                    minimumLotSizes:
                        new[] { 5.0, 12.0 },
                    productionCapacities:
                        new[] { 10.0, 12.0 });

        Assert.Equal(
            UniversalConditionState.NotSatisfied,
            analysis.State);
    }

    [Fact]
    public void DegenerateZeroCapacity_IsNotClaimedAsZeroOrFull()
    {
        ZeroOrFullCapacityProductionAnalysis analysis =
            new ZeroOrFullCapacityProductionAnalyzer()
                .Analyze(
                    minimumLotSizes:
                        new[] { 0.0 },
                    productionCapacities:
                        new[] { 0.0 });

        Assert.Equal(
            UniversalConditionState.NotSatisfied,
            analysis.State);
    }

    [Fact]
    public void NumericalTolerance_IsRespected()
    {
        var tolerance =
            new TemporalPatternTolerance(
                absoluteTolerance: 1e-6,
                relativeTolerance: 0.0);

        ZeroOrFullCapacityProductionAnalysis analysis =
            new ZeroOrFullCapacityProductionAnalyzer()
                .Analyze(
                    minimumLotSizes:
                        new[] { 9.9999995 },
                    productionCapacities:
                        new[] { 10.0 },
                    tolerance);

        Assert.True(analysis.IsSatisfied);
    }

    [Fact]
    public void InvalidSeries_AreRejected()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new ZeroOrFullCapacityProductionAnalyzer()
                    .Analyze(
                        minimumLotSizes:
                            new[] { 1.0, 2.0 },
                        productionCapacities:
                            new[] { 1.0 }));

        Assert.Throws<ArgumentException>(
            () =>
                new ZeroOrFullCapacityProductionAnalyzer()
                    .Analyze(
                        minimumLotSizes:
                            new[] { -1.0 },
                        productionCapacities:
                            new[] { 1.0 }));
    }
}
