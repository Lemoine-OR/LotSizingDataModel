using LotSizingDataModel.Instance.Descriptors.Temporal;

namespace LotSizingDataModel.Instance.Tests.Descriptors.Temporal;

public sealed class BitranYanasseProfileAnalyzerTests
{
    [Fact]
    public void Analyze_ProducesHistoricalFourPositionCode()
    {
        var analyzer = new BitranYanasseProfileAnalyzer();

        BitranYanasseTemporalProfile profile =
            analyzer.Analyze(
                setupCost: new[] { 9.0, 8.0, 8.0, 4.0 },
                holdingCost: new[] { 1.0, 3.0, 2.0, 4.0 },
                productionCost: new[] { 7.0, 6.0, 6.0, 2.0 },
                capacity: new[] { 10.0, 12.0, 12.0, 15.0 });

        Assert.Equal(
            "NI/G/NI/ND",
            profile.HistoricalCode);

        Assert.Equal(
            TemporalPatternType.NonIncreasing,
            profile.SetupCost.Pattern);

        Assert.Equal(
            TemporalPatternType.General,
            profile.HoldingCost.Pattern);

        Assert.Equal(
            TemporalPatternType.NonIncreasing,
            profile.ProductionCost.Pattern);

        Assert.Equal(
            TemporalPatternType.NonDecreasing,
            profile.Capacity.Pattern);
    }

    [Fact]
    public void HistoricalProfile_DoesNotClaimUniversalNotation()
    {
        var analyzer = new BitranYanasseProfileAnalyzer();

        BitranYanasseTemporalProfile profile =
            analyzer.Analyze(
                new[] { 0.0, 0.0 },
                new[] { 1.0, 1.0 },
                new[] { 2.0, 1.0 },
                new[] { 3.0, 4.0 });

        Assert.Equal("Z/C/NI/ND", profile.HistoricalCode);
        Assert.DoesNotContain("|", profile.HistoricalCode);
    }
}
