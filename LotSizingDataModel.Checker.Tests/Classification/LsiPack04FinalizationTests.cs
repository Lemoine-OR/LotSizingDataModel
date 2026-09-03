using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Classification.Notation;
using LotSizingDataModel.Instance.Serialization;

namespace LotSizingDataModel.Checker.Tests.Classification;

public sealed class LsiPack04FinalizationTests
{
    [Fact]
    public void CanonicalNotation_RoundTripsExactly()
    {
        var signature = new LotSizingInstanceSignature
        {
            Planning = new PlanningSignature
            {
                Horizon = PlanningHorizonKind.Finite,
                TimeModel = TimeModelKind.Discrete,
                BucketStructure = BucketStructureKind.Unknown,
                Information = InformationStructureKind.Deterministic,
                DemandPattern = DemandPatternKind.Dynamic,
                DemandSource = DemandSourceKind.Exogenous
            },

            System = new SystemSignature
            {
                Items = CardinalityKind.Multiple,
                Levels = CardinalityKind.Multiple,
                Network = NetworkStructureKind.SingleSite,
                Routing = RoutingStructureKind.Unknown,
                ResourceEnvironment = ResourceEnvironmentKind.SingleResource
            },

            Size = new InstanceSizeSignature
            {
                Periods = 12,
                Items = 5,
                Plants = 1,
                WorkCenters = 1,
                Warehouses = 0,
                Suppliers = 0,
                DistributionCenters = 0,
                TransportResources = 0,
                BomRelationships = 4,
                MaximumBomDepth = 4
            }
        };

        var mixedProfile = new TemporalProfile
        {
            Kind = TemporalProfileKind.Mixed
        };

        mixedProfile.ReplaceComponents(
            new[]
            {
                TemporalProfileKind.Constant,
                TemporalProfileKind.General
            });

        signature.Features.Set(
            "SET.C",
            FeatureState.Present,
            mixedProfile);

        string canonical =
            signature.CanonicalNotation;

        LotSizingInstanceSignature clone =
            LotSizingSignatureParser.Parse(canonical);

        Assert.Equal(
            canonical,
            clone.CanonicalNotation);
    }

    [Fact]
    public void Parser_RejectsInvalidCanonicalNotation()
    {
        bool success =
            LotSizingSignatureParser.TryParse(
                "LSI/1.0: invalid",
                out _,
                out string error);

        Assert.False(success);
        Assert.NotEmpty(error);
    }

    [Fact]
    public void DellaertJeunetFixture_ClassificationSignatureRoundTripsXml()
    {
        string fixturePath =
            Path.Combine(
                AppContext.BaseDirectory,
                "Fixtures",
                "DJ_Petit_5items_12periodes_Serial_ID45_ph1in45st1de6mh1ms0.xml");

        LotSizingDataModel.Instance.LotSizingInstance instance =
            LotSizingInstanceXmlSerializer.DeserializeFromFile(
                fixturePath);

        instance.ProblemClassification =
            LotSizingProblemClassifier.Classify(
                instance.SupplyChain,
                KnownProblemTypeCatalogFactory.CreateStandardCatalog());

        string before =
            instance.ProblemClassification
                .Signature
                .CanonicalNotation;

        string xml =
            LotSizingInstanceXmlSerializer.SerializeToString(
                instance,
                validateBeforeSerialization: false);

        Assert.Contains(
            "<signature",
            xml,
            StringComparison.Ordinal);

        LotSizingDataModel.Instance.LotSizingInstance clone =
            LotSizingInstanceXmlSerializer.DeserializeFromString(
                xml,
                validateAfterDeserialization: false);

        string after =
            clone.ProblemClassification
                .Signature
                .CanonicalNotation;

        Assert.Equal(before, after);
    }

    [Fact]
    public void DellaertJeunetFixtures_HaveStableLsiCoverage()
    {
        string fixtureDirectory =
            Path.Combine(
                AppContext.BaseDirectory,
                "Fixtures");

        string[] fixturePaths =
            Directory
                .EnumerateFiles(
                    fixtureDirectory,
                    "DJ_*.xml",
                    SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

        Assert.NotEmpty(fixturePaths);

        var signatures =
            new List<LotSizingInstanceSignature>();

        foreach (string fixturePath in fixturePaths)
        {
            LotSizingDataModel.Instance.LotSizingInstance instance =
                LotSizingInstanceXmlSerializer.DeserializeFromFile(
                    fixturePath);

            LotSizingProblemClassification classification =
                LotSizingProblemClassifier.Classify(
                    instance.SupplyChain,
                    KnownProblemTypeCatalogFactory.CreateStandardCatalog());

            Assert.False(
                string.IsNullOrWhiteSpace(
                    classification.Signature.CanonicalNotation));

            signatures.Add(classification.Signature);
        }

        LsiSignatureCoverageReport report =
            LsiSignatureCoverageReport.Analyze(signatures);

        Assert.Equal(
            fixturePaths.Length,
            report.SignatureCount);

        Assert.Equal(
            fixturePaths.Length,
            report.LegacyProjectedCount);
    }

    [Fact]
    public void LegacyXmlWithoutSignature_RemainsReadable()
    {
        string fixturePath =
            Path.Combine(
                AppContext.BaseDirectory,
                "Fixtures",
                "DJ_Petit_5items_12periodes_Serial_ID45_ph1in45st1de6mh1ms0.xml");

        LotSizingDataModel.Instance.LotSizingInstance instance =
            LotSizingInstanceXmlSerializer.DeserializeFromFile(
                fixturePath);

        Assert.NotNull(instance);
        Assert.NotNull(instance.ProblemClassification);
        Assert.NotNull(instance.ProblemClassification.Signature);
    }
}
