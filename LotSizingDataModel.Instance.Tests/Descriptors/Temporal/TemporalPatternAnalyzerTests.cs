using LotSizingDataModel.Instance.Descriptors.Temporal;

namespace LotSizingDataModel.Instance.Tests.Descriptors.Temporal;

public sealed class TemporalPatternAnalyzerTests
{
    private readonly TemporalPatternAnalyzer _analyzer = new();

    [Theory]
    [InlineData(TemporalPatternType.Zero, 0.0, 0.0, 0.0)]
    [InlineData(TemporalPatternType.Constant, 5.0, 5.0, 5.0)]
    [InlineData(TemporalPatternType.NonIncreasing, 5.0, 4.0, 1.0)]
    [InlineData(TemporalPatternType.NonDecreasing, 1.0, 4.0, 5.0)]
    [InlineData(TemporalPatternType.General, 1.0, 5.0, 2.0)]
    public void Analyze_ReturnsCanonicalHistoricalPattern(
        TemporalPatternType expected,
        double first,
        double second,
        double third)
    {
        TemporalPatternAnalysis result =
            _analyzer.Analyze(
                new[] { first, second, third });

        Assert.Equal(expected, result.Pattern);
    }

    [Fact]
    public void Analyze_ZeroTakesPrecedenceOverConstantAndMonotonicity()
    {
        TemporalPatternAnalysis result =
            _analyzer.Analyze(
                new[] { 0.0, 0.0, 0.0 });

        Assert.Equal(TemporalPatternType.Zero, result.Pattern);
        Assert.Equal("Z", result.HistoricalCode);
    }

    [Fact]
    public void Analyze_NonZeroConstantTakesPrecedenceOverMonotonicity()
    {
        TemporalPatternAnalysis result =
            _analyzer.Analyze(
                new[] { 7.0, 7.0, 7.0 });

        Assert.Equal(TemporalPatternType.Constant, result.Pattern);
        Assert.Equal("C", result.HistoricalCode);
    }

    [Fact]
    public void Analyze_UsesExplicitToleranceForNearConstantSeries()
    {
        var tolerance =
            new TemporalPatternTolerance(
                absoluteTolerance: 1e-6,
                relativeTolerance: 0.0);

        TemporalPatternAnalysis result =
            _analyzer.Analyze(
                new[]
                {
                    5.0,
                    5.0 + 4e-7,
                    5.0 - 3e-7
                },
                tolerance);

        Assert.Equal(TemporalPatternType.Constant, result.Pattern);
    }

    [Fact]
    public void Analyze_ToleranceDirectionalAmbiguityFallsBackToGeneral()
    {
        var tolerance =
            new TemporalPatternTolerance(
                absoluteTolerance: 1e-6,
                relativeTolerance: 0.0);

        TemporalPatternAnalysis result =
            _analyzer.Analyze(
                new[]
                {
                    10.0,
                    10.0 + 0.9e-6,
                    10.0 + 1.8e-6
                },
                tolerance);

        Assert.Equal(TemporalPatternType.General, result.Pattern);
    }

    [Fact]
    public void Analyze_SingleNonZeroValueIsConstant()
    {
        TemporalPatternAnalysis result =
            _analyzer.Analyze(
                new[] { 3.0 });

        Assert.Equal(TemporalPatternType.Constant, result.Pattern);
    }

    [Fact]
    public void Analyze_RejectsEmptySeries()
    {
        Assert.Throws<ArgumentException>(
            () => _analyzer.Analyze(Array.Empty<double>()));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Analyze_RejectsNonFiniteValues(double invalid)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                _analyzer.Analyze(
                    new[] { 1.0, invalid, 2.0 }));
    }

    [Fact]
    public void HistoricalCodes_AreStable()
    {
        Assert.Equal("Z", TemporalPatternType.Zero.ToBitranYanasseCode());
        Assert.Equal("C", TemporalPatternType.Constant.ToBitranYanasseCode());
        Assert.Equal("NI", TemporalPatternType.NonIncreasing.ToBitranYanasseCode());
        Assert.Equal("ND", TemporalPatternType.NonDecreasing.ToBitranYanasseCode());
        Assert.Equal("G", TemporalPatternType.General.ToBitranYanasseCode());
    }
}
