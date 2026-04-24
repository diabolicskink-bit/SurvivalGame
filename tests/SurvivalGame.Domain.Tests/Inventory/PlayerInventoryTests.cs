using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class PlayerInventoryTests
{
    [Fact]
    public void NewPlayerStartsWithEmptyInventory()
    {
        var player = new PlayerState();

        Assert.True(player.Inventory.IsEmpty);
        Assert.Empty(player.Inventory.Items);
    }

    [Fact]
    public void AddTracksHeldItemQuantity()
    {
        var inventory = new PlayerInventory();
        var itemId = new ItemId("stone");

        inventory.Add(itemId, 3);

        Assert.False(inventory.IsEmpty);
        Assert.Equal(3, inventory.CountOf(itemId));
        Assert.Equal(new InventoryItemStack(itemId, 3), Assert.Single(inventory.Items));
    }

    [Fact]
    public void AddStacksMatchingItems()
    {
        var inventory = new PlayerInventory();
        var itemId = new ItemId("branch");

        inventory.Add(itemId);
        inventory.Add(itemId, 2);

        Assert.Equal(3, inventory.CountOf(itemId));
    }

    [Fact]
    public void TryRemoveReducesHeldQuantity()
    {
        var inventory = new PlayerInventory();
        var itemId = new ItemId("rag");

        inventory.Add(itemId, 5);
        var removed = inventory.TryRemove(itemId, 2);

        Assert.True(removed);
        Assert.Equal(3, inventory.CountOf(itemId));
    }

    [Fact]
    public void TryRemoveDeletesStackWhenQuantityReachesZero()
    {
        var inventory = new PlayerInventory();
        var itemId = new ItemId("tin_can");

        inventory.Add(itemId, 2);
        var removed = inventory.TryRemove(itemId, 2);

        Assert.True(removed);
        Assert.True(inventory.IsEmpty);
        Assert.Equal(0, inventory.CountOf(itemId));
    }

    [Fact]
    public void TryRemoveReturnsFalseWhenNotEnoughItemsAreHeld()
    {
        var inventory = new PlayerInventory();
        var itemId = new ItemId("water_bottle");

        inventory.Add(itemId);
        var removed = inventory.TryRemove(itemId, 2);

        Assert.False(removed);
        Assert.Equal(1, inventory.CountOf(itemId));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ItemIdRejectsEmptyValues(string value)
    {
        Assert.Throws<ArgumentException>(() => new ItemId(value));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddRejectsNonPositiveQuantities(int quantity)
    {
        var inventory = new PlayerInventory();

        Assert.Throws<ArgumentOutOfRangeException>(() => inventory.Add(new ItemId("stone"), quantity));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TryRemoveRejectsNonPositiveQuantities(int quantity)
    {
        var inventory = new PlayerInventory();

        Assert.Throws<ArgumentOutOfRangeException>(() => inventory.TryRemove(new ItemId("stone"), quantity));
    }
}
