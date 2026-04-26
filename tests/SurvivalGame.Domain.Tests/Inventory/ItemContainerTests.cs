using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class ItemContainerTests
{
    [Fact]
    public void ContainerPlacesRectangularItemsInsideBounds()
    {
        var container = new ItemContainer(new ContainerId("crate"), "Crate", new InventoryItemSize(4, 3));
        var item = ContainerItemRef.Stack(new ItemId("water_bottle"));

        var placed = container.TryPlace(item, new InventoryItemSize(2, 2), new InventoryGridPosition(1, 1));

        Assert.True(placed);
        var placement = Assert.Single(container.Placements);
        Assert.Equal(item, placement.Item);
        Assert.Equal(new InventoryGridPosition(1, 1), placement.Position);
        Assert.Equal(new InventoryItemSize(2, 2), placement.Size);
    }

    [Fact]
    public void ContainerRejectsOutOfBoundsPlacements()
    {
        var container = new ItemContainer(new ContainerId("crate"), "Crate", new InventoryItemSize(4, 3));

        var placed = container.TryPlace(
            ContainerItemRef.Stack(new ItemId("rifle")),
            new InventoryItemSize(3, 2),
            new InventoryGridPosition(2, 2)
        );

        Assert.False(placed);
        Assert.Empty(container.Placements);
    }

    [Fact]
    public void ContainerRejectsOverlappingPlacements()
    {
        var container = new ItemContainer(new ContainerId("crate"), "Crate", new InventoryItemSize(4, 3));
        Assert.True(container.TryPlace(
            ContainerItemRef.Stack(new ItemId("first")),
            new InventoryItemSize(2, 2),
            new InventoryGridPosition(0, 0)
        ));

        var placed = container.TryPlace(
            ContainerItemRef.Stack(new ItemId("second")),
            new InventoryItemSize(2, 1),
            new InventoryGridPosition(1, 1)
        );

        Assert.False(placed);
        Assert.Single(container.Placements);
    }

    [Fact]
    public void AutoPlacementFindsNextAvailableSpace()
    {
        var container = new ItemContainer(new ContainerId("crate"), "Crate", new InventoryItemSize(3, 2));
        Assert.True(container.TryPlace(
            ContainerItemRef.Stack(new ItemId("first")),
            new InventoryItemSize(2, 2),
            new InventoryGridPosition(0, 0)
        ));

        var second = ContainerItemRef.Stack(new ItemId("second"));
        var placed = container.TryAutoPlace(second, new InventoryItemSize(1, 2));

        Assert.True(placed);
        Assert.True(container.TryGetPlacement(second, out var placement));
        Assert.Equal(new InventoryGridPosition(2, 0), placement.Position);
    }

    [Fact]
    public void ContainerStoreRejectsDuplicateContainers()
    {
        var store = new ItemContainerStore();
        var container = new ItemContainer(new ContainerId("crate"), "Crate", new InventoryItemSize(3, 2));
        store.Add(container);

        Assert.Throws<InvalidOperationException>(() => store.Add(container));
        Assert.Same(container, store.Get(new ContainerId("crate")));
    }
}
