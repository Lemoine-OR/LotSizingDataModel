using LotSizingDataModel.Instance.Descriptors.Classical;
using LotSizingDataModel.Instance.Descriptors.Temporal;
using LotSizingDataModel.Instance.Notation;

namespace LotSizingDataModel.Instance.Tests.Descriptors.Classical;

public sealed class NonSpeculativeCostConditionAnalyzerTests
{
    [Fact]
    public void NonSpeculativeMargins_AreSatisfied()
    {
        NonSpeculativeCostConditionAnalysis analysis =
            new NonSpeculativeCostConditionAnalyzer()
                .Analyze(
                    adjacentUnitCosts:
                        new[] { 5.0, 6.0, 6.5 },
                    holdingCosts:
                        new[] { 1.5, 1.0 });

        Assert.Equal(
            UniversalConditionState.Satisfied,
            analysis.State);

        Assert.True(analysis.IsSatisfied);
        Assert.Equal(
            new[] { 0.5, 0.5 },
            analysis.TransformedHoldingMargins);
    }

    [Fact]
    public void SpeculativeAdjacentCostDrop_IsNotSatisfied()
    {
        NonSpeculativeCostConditionAnalysis analysis =
            new NonSpeculativeCostConditionAnalyzer()
                .Analyze(
                    adjacentUnitCosts:
                        new[] { 2.0, 5.0 },
                    holdingCosts:
                        new[] { 1.0 });

        Assert.Equal(
            UniversalConditionState.NotSatisfied,
            analysis.State);

        Assert.False(analysis.IsSatisfied);
        Assert.Equal(
            -2.0,
            analysis.MinimumMargin);
    }

    [Fact]
    public void NumericalTolerance_IsRespected()
    {
        var tolerance =
            new TemporalPatternTolerance(
                absoluteTolerance: 1e-6,
                relativeTolerance: 0.0);

        NonSpeculativeCostConditionAnalysis analysis =
            new NonSpeculativeCostConditionAnalyzer()
                .Analyze(
                    adjacentUnitCosts:
                        new[] { 1.0, 2.0000005 },
                    holdingCosts:
                        new[] { 1.0 },
                    tolerance);

        Assert.True(analysis.IsSatisfied);
    }

    [Fact]
    public void LengthMismatch_IsRejected()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new NonSpeculativeCostConditionAnalyzer()
                    .Analyze(
                        adjacentUnitCosts:
                            new[] { 1.0, 2.0, 3.0 },
                        holdingCosts:
                            new[] { 1.0 }));
    }

    [Fact]
    public void NonFiniteInput_IsRejected()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new NonSpeculativeCostConditionAnalyzer()
                    .Analyze(
                        adjacentUnitCosts:
                            new[] { 1.0, double.NaN },
                        holdingCosts:
                            new[] { 1.0 }));
    }
}
