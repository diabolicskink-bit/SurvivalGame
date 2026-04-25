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
    public void WaitAdvancesTurn()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();

        var result = pipeline.Execute(state, new WaitActionRequest());

        Assert.True(result.Succeeded);
        Assert.True(result.AdvancedTurn);
        Assert.Equal(1, state.Turn.CurrentTurn);
        Assert.Contains("Waited.", result.Messages);
    }

    [Fact]
    public void MoveAdvancesTurnAndUpdatesPlayerPosition()
    {
        var pipeline = CreatePipeline();
        var state = CreateState(startPosition: new GridPosition(2, 2));

        var result = pipeline.Execute(state, new MoveActionRequest(GridOffset.Right));

        Assert.True(result.Succeeded);
        Assert.Equal(new GridPosition(3, 2), state.Player.Position);
        Assert.Equal(1, state.Turn.CurrentTurn);
    }

    [Fact]
    public void InvalidMoveDoesNotAdvanceTurn()
    {
        var pipeline = CreatePipeline();
        var state = CreateState(startPosition: new GridPosition(0, 0));

        var result = pipeline.Execute(state, new MoveActionRequest(GridOffset.Left));

        Assert.False(result.Succeeded);
        Assert.Equal(new GridPosition(0, 0), state.Player.Position);
        Assert.Equal(0, state.Turn.CurrentTurn);
    }

    [Fact]
    public void MoveFailsWithoutAdvancingTurnWhenBlockedByWorldObject()
    {
        var pipeline = CreatePipeline(CreateWorldObjectCatalog());
        var worldObjects = new TileObjectMap();
        worldObjects.Place(new GridPosition(3, 2), PrototypeWorldObjects.Wall);
        var state = CreateState(worldObjects: worldObjects, startPosition: new GridPosition(2, 2));

        var result = pipeline.Execute(state, new MoveActionRequest(GridOffset.Right));

        Assert.False(result.Succeeded);
        Assert.Equal(new GridPosition(2, 2), state.Player.Position);
        Assert.Equal(0, state.Turn.CurrentTurn);
        Assert.Contains("Blocked by Wall.", result.Messages);
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
        Assert.Equal(1, state.Turn.CurrentTurn);
    }

    [Fact]
    public void PickupMovesGroundItemsIntoPlayerInventoryAndAdvancesTurn()
    {
        var pipeline = CreatePipeline();
        var groundItems = new TileItemMap();
        var position = new GridPosition(1, 1);
        groundItems.Place(position, PrototypeItems.Stone, 2);
        groundItems.Place(position, PrototypeItems.Branch);
        var state = CreateState(groundItems, startPosition: position);

        var result = pipeline.Execute(state, new PickupActionRequest());

        Assert.True(result.Succeeded);
        Assert.Equal(1, state.Turn.CurrentTurn);
        Assert.Equal(2, state.Player.Inventory.CountOf(PrototypeItems.Stone));
        Assert.Equal(1, state.Player.Inventory.CountOf(PrototypeItems.Branch));
        Assert.Empty(state.World.GroundItems.ItemsAt(position));
        Assert.Contains(result.Messages, message => message.Contains("Picked up Stone x2."));
    }

    [Fact]
    public void PickupFailsWithoutAdvancingTurnWhenNoItemsArePresent()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();

        var result = pipeline.Execute(state, new PickupActionRequest());

        Assert.False(result.Succeeded);
        Assert.Equal(0, state.Turn.CurrentTurn);
    }

    [Fact]
    public void EquipMovesItemFromInventoryToSlotWithoutAdvancingTurn()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeItems.BaseballCap);

        var result = pipeline.Execute(
            state,
            new EquipItemActionRequest(PrototypeItems.BaseballCap, EquipmentSlotId.Head)
        );

        Assert.True(result.Succeeded);
        Assert.False(result.AdvancedTurn);
        Assert.Equal(0, state.Turn.CurrentTurn);
        Assert.Equal(0, state.Player.Inventory.CountOf(PrototypeItems.BaseballCap));
        Assert.True(state.Player.Equipment.TryGetEquippedItem(EquipmentSlotId.Head, out var equippedItem));
        Assert.Equal(PrototypeItems.BaseballCap, equippedItem.ItemId);
        Assert.Contains("Equipped Baseball cap to Head.", result.Messages);
    }

    [Fact]
    public void EquipFailsWithoutAdvancingTurnWhenItemIsNotHeld()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();

        var result = pipeline.Execute(
            state,
            new EquipItemActionRequest(PrototypeItems.BaseballCap, EquipmentSlotId.Head)
        );

        Assert.False(result.Succeeded);
        Assert.Equal(0, state.Turn.CurrentTurn);
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
        Assert.Equal(0, state.Turn.CurrentTurn);
        Assert.Equal(1, state.Player.Inventory.CountOf(PrototypeItems.RunningShoes));
        Assert.True(state.Player.Equipment.IsEmpty(EquipmentSlotId.Head));
    }

    [Fact]
    public void EquipFailsWithoutAdvancingTurnWhenSlotDoesNotExist()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeItems.BaseballCap);

        var result = pipeline.Execute(
            state,
            new EquipItemActionRequest(PrototypeItems.BaseballCap, new EquipmentSlotId("Tail"))
        );

        Assert.False(result.Succeeded);
        Assert.Equal(0, state.Turn.CurrentTurn);
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
        Assert.Equal(0, state.Turn.CurrentTurn);
        Assert.Equal(1, state.Player.Inventory.CountOf(PrototypeItems.BaseballCap));
        Assert.True(state.Player.Equipment.TryGetEquippedItem(EquipmentSlotId.Head, out var equippedItem));
        Assert.Equal(new ItemId("motorcycle_helmet"), equippedItem.ItemId);
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
        GridPosition? startPosition = null
    )
    {
        return new PrototypeGameState(
            new GridBounds(5, 5),
            groundItems ?? new TileItemMap(),
            new TileSurfaceMap(new GridBounds(5, 5), PrototypeSurfaces.Concrete),
            worldObjects ?? new TileObjectMap(),
            startPosition ?? new GridPosition(2, 2)
        );
    }
}
