using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class TileItemMapTests
{
    [Fact]
    public void NewTileItemMapStartsEmpty()
    {
        var itemMap = new TileItemMap();

        Assert.True(itemMap.IsEmpty);
        Assert.Empty(itemMap.AllItems);
    }

    [Fact]
    public void PlaceAddsItemsToPosition()
    {
        var itemMap = new TileItemMap();
        var position = new GridPosition(3, 4);

        itemMap.Place(position, PrototypeItems.Stone, 2);

        Assert.False(itemMap.IsEmpty);
        Assert.Equal(new GroundItemStack(PrototypeItems.Stone, 2), Assert.Single(itemMap.ItemsAt(position)));
    }

    [Fact]
    public void PlaceStacksMatchingItemsOnSamePosition()
    {
        var itemMap = new TileItemMap();
        var position = new GridPosition(5, 6);

        itemMap.Place(position, PrototypeItems.Branch);
        itemMap.Place(position, PrototypeItems.Branch, 3);

        Assert.Equal(new GroundItemStack(PrototypeItems.Branch, 4), Assert.Single(itemMap.ItemsAt(position)));
    }

    [Fact]
    public void ItemsAtReturnsEmptyForPositionsWithoutItems()
    {
        var itemMap = new TileItemMap();

        Assert.Empty(itemMap.ItemsAt(new GridPosition(1, 1)));
    }

    [Fact]
    public void TakeAllAtRemovesAndReturnsItemsAtPosition()
    {
        var itemMap = new TileItemMap();
        var position = new GridPosition(2, 3);
        itemMap.Place(position, PrototypeItems.Stone, 2);

        var removedItems = itemMap.TakeAllAt(position);

        Assert.Equal(new GroundItemStack(PrototypeItems.Stone, 2), Assert.Single(removedItems));
        Assert.Empty(itemMap.ItemsAt(position));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PlaceRejectsNonPositiveQuantities(int quantity)
    {
        var itemMap = new TileItemMap();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            itemMap.Place(new GridPosition(1, 1), PrototypeItems.Stone, quantity)
        );
    }
}
