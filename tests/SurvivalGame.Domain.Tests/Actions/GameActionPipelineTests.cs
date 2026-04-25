using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class GameActionPipelineTests
{
    [Fact]
    public void WaitIsAlwaysAvailable()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();

        var actions = pipeline.GetAvailableActions(state);

        Assert.Contains(actions, action => action.Kind == GameActionKind.Wait);
    }

    [Fact]
    public void PickupIsAvailableOnlyWhenPlayerIsOnItems()
    {
        var pipeline = CreatePipeline();
        var groundItems = new TileItemMap();
        groundItems.Place(new GridPosition(1, 1), PrototypeItems.Stone, 2);
        var state = CreateState(groundItems, startPosition: new GridPosition(1, 1));

        var actions = pipeline.GetAvailableActions(state);

        Assert.Contains(actions, action => action.Kind == GameActionKind.Pickup);
    }

    [Fact]
    public void PickupIsNotAvailableWhenPlayerIsNotOnItems()
    {
        var pipeline = CreatePipeline();
        var groundItems = new TileItemMap();
        groundItems.Place(new GridPosition(1, 1), PrototypeItems.Stone, 2);
        var state = CreateState(groundItems, startPosition: new GridPosition(2, 2));

        var actions = pipeline.GetAvailableActions(state);

        Assert.DoesNotContain(actions, action => action.Kind == GameActionKind.Pickup);
    }

    [Fact]
    public void EquipIsAvailableForHeldEquipableItemsWithMatchingEmptySlots()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeItems.BaseballCap);
        state.Player.Inventory.Add(PrototypeItems.RunningShoes);

        var actions = pipeline.GetAvailableActions(state);

        Assert.Contains(actions, action => action.Kind == GameActionKind.EquipItem && action.Label == "Equip Baseball cap (Head)");
        Assert.Contains(actions, action => action.Kind == GameActionKind.EquipItem && action.Label == "Equip Running shoes (Feet)");
    }

    [Fact]
    public void EquipIsNotAvailableForItemsThatDoNotAllowEquip()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeItems.Stone);

        var actions = pipeline.GetAvailableActions(state);

        Assert.DoesNotContain(actions, action => action.Kind == GameActionKind.EquipItem && action.Label.Contains("Stone"));
    }

    [Fact]
    public void StackItemInspectAndDropActionsAreAvailableForInventoryStacks()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeItems.Stone, 3);

        var actions = pipeline.GetAvailableActions(state);

        Assert.Contains(actions, action =>
            action.Kind == GameActionKind.InspectItem
            && action.Request is InspectItemActionRequest request
            && request.ItemId == PrototypeItems.Stone
        );
        Assert.Contains(actions, action =>
            action.Kind == GameActionKind.DropItemStack
            && action.Request is DropItemStackActionRequest request
            && request.ItemId == PrototypeItems.Stone
            && request.Quantity == 1
        );
        Assert.Contains(actions, action =>
            action.Kind == GameActionKind.DropItemStack
            && action.Request is DropItemStackActionRequest request
            && request.ItemId == PrototypeItems.Stone
            && request.Quantity == 3
        );
    }

    [Fact]
    public void LegacyEquipmentCanBeInspectedAndUnequipped()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Equipment.OccupySlot(
            EquipmentSlotId.Head,
            new EquippedItemRef(PrototypeItems.BaseballCap, new ItemTypePath("Clothing", "Head", "Cap"))
        );

        var actions = pipeline.GetAvailableActions(state);

        Assert.Contains(actions, action =>
            action.Kind == GameActionKind.InspectItem
            && action.Request is InspectItemActionRequest request
            && request.ItemId == PrototypeItems.BaseballCap
        );
        Assert.Contains(actions, action =>
            action.Kind == GameActionKind.UnequipItem
            && action.Request is UnequipItemActionRequest request
            && request.SlotId == EquipmentSlotId.Head
        );
    }

    [Fact]
    public void WaitAdvancesTimeByOneHundredTicks()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();

        var result = pipeline.Execute(state, new WaitActionRequest());

        Assert.True(result.Succeeded);
        Assert.Equal(GameActionPipeline.WaitTickCost, result.ElapsedTicks);
        Assert.Equal(100, state.Time.ElapsedTicks);
        Assert.Contains("You wait. Time +100.", result.Messages);
    }

    [Fact]
    public void MoveAdvancesTimeByOneHundredTicksAndUpdatesPlayerPosition()
    {
        var pipeline = CreatePipeline();
        var state = CreateState(startPosition: new GridPosition(2, 2));

        var result = pipeline.Execute(state, new MoveActionRequest(GridOffset.Right));

        Assert.True(result.Succeeded);
        Assert.Equal(GameActionPipeline.MoveTickCost, result.ElapsedTicks);
        Assert.Equal(new GridPosition(3, 2), state.Player.Position);
        Assert.Equal(100, state.Time.ElapsedTicks);
        Assert.Contains("Moved to 3, 2. Time +100.", result.Messages);
    }

    [Fact]
    public void InvalidMoveDoesNotAdvanceTime()
    {
        var pipeline = CreatePipeline();
        var state = CreateState(startPosition: new GridPosition(0, 0));

        var result = pipeline.Execute(state, new MoveActionRequest(GridOffset.Left));

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(new GridPosition(0, 0), state.Player.Position);
        Assert.Equal(0, state.Time.ElapsedTicks);
    }

    [Fact]
    public void MoveFailsWithoutAdvancingTimeWhenBlockedByWorldObject()
    {
        var pipeline = CreatePipeline(CreateWorldObjectCatalog());
        var worldObjects = new TileObjectMap();
        worldObjects.Place(new GridPosition(3, 2), PrototypeWorldObjects.Wall);
        var state = CreateState(worldObjects: worldObjects, startPosition: new GridPosition(2, 2));

        var result = pipeline.Execute(state, new MoveActionRequest(GridOffset.Right));

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(new GridPosition(2, 2), state.Player.Position);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Contains("Blocked by Wall.", result.Messages);
    }

    [Fact]
    public void MoveFailsWithoutAdvancingTimeWhenBlockedByNpc()
    {
        var pipeline = CreatePipeline();
        var npcs = new NpcRoster();
        npcs.Add(new NpcState(PrototypeNpcs.TestDummy, "Test Dummy", new GridPosition(3, 2), 200, 200));
        var state = CreateState(npcs: npcs, startPosition: new GridPosition(2, 2));

        var result = pipeline.Execute(state, new MoveActionRequest(GridOffset.Right));

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(new GridPosition(2, 2), state.Player.Position);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Contains("Blocked by Test Dummy.", result.Messages);
    }

    [Fact]
    public void MoveSucceedsThroughNonBlockingWorldObject()
    {
        var pipeline = CreatePipeline(CreateWorldObjectCatalog());
        var worldObjects = new TileObjectMap();
        worldObjects.Place(new GridPosition(3, 2), PrototypeWorldObjects.Chair);
        var state = CreateState(worldObjects: worldObjects, startPosition: new GridPosition(2, 2));

        var result = pipeline.Execute(state, new MoveActionRequest(GridOffset.Right));

        Assert.True(result.Succeeded);
        Assert.Equal(new GridPosition(3, 2), state.Player.Position);
        Assert.Equal(100, state.Time.ElapsedTicks);
    }

    [Fact]
    public void PickupMovesGroundItemsIntoPlayerInventoryAndAdvancesTimeByFiftyTicks()
    {
        var pipeline = CreatePipeline();
        var groundItems = new TileItemMap();
        var position = new GridPosition(1, 1);
        groundItems.Place(position, PrototypeItems.Stone, 2);
        groundItems.Place(position, PrototypeItems.Branch);
        var state = CreateState(groundItems, startPosition: position);

        var result = pipeline.Execute(state, new PickupActionRequest());

        Assert.True(result.Succeeded);
        Assert.Equal(GameActionPipeline.PickupTickCost, result.ElapsedTicks);
        Assert.Equal(50, state.Time.ElapsedTicks);
        Assert.Equal(2, state.Player.Inventory.CountOf(PrototypeItems.Stone));
        Assert.Equal(1, state.Player.Inventory.CountOf(PrototypeItems.Branch));
        Assert.Empty(state.World.GroundItems.ItemsAt(position));
        Assert.Contains(result.Messages, message => message.Contains("Picked up 2 x Stone. Time +50."));
    }

    [Fact]
    public void PickupFailsWithoutAdvancingTimeWhenNoItemsArePresent()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();

        var result = pipeline.Execute(state, new PickupActionRequest());

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Contains("There is nothing here to pick up.", result.Messages);
    }

    [Fact]
    public void EquipMovesItemFromInventoryToSlotWithoutAdvancingTime()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeItems.BaseballCap);

        var result = pipeline.Execute(
            state,
            new EquipItemActionRequest(PrototypeItems.BaseballCap, EquipmentSlotId.Head)
        );

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(0, state.Player.Inventory.CountOf(PrototypeItems.BaseballCap));
        Assert.True(state.Player.Equipment.TryGetEquippedItem(EquipmentSlotId.Head, out var equippedItem));
        Assert.Equal(PrototypeItems.BaseballCap, equippedItem.ItemId);
        Assert.Contains("Equipped Baseball cap to Head.", result.Messages);
    }

    [Fact]
    public void EquipFailsWithoutAdvancingTimeWhenItemIsNotHeld()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();

        var result = pipeline.Execute(
            state,
            new EquipItemActionRequest(PrototypeItems.BaseballCap, EquipmentSlotId.Head)
        );

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.True(state.Player.Equipment.IsEmpty(EquipmentSlotId.Head));
    }

    [Fact]
    public void EquipFailsWithoutRemovingItemWhenSlotRejectsType()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeItems.RunningShoes);

        var result = pipeline.Execute(
            state,
            new EquipItemActionRequest(PrototypeItems.RunningShoes, EquipmentSlotId.Head)
        );

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(1, state.Player.Inventory.CountOf(PrototypeItems.RunningShoes));
        Assert.True(state.Player.Equipment.IsEmpty(EquipmentSlotId.Head));
    }

    [Fact]
    public void EquipFailsWithoutAdvancingTimeWhenSlotDoesNotExist()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeItems.BaseballCap);

        var result = pipeline.Execute(
            state,
            new EquipItemActionRequest(PrototypeItems.BaseballCap, new EquipmentSlotId("Tail"))
        );

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(1, state.Player.Inventory.CountOf(PrototypeItems.BaseballCap));
        Assert.True(state.Player.Equipment.IsEmpty(EquipmentSlotId.Head));
    }

    [Fact]
    public void EquipFailsWithoutRemovingItemWhenSlotIsOccupied()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeItems.BaseballCap);
        state.Player.Inventory.Add(new ItemId("motorcycle_helmet"));
        state.Player.Equipment.OccupySlot(
            EquipmentSlotId.Head,
            new EquippedItemRef(new ItemId("motorcycle_helmet"), new ItemTypePath("Armor", "Head", "Helmet"))
        );

        var result = pipeline.Execute(
            state,
            new EquipItemActionRequest(PrototypeItems.BaseballCap, EquipmentSlotId.Head)
        );

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(1, state.Player.Inventory.CountOf(PrototypeItems.BaseballCap));
        Assert.True(state.Player.Equipment.TryGetEquippedItem(EquipmentSlotId.Head, out var equippedItem));
        Assert.Equal(new ItemId("motorcycle_helmet"), equippedItem.ItemId);
    }

    [Fact]
    public void InspectStackItemReportsDetailsWithoutAdvancingTimeOrMutatingInventory()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeItems.Stone, 3);

        var result = pipeline.Execute(state, new InspectItemActionRequest(PrototypeItems.Stone));

        Assert.True(result.Succeeded);
        Assert.Equal(GameActionPipeline.InspectItemTickCost, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(3, state.Player.Inventory.CountOf(PrototypeItems.Stone));
        Assert.Contains("Stone - Material. Tags: none. Location: Inventory x3.", result.Messages);
    }

    [Fact]
    public void DropOneStackItemPreservesRemainingStackAndPlacesDroppedQuantityOnGround()
    {
        var pipeline = CreatePipeline();
        var state = CreateState(startPosition: new GridPosition(2, 2));
        state.Player.Inventory.Add(PrototypeItems.Stone, 3);

        var result = pipeline.Execute(state, new DropItemStackActionRequest(PrototypeItems.Stone, 1));

        Assert.True(result.Succeeded);
        Assert.Equal(GameActionPipeline.DropItemTickCost, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(2, state.Player.Inventory.CountOf(PrototypeItems.Stone));
        Assert.Contains(
            state.World.GroundItems.ItemsAt(new GridPosition(2, 2)),
            stack => stack.ItemId == PrototypeItems.Stone && stack.Quantity == 1
        );
        Assert.Contains("Dropped Stone.", result.Messages);
    }

    [Fact]
    public void DropAllStackItemsRemovesInventoryStackAndPlacesAllOnGround()
    {
        var pipeline = CreatePipeline();
        var state = CreateState(startPosition: new GridPosition(2, 2));
        state.Player.Inventory.Add(PrototypeItems.Stone, 3);

        var result = pipeline.Execute(state, new DropItemStackActionRequest(PrototypeItems.Stone, 3));

        Assert.True(result.Succeeded);
        Assert.Equal(0, state.Player.Inventory.CountOf(PrototypeItems.Stone));
        Assert.Contains(
            state.World.GroundItems.ItemsAt(new GridPosition(2, 2)),
            stack => stack.ItemId == PrototypeItems.Stone && stack.Quantity == 3
        );
        Assert.Contains("Dropped 3 x Stone.", result.Messages);
    }

    [Fact]
    public void DropStackItemFailsSafelyWhenQuantityIsUnavailable()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeItems.Stone, 2);

        var result = pipeline.Execute(state, new DropItemStackActionRequest(PrototypeItems.Stone, 3));

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(2, state.Player.Inventory.CountOf(PrototypeItems.Stone));
        Assert.Empty(state.World.GroundItems.ItemsAt(state.Player.Position));
    }

    [Fact]
    public void UnequipLegacyEquipmentReturnsItemToInventoryAndClearsSlot()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Equipment.OccupySlot(
            EquipmentSlotId.Head,
            new EquippedItemRef(PrototypeItems.BaseballCap, new ItemTypePath("Clothing", "Head", "Cap"))
        );

        var result = pipeline.Execute(state, new UnequipItemActionRequest(EquipmentSlotId.Head));

        Assert.True(result.Succeeded);
        Assert.Equal(GameActionPipeline.UnequipItemTickCost, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.True(state.Player.Equipment.IsEmpty(EquipmentSlotId.Head));
        Assert.Equal(1, state.Player.Inventory.CountOf(PrototypeItems.BaseballCap));
        Assert.Contains("Unequipped Baseball cap from Head.", result.Messages);
    }

    private static GameActionPipeline CreatePipeline(WorldObjectCatalog? worldObjectCatalog = null)
    {
        var catalog = new ItemCatalog();
        catalog.Add(new ItemDefinition(PrototypeItems.Stone, "Stone", "", "Material"));
        catalog.Add(new ItemDefinition(PrototypeItems.Branch, "Branch", "", "Material"));
        catalog.Add(new ItemDefinition(
            PrototypeItems.BaseballCap,
            "Baseball cap",
            "",
            "Clothing",
            new[] { "Head", "Cap", "BaseballCap" },
            actions: new[] { "equip" }
        ));
        catalog.Add(new ItemDefinition(
            PrototypeItems.RunningShoes,
            "Running shoes",
            "",
            "Clothing",
            new[] { "Feet", "Shoes", "RunningShoes" },
            actions: new[] { "equip" }
        ));
        catalog.Add(new ItemDefinition(
            new ItemId("motorcycle_helmet"),
            "Motorcycle helmet",
            "",
            "Armor",
            new[] { "Head", "Helmet", "MotorcycleHelmet" },
            actions: new[] { "equip" }
        ));

        return new GameActionPipeline(catalog, worldObjectCatalog);
    }

    private static WorldObjectCatalog CreateWorldObjectCatalog()
    {
        var catalog = new WorldObjectCatalog();
        catalog.Add(new WorldObjectDefinition(PrototypeWorldObjects.Wall, "Wall", "", "Structure", blocksMovement: true));
        catalog.Add(new WorldObjectDefinition(PrototypeWorldObjects.Chair, "Chair", "", "Furniture"));
        return catalog;
    }

    private static PrototypeGameState CreateState(
        TileItemMap? groundItems = null,
        TileObjectMap? worldObjects = null,
        NpcRoster? npcs = null,
        GridPosition? startPosition = null
    )
    {
        var bounds = new GridBounds(5, 5);
        return new PrototypeGameState(
            bounds,
            groundItems ?? new TileItemMap(),
            new TileSurfaceMap(bounds, PrototypeSurfaces.Concrete),
            worldObjects ?? new TileObjectMap(),
            npcs ?? new NpcRoster(),
            startPosition ?? new GridPosition(2, 2)
        );
    }
}
