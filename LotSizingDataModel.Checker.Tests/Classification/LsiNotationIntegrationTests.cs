using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Classification.Notation;
using LotSizingDataModel.Instance.Serialization;

namespace LotSizingDataModel.Checker.Tests.Classification;

public sealed class LsiNotationIntegrationTests
{
    [Fact]
    public void TemporalProfileAnalyzer_ClassifiesCoreProfiles()
    {
        Assert.Equal(
            TemporalProfileKind.Zero,
            TemporalProfileAnalyzer.Analyze(
                new[] { 0.0, 0.0, 0.0 }).Kind);

        Assert.Equal(
            TemporalProfileKind.Constant,
            TemporalProfileAnalyzer.Analyze(
                new[] { 2.0, 2.0, 2.0 }).Kind);

        Assert.Equal(
            TemporalProfileKind.NonIncreasing,
            TemporalProfileAnalyzer.Analyze(
                new[] { 3.0, 2.0, 2.0, 1.0 }).Kind);

        Assert.Equal(
            TemporalProfileKind.NonDecreasing,
            TemporalProfileAnalyzer.Analyze(
                new[] { 1.0, 2.0, 2.0, 3.0 }).Kind);

        Assert.Equal(
            TemporalProfileKind.General,
            TemporalProfileAnalyzer.Analyze(
                new[] { 1.0, 3.0, 2.0 }).Kind);
    }

    [Fact]
    public void DellaertJeunetFixture_ProducesLsiSignature()
    {
        string fixturePath =
            Path.Combine(
                AppContext.BaseDirectory,
                "Fixtures",
                "DJ_Petit_5items_12periodes_Serial_ID45_ph1in45st1de6mh1ms0.xml");

        LotSizingDataModel.Instance.LotSizingInstance instance =
            LotSizingInstanceXmlSerializer.DeserializeFromFile(
                fixturePath);

        LotSizingProblemClassification classification =
            LotSizingProblemClassifier.Classify(
                instance.SupplyChain,
                KnownProblemTypeCatalogFactory.CreateStandardCatalog());

        Assert.NotNull(classification.Signature);
        Assert.Equal(5, classification.Signature.Size.Items);
        Assert.Equal(12, classification.Signature.Size.Periods);
        Assert.StartsWith(
            "LSI/1.0:",
            classification.Signature.CanonicalNotation);
        Assert.Contains(
            "PS=Serial",
            classification.Signature.CanonicalNotation,
            StringComparison.Ordinal);
    }
}
