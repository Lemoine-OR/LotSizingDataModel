using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Classification.Notation;
using LotSizingDataModel.Instance.Serialization;

namespace LotSizingDataModel.Checker.Tests.Classification;

public sealed class LsiPack03LocalCompatibilityTests
{
    [Fact]
    public void DellaertJeunetFixture_LegacyProjectionMatchesClassifier()
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

        LegacyProblemFamilyProjection projection =
            LegacyProblemFamilyProjector.Project(
                classification.Signature);

        Assert.Equal(
            classification.PrimaryProblemTypeCode,
            projection.PrimaryCode);

        Assert.True(
            LegacyProblemFamilyProjector
                .IsConsistentWith(classification));
    }

    [Theory]
    [InlineData(CardinalityKind.Single, CardinalityKind.Single, false, "LS-U")]
    [InlineData(CardinalityKind.Single, CardinalityKind.Single, true, "LS-C")]
    [InlineData(CardinalityKind.Multiple, CardinalityKind.Single, true, "CLSP")]
    [InlineData(CardinalityKind.Multiple, CardinalityKind.Multiple, false, "MLLP")]
    [InlineData(CardinalityKind.Multiple, CardinalityKind.Multiple, true, "MLCLSP")]
    public void Projection_CoversFiveLegacyFamilies(
        CardinalityKind items,
        CardinalityKind levels,
        bool capacitated,
        string expected)
    {
        var signature = new LotSizingInstanceSignature();

        signature.System.Items = items;
        signature.System.Levels = levels;

        signature.Features.Set(
            LsiFeatureCodes.ProductionCapacity,
            capacitated
                ? FeatureState.Present
                : FeatureState.Absent);

        LegacyProblemFamilyProjection projection =
            LegacyProblemFamilyProjector.Project(signature);

        Assert.Equal(expected, projection.PrimaryCode);
    }
}
