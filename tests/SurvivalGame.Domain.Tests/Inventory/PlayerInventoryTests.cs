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
        Assert.Equal(PrototypeItemContainers.PlayerInventory, inventory.Container.Id);
        Assert.True(inventory.Container.TryGetPlacement(ContainerItemRef.Stack(itemId), out var placement));
        Assert.Equal(InventoryItemSize.Default, placement.Size);
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
        Assert.False(inventory.Container.Contains(ContainerItemRef.Stack(itemId)));
    }

    [Fact]
    public void AddCanUseNonDefaultInventorySize()
    {
        var inventory = new PlayerInventory();
        var rifle = new ItemId("rifle");

        inventory.Add(rifle, size: new InventoryItemSize(5, 2));

        Assert.True(inventory.Container.TryGetPlacement(ContainerItemRef.Stack(rifle), out var placement));
        Assert.Equal(new InventoryItemSize(5, 2), placement.Size);
    }

    [Fact]
    public void TryAddFailsWhenInventoryGridHasNoRoom()
    {
        var inventory = new PlayerInventory();

        for (var index = 0; index < 200; index++)
        {
            Assert.True(inventory.TryAdd(new ItemId($"stone_{index}")));
        }

        Assert.False(inventory.TryAdd(new ItemId("overflow")));
        Assert.Equal(200, inventory.Items.Count);
    }

    [Fact]
    public void GridExemptStacksDoNotCreateInventoryGridPlacement()
    {
        var inventory = new PlayerInventory();
        var ammo = new ItemId("ammo_9mm_standard");

        inventory.Add(ammo, 50, usesGrid: false);

        Assert.Equal(50, inventory.CountOf(ammo));
        Assert.False(inventory.Container.Contains(ContainerItemRef.Stack(ammo)));
    }

    [Fact]
    public void GridExemptStacksCanBeAddedWhenInventoryGridIsFull()
    {
        var inventory = new PlayerInventory();
        var ammo = new ItemId("ammo_9mm_standard");

        for (var index = 0; index < 200; index++)
        {
            Assert.True(inventory.TryAdd(new ItemId($"stone_{index}")));
        }

        Assert.True(inventory.TryAdd(ammo, 50, usesGrid: false));
        Assert.Equal(50, inventory.CountOf(ammo));
        Assert.False(inventory.Container.Contains(ContainerItemRef.Stack(ammo)));
    }

    [Theory]
    [InlineData("Ammunition", false)]
    [InlineData("Weapon", true)]
    [InlineData("FeedDevice", true)]
    [InlineData("Food", true)]
    [InlineData("Material", true)]
    public void InventoryGridRulesOnlyExemptsAmmunition(string category, bool expectedUsesGrid)
    {
        var item = new ItemDefinition(new ItemId($"test_{category}"), category, "", category);

        Assert.Equal(expectedUsesGrid, InventoryGridRules.UsesGrid(item));
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
