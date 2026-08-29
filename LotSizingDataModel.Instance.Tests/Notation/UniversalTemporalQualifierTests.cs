using LotSizingDataModel.Instance.Descriptors.Temporal;
using LotSizingDataModel.Instance.Notation;

namespace LotSizingDataModel.Instance.Tests.Notation;

public sealed class UniversalTemporalQualifierTests
{
    [Fact]
    public void ParserRenderer_RoundTripsGenericTemporalQualifiers()
    {
        string text =
            "1,SL,Net:UNK | Dem,Det,Prod,Cap:P," +
            "TP:SC=NI,TP:HC=G,TP:PC=NI,TP:CapP=ND | Obj:Econ";

        UniversalLotSizingNotation parsed =
            new UniversalNotationParser()
                .Parse(text);

        Assert.Equal(
            text,
            parsed.Render());

        Assert.Equal(
            4,
            parsed.Beta.TemporalQualifiers.Count);
    }

    [Fact]
    public void Renderer_CanonicalizesTemporalQualifierOrder()
    {
        string raw =
            "1,SL,Net:UNK | TP:CapP=ND,Prod,TP:PC=NI," +
            "Dem,TP:SC=NI,Det,Cap:P,TP:HC=G | Obj:Econ";

        string canonical =
            new UniversalNotationParser()
                .Canonicalize(raw);

        Assert.Equal(
            "1,SL,Net:UNK | Dem,Det,Prod,Cap:P," +
            "TP:SC=NI,TP:HC=G,TP:PC=NI,TP:CapP=ND | Obj:Econ",
            canonical);
    }

    [Fact]
    public void Beta_RejectsConflictingPatternsForSameParameter()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new UniversalNotationBeta(
                    temporalQualifiers:
                        new[]
                        {
                            new UniversalTemporalQualifier(
                                UniversalTemporalParameter.SetupCost,
                                TemporalPatternType.Constant),
                            new UniversalTemporalQualifier(
                                UniversalTemporalParameter.SetupCost,
                                TemporalPatternType.General)
                        }));
    }

    [Fact]
    public void MinimumLotSize_IsGenericTemporalParameter()
    {
        string text =
            "1,SL,Net:UNK | MinLot,TP:MinLot=C | Obj:Econ";

        UniversalLotSizingNotation parsed =
            new UniversalNotationParser()
                .Parse(text);

        Assert.Equal(
            text,
            parsed.Render());

        Assert.Contains(
            parsed.Beta.TemporalQualifiers,
            qualifier =>
                qualifier.Parameter ==
                    UniversalTemporalParameter.MinimumLotSize &&
                qualifier.Pattern ==
                    TemporalPatternType.Constant);
    }

    [Fact]
    public void Parser_RejectsUnknownTemporalParameter()
    {
        Assert.Throws<FormatException>(
            () =>
                new UniversalNotationParser()
                    .Parse(
                        "1,SL,Net:UNK | TP:Magic=C | Obj:Econ"));
    }

    [Fact]
    public void Parser_RejectsUnknownTemporalPattern()
    {
        Assert.Throws<FormatException>(
            () =>
                new UniversalNotationParser()
                    .Parse(
                        "1,SL,Net:UNK | TP:SC=XYZ | Obj:Econ"));
    }
}
