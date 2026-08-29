using LotSizingDataModel.Instance.Common;

namespace LotSizingDataModel.Instance.Tests.Common;

public sealed class ProductStructureTypeTests
{
    [Fact]
    public void Enum_ContainsCanonicalAcyclicProductStructureCategories()
    {
        ProductStructureType[] expected =
        [
            ProductStructureType.Unknown,
            ProductStructureType.IndependentItems,
            ProductStructureType.Serial,
            ProductStructureType.Assembly,
            ProductStructureType.Arborescent,
            ProductStructureType.General
        ];

        Assert.Equal(expected, Enum.GetValues<ProductStructureType>());
    }

    [Fact]
    public void ProductStructureType_DoesNotEncodeCyclesAsAValidCategory()
    {
        string[] names = Enum.GetNames<ProductStructureType>();

        Assert.DoesNotContain(
            names,
            name =>
                name.Contains(
                    "Cycle",
                    StringComparison.OrdinalIgnoreCase));
    }
}
