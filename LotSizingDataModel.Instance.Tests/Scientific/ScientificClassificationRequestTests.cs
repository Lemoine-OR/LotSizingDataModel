using LotSizingDataModel.Instance.Scientific;

namespace LotSizingDataModel.Instance.Tests.Scientific;

public sealed class ScientificClassificationRequestTests
{
    [Fact]
    public void DeclaredNotation_IsTrimmedButNotCanonicalizedOrOverwritten()
    {
        var request =
            new ScientificClassificationRequest(
                declaredNotation:
                    "  1,SL,Net:UNK | Dem | Obj:?  ");

        Assert.Equal(
            "1,SL,Net:UNK | Dem | Obj:?",
            request.DeclaredNotation);
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidNumericalTolerance_IsRejected(
        double tolerance)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ScientificClassificationRequest(
                    numericalTolerance: tolerance));
    }
}
