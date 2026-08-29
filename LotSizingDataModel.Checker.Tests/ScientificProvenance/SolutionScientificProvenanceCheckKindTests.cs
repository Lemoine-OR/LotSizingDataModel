using LotSizingDataModel.Checker.Scientific;

namespace LotSizingDataModel.Checker.Tests.ScientificProvenance;

public sealed class SolutionScientificProvenanceCheckKindTests
{
    [Fact]
    public void ProvenanceStates_AreOrthogonalAndExplicit()
    {
        Assert.NotEqual(
            SolutionScientificProvenanceCheckKind.Missing,
            SolutionScientificProvenanceCheckKind.Invalid);

        Assert.NotEqual(
            SolutionScientificProvenanceCheckKind.Stale,
            SolutionScientificProvenanceCheckKind.Contradiction);

        Assert.NotEqual(
            SolutionScientificProvenanceCheckKind.Incomplete,
            SolutionScientificProvenanceCheckKind.Contradiction);

        Assert.NotEqual(
            SolutionScientificProvenanceCheckKind.Coherent,
            SolutionScientificProvenanceCheckKind.Missing);
    }
}
