using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Descriptors.Temporal;
using LotSizingDataModel.Instance.Notation;

namespace LotSizingDataModel.Instance.Tests.Notation;

public sealed class StartUpNotationTests
{
    [Fact]
    public void Parser_RoundTripsStartUpTimeFeatureAndConstantQualifier()
    {
        const string text =
            "1,SL,Net:UNK | " +
            "Dem,Prod,SU,SUT,TP:SUT=C | Obj:Econ";

        UniversalLotSizingNotation notation =
            new UniversalNotationParser().Parse(text);

        Assert.Equal(
            text,
            notation.Render());

        Assert.Contains(
            UniversalNotationFeature.StartUpCost,
            notation.Beta.Features);

        Assert.Contains(
            UniversalNotationFeature.StartUpTime,
            notation.Beta.Features);

        Assert.Contains(
            notation.Beta.TemporalQualifiers,
            qualifier =>
                qualifier.Parameter ==
                    UniversalTemporalParameter.StartUpTime &&
                qualifier.Pattern ==
                    TemporalPatternType.Constant);
    }

    [Fact]
    public void Generator_EmitsDistinctSetupAndStartUpTokens()
    {
        var descriptor =
            new LotSizingProblemDescriptor
            {
                Setup =
                    new SetupDescriptor
                    {
                        HasSetupCosts = true,
                        HasSetupTimes = true,
                        HasStartUpCosts = true,
                        HasStartUpTimes = true
                    }
            };

        string text =
            new UniversalNotationGenerator()
                .Generate(descriptor)
                .Render();

        Assert.Contains("SC", text, StringComparison.Ordinal);
        Assert.Contains("ST", text, StringComparison.Ordinal);
        Assert.Contains("SU", text, StringComparison.Ordinal);
        Assert.Contains("SUT", text, StringComparison.Ordinal);
    }
}
