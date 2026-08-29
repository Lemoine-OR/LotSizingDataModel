using LotSizingDataModel.Core.LogicalModel;

namespace LotSizingDataModel.Core.Tests.LogicalModel;

public sealed class ComponentRequirementTests
{
    [Fact]
    public void Constructor_SetsParentComponentAndQuantity()
    {
        var requirement =
            new ComponentRequirement(
                parentItemId: 10,
                componentItemId: 4,
                quantity: 3);

        Assert.Equal(10, requirement.ParentItemId);
        Assert.Equal(4, requirement.ComponentItemId);
        Assert.Equal(3, requirement.Quantity);
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(0, -1, 0)]
    [InlineData(0, 0, -1)]
    public void Constructor_RejectsNegativeDomainValues(
        int parentItemId,
        int componentItemId,
        int quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ComponentRequirement(
                    parentItemId,
                    componentItemId,
                    quantity));
    }
}
