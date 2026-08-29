using LotSizingDataModel.Core.LogicalModel;

namespace LotSizingDataModel.Core.Tests.LogicalModel;

public sealed class ItemTests
{
    [Fact]
    public void Constructor_SetsIdentityAndBillOfMaterialsLevel()
    {
        var item = new Item(
            id: 17,
            name: "Finished product",
            billOfMaterialsLevel: 3);

        Assert.Equal(17, item.Id);
        Assert.Equal("Finished product", item.Name);
        Assert.Equal(3, item.BillOfMaterialsLevel);
    }

    [Fact]
    public void BillOfMaterialsLevel_RejectsNegativeValue()
    {
        var item = new Item();

        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => item.BillOfMaterialsLevel = -1);

        Assert.Equal("value", exception.ParamName);
    }
}
