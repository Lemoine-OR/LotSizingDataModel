using LotSizingDataModel.Instance.Descriptors.Temporal;
using LotSizingDataModel.Instance.Notation;

namespace LotSizingDataModel.Instance.Tests.Notation;

public sealed class MaximumLotAndSupplierCapacityNotationTests
{
    [Fact]
    public void Parser_RoundTripsBothTemporalQualifiers()
    {
        const string text =
            "1,SL,Net:UNK | " +
            "Dem,Prod,Cap:S,MaxLot,TP:MaxLot=C,TP:CapS=NI | Obj:Econ";

        UniversalLotSizingNotation notation =
            new UniversalNotationParser().Parse(text);

        Assert.Equal(text, notation.Render());

        Assert.Contains(
            notation.Beta.TemporalQualifiers,
            qualifier =>
                qualifier.Parameter ==
                    UniversalTemporalParameter.MaximumLotSize &&
                qualifier.Pattern ==
                    TemporalPatternType.Constant);

        Assert.Contains(
            notation.Beta.TemporalQualifiers,
            qualifier =>
                qualifier.Parameter ==
                    UniversalTemporalParameter.SupplierCapacity &&
                qualifier.Pattern ==
                    TemporalPatternType.NonIncreasing);
    }
}
