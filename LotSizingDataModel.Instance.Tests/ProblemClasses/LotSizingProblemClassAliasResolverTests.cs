using LotSizingDataModel.Instance.ProblemClasses;

namespace LotSizingDataModel.Instance.Tests.ProblemClasses;

public sealed class LotSizingProblemClassAliasResolverTests
{
    private readonly LotSizingProblemClassAliasResolver _resolver =
        new();

    [Fact]
    public void Ulsp_IsAmbiguousAcrossSingleAndMultiItemUsage()
    {
        LotSizingProblemClassAliasResolution result =
            _resolver.Resolve("ULSP");

        Assert.Equal(
            LotSizingProblemClassAliasResolutionKind.Ambiguous,
            result.Kind);

        Assert.Equal(
            2,
            result.Matches.Count);

        Assert.Contains(
            result.Matches,
            definition =>
                definition.Id ==
                CanonicalLotSizingProblemClassId
                    .SingleItemUncapacitatedLotSizing);

        Assert.Contains(
            result.Matches,
            definition =>
                definition.Id ==
                CanonicalLotSizingProblemClassId
                    .MultiItemUncapacitatedLotSizing);
    }

    [Fact]
    public void Clsp_IsAmbiguousAcrossSingleAndMultiItemUsage()
    {
        LotSizingProblemClassAliasResolution result =
            _resolver.Resolve("CLSP");

        Assert.Equal(
            LotSizingProblemClassAliasResolutionKind.Ambiguous,
            result.Kind);

        Assert.Equal(
            2,
            result.Matches.Count);
    }

    [Fact]
    public void Mllp_ResolvesToUncapacitatedMultiLevelClass()
    {
        LotSizingProblemClassAliasResolution result =
            _resolver.Resolve("MLLP");

        Assert.Equal(
            LotSizingProblemClassAliasResolutionKind.Unique,
            result.Kind);

        Assert.Equal(
            CanonicalLotSizingProblemClassId
                .UncapacitatedMultiLevelLotSizing,
            result.UniqueMatch!.Id);
    }

    [Fact]
    public void Mlls_IsAmbiguousWithRespectToCapacity()
    {
        LotSizingProblemClassAliasResolution result =
            _resolver.Resolve("MLLS");

        Assert.Equal(
            LotSizingProblemClassAliasResolutionKind.Ambiguous,
            result.Kind);

        Assert.Contains(
            result.Matches,
            definition =>
                definition.Id ==
                CanonicalLotSizingProblemClassId
                    .UncapacitatedMultiLevelLotSizing);

        Assert.Contains(
            result.Matches,
            definition =>
                definition.Id ==
                CanonicalLotSizingProblemClassId
                    .MultiLevelCapacitatedLotSizing);
    }

    [Theory]
    [InlineData("DRP")]
    [InlineData("MRP")]
    public void PlanningParadigms_AreNotResolvedAsProblemClasses(
        string query)
    {
        Assert.Equal(
            LotSizingProblemClassAliasResolutionKind.Unknown,
            _resolver.Resolve(query).Kind);
    }
}
