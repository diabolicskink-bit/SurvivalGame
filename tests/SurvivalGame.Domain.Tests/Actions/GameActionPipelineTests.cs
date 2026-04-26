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

        var result = pipeline.Execute(new WaitActionRequest(), state);

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

        var result = pipeline.Execute(new MoveActionRequest(GridOffset.Right), state);

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

        var result = pipeline.Execute(new MoveActionRequest(GridOffset.Left), state);

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

        var result = pipeline.Execute(new MoveActionRequest(GridOffset.Right), state);

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

        var result = pipeline.Execute(new MoveActionRequest(GridOffset.Right), state);

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

        var result = pipeline.Execute(new MoveActionRequest(GridOffset.Right), state);

        Assert.True(result.Succeeded);
        Assert.Equal(new GridPosition(3, 2), state.Player.Position);
        Assert.Equal(100, state.Time.ElapsedTicks);
    }

    [Fact]
    public void AutomatedTurretDoesNotFireOutsideRange()
    {
        var pipeline = CreatePipeline();
        var state = CreateState(
            npcs: CreateAutomatedTurretRoster(new GridPosition(9, 9)),
            startPosition: new GridPosition(0, 0),
            bounds: new GridBounds(10, 10)
        );

        var result = pipeline.Execute(new WaitActionRequest(), state);

        Assert.True(result.Succeeded);
        Assert.Equal(100, state.Time.ElapsedTicks);
        Assert.Equal(100, state.Player.Vitals.Health.Current);
        Assert.DoesNotContain(result.Messages, message => message.Contains("Automated turret"));
    }

    [Fact]
    public void AutomatedTurretFiresOnceWhenActionCrossesOneCadenceInRange()
    {
        var pipeline = CreatePipeline();
        var state = CreateState(
            npcs: CreateAutomatedTurretRoster(new GridPosition(3, 2)),
            startPosition: new GridPosition(2, 2)
        );

        var result = pipeline.Execute(new WaitActionRequest(), state);

        Assert.True(result.Succeeded);
        Assert.Equal(100, state.Time.ElapsedTicks);
        Assert.Equal(90, state.Player.Vitals.Health.Current);
        Assert.Contains(
            "Automated turret at 3, 2 hits you for 10 damage. Health: 90/100.",
            result.Messages
        );
    }

    [Fact]
    public void AutomatedTurretFiresForEveryCadenceBoundaryCrossedByAction()
    {
        var pipeline = CreatePipeline();
        var groundItems = new TileItemMap();
        groundItems.Place(new GridPosition(2, 2), PrototypeItems.Stone);
        var state = CreateState(
            groundItems,
            npcs: CreateAutomatedTurretRoster(new GridPosition(3, 2)),
            startPosition: new GridPosition(2, 2)
        );

        var pickupResult = pipeline.Execute(new PickupActionRequest(), state);

        Assert.True(pickupResult.Succeeded);
        Assert.Equal(50, state.Time.ElapsedTicks);
        Assert.Equal(100, state.Player.Vitals.Health.Current);

        var waitResult = pipeline.Execute(new WaitActionRequest(), state);

        Assert.True(waitResult.Succeeded);
        Assert.Equal(150, state.Time.ElapsedTicks);
        Assert.Equal(80, state.Player.Vitals.Health.Current);
        Assert.Equal(
            2,
            waitResult.Messages.Count(message => message.Contains("Automated turret at 3, 2 hits you"))
        );
    }

    [Fact]
    public void AutomatedTurretDoesNotFireAfterFailedOrZeroTickActions()
    {
        var pipeline = CreatePipeline(CreateWorldObjectCatalog());
        var worldObjects = new TileObjectMap();
        worldObjects.Place(new GridPosition(1, 2), PrototypeWorldObjects.Wall);
        var state = CreateState(
            worldObjects: worldObjects,
            npcs: CreateAutomatedTurretRoster(new GridPosition(3, 3)),
            startPosition: new GridPosition(2, 2)
        );
        state.Player.Inventory.Add(PrototypeItems.Stone);

        var failedMove = pipeline.Execute(new MoveActionRequest(GridOffset.Left), state);
        var inspect = pipeline.Execute(new InspectItemActionRequest(PrototypeItems.Stone), state);

        Assert.False(failedMove.Succeeded);
        Assert.True(inspect.Succeeded);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(100, state.Player.Vitals.Health.Current);
        Assert.DoesNotContain(failedMove.Messages, message => message.Contains("Automated turret"));
        Assert.DoesNotContain(inspect.Messages, message => message.Contains("Automated turret"));
    }

    [Fact]
    public void AutomatedTurretDoesNotFireWhenDisabled()
    {
        var pipeline = CreatePipeline();
        var state = CreateState(
            npcs: CreateAutomatedTurretRoster(new GridPosition(3, 2), disabled: true),
            startPosition: new GridPosition(2, 2)
        );

        var result = pipeline.Execute(new WaitActionRequest(), state);

        Assert.True(result.Succeeded);
        Assert.Equal(100, state.Time.ElapsedTicks);
        Assert.Equal(100, state.Player.Vitals.Health.Current);
        Assert.DoesNotContain(result.Messages, message => message.Contains("Automated turret"));
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

        var result = pipeline.Execute(new PickupActionRequest(), state);

        Assert.True(result.Succeeded);
        Assert.Equal(GameActionPipeline.PickupTickCost, result.ElapsedTicks);
        Assert.Equal(50, state.Time.ElapsedTicks);
        Assert.Equal(2, state.Player.Inventory.CountOf(PrototypeItems.Stone));
        Assert.Equal(1, state.Player.Inventory.CountOf(PrototypeItems.Branch));
        Assert.Empty(state.LocalMap.GroundItems.ItemsAt(position));
        Assert.Contains(result.Messages, message => message.Contains("Picked up 2 x Stone. Time +50."));
    }

    [Fact]
    public void PickupFailsWithoutAdvancingTimeWhenNoItemsArePresent()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();

        var result = pipeline.Execute(new PickupActionRequest(), state);

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Contains("There is nothing here to pick up.", result.Messages);
    }

    [Fact]
    public void PickupFailsWithoutTakingItemsWhenInventoryGridIsFull()
    {
        var pipeline = CreatePipeline();
        var position = new GridPosition(1, 1);
        var groundItems = new TileItemMap();
        groundItems.Place(position, PrototypeItems.Stone);
        var state = CreateState(groundItems, startPosition: position);
        for (var index = 0; index < 200; index++)
        {
            state.Player.Inventory.Add(new ItemId($"filler_{index}"));
        }

        var result = pipeline.Execute(new PickupActionRequest(), state);

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(0, state.Player.Inventory.CountOf(PrototypeItems.Stone));
        Assert.Contains(groundItems.ItemsAt(position), stack => stack.ItemId == PrototypeItems.Stone);
        Assert.Contains("Not enough inventory grid space.", result.Messages);
    }

    [Fact]
    public void PickupAllowsLooseAmmoWhenInventoryGridIsFull()
    {
        var pipeline = CreatePipeline();
        var position = new GridPosition(1, 1);
        var groundItems = new TileItemMap();
        groundItems.Place(position, PrototypeFirearms.Ammo9mmStandard, 30);
        var state = CreateState(groundItems, startPosition: position);
        for (var index = 0; index < 200; index++)
        {
            state.Player.Inventory.Add(new ItemId($"filler_{index}"));
        }

        var result = pipeline.Execute(new PickupActionRequest(), state);

        Assert.True(result.Succeeded);
        Assert.Equal(GameActionPipeline.PickupTickCost, result.ElapsedTicks);
        Assert.Equal(30, state.Player.Inventory.CountOf(PrototypeFirearms.Ammo9mmStandard));
        Assert.False(state.Player.Inventory.Container.Contains(ContainerItemRef.Stack(PrototypeFirearms.Ammo9mmStandard)));
        Assert.Empty(groundItems.ItemsAt(position));
    }

    [Fact]
    public void SearchContainerIsAvailableForAdjacentContainerObjects()
    {
        var pipeline = CreatePipeline(CreateWorldObjectCatalog());
        var worldObjects = new TileObjectMap();
        var containerId = new WorldObjectInstanceId("test_fridge_01");
        worldObjects.Place(
            new GridPosition(2, 1),
            PrototypeWorldObjects.Fridge,
            WorldObjectFacing.North,
            WorldObjectFootprint.SingleTile,
            instanceId: containerId
        );
        var state = CreateState(worldObjects: worldObjects, startPosition: new GridPosition(2, 2));

        var actions = pipeline.GetAvailableActions(state);

        Assert.Contains(actions, action =>
            action.Kind == GameActionKind.SearchContainer
            && action.Request is SearchContainerActionRequest request
            && request.ContainerId == containerId
        );
        Assert.Empty(state.LocalMap.ContainerStates.RealizedContainers);
    }

    [Fact]
    public void SearchContainerLazilyRealizesFixedLootAndAdvancesTime()
    {
        var pipeline = CreatePipeline(CreateWorldObjectCatalog());
        var containerId = new WorldObjectInstanceId("test_fridge_01");
        var worldObjects = new TileObjectMap();
        worldObjects.Place(
            new GridPosition(2, 1),
            PrototypeWorldObjects.Fridge,
            WorldObjectFacing.North,
            WorldObjectFootprint.SingleTile,
            instanceId: containerId,
            containerLoot: new WorldObjectContainerLootSpec(new[]
            {
                new GroundItemStack(new ItemId("canned_beans"), 2),
                new GroundItemStack(PrototypeItems.WaterBottle, 1)
            })
        );
        var state = CreateState(worldObjects: worldObjects, startPosition: new GridPosition(2, 2));

        var result = pipeline.Execute(new SearchContainerActionRequest(containerId), state);

        Assert.True(result.Succeeded);
        Assert.Equal(WorldObjectContainerDefinition.DefaultSearchTickCost, result.ElapsedTicks);
        Assert.Equal(WorldObjectContainerDefinition.DefaultSearchTickCost, state.Time.ElapsedTicks);
        var containerState = Assert.Single(state.LocalMap.ContainerStates.RealizedContainers);
        Assert.Equal(2, containerState.CountOf(new ItemId("canned_beans")));
        Assert.Equal(1, containerState.CountOf(PrototypeItems.WaterBottle));
        Assert.Equal(0, state.Player.Inventory.CountOf(new ItemId("canned_beans")));
        Assert.Contains("You search Fridge. You find 2 x Canned beans, Water bottle. Time +75.", result.Messages);
    }

    [Fact]
    public void SearchContainerWithNoFixedLootRealizesAsEmpty()
    {
        var pipeline = CreatePipeline(CreateWorldObjectCatalog());
        var containerId = new WorldObjectInstanceId("empty_fridge_01");
        var worldObjects = new TileObjectMap();
        worldObjects.Place(
            new GridPosition(2, 1),
            PrototypeWorldObjects.Fridge,
            WorldObjectFacing.North,
            WorldObjectFootprint.SingleTile,
            instanceId: containerId
        );
        var state = CreateState(worldObjects: worldObjects, startPosition: new GridPosition(2, 2));

        var result = pipeline.Execute(new SearchContainerActionRequest(containerId), state);

        Assert.True(result.Succeeded);
        var containerState = Assert.Single(state.LocalMap.ContainerStates.RealizedContainers);
        Assert.True(containerState.IsEmpty);
        Assert.Contains("You search Fridge. It is empty. Time +75.", result.Messages);
    }

    [Fact]
    public void TakeContainerItemMovesStackIntoInventoryAfterSearch()
    {
        var pipeline = CreatePipeline(CreateWorldObjectCatalog());
        var containerId = new WorldObjectInstanceId("test_fridge_01");
        var worldObjects = new TileObjectMap();
        worldObjects.Place(
            new GridPosition(2, 1),
            PrototypeWorldObjects.Fridge,
            WorldObjectFacing.North,
            WorldObjectFootprint.SingleTile,
            instanceId: containerId,
            containerLoot: new WorldObjectContainerLootSpec(new[]
            {
                new GroundItemStack(new ItemId("canned_beans"), 2)
            })
        );
        var state = CreateState(worldObjects: worldObjects, startPosition: new GridPosition(2, 2));
        pipeline.Execute(new SearchContainerActionRequest(containerId), state);

        var result = pipeline.Execute(
            new TakeContainerItemStackActionRequest(containerId, new ItemId("canned_beans"), 2),
            state
        );

        Assert.True(result.Succeeded);
        Assert.Equal(GameActionPipeline.PickupTickCost, result.ElapsedTicks);
        Assert.Equal(
            WorldObjectContainerDefinition.DefaultSearchTickCost + GameActionPipeline.PickupTickCost,
            state.Time.ElapsedTicks
        );
        Assert.Equal(2, state.Player.Inventory.CountOf(new ItemId("canned_beans")));
        Assert.True(Assert.Single(state.LocalMap.ContainerStates.RealizedContainers).IsEmpty);
        Assert.Contains("Took 2 x Canned beans from Fridge. Time +50.", result.Messages);
    }

    [Fact]
    public void TakeContainerItemFailsBeforeContainerIsSearched()
    {
        var pipeline = CreatePipeline(CreateWorldObjectCatalog());
        var containerId = new WorldObjectInstanceId("test_fridge_01");
        var worldObjects = new TileObjectMap();
        worldObjects.Place(
            new GridPosition(2, 1),
            PrototypeWorldObjects.Fridge,
            WorldObjectFacing.North,
            WorldObjectFootprint.SingleTile,
            instanceId: containerId,
            containerLoot: new WorldObjectContainerLootSpec(new[]
            {
                new GroundItemStack(new ItemId("canned_beans"), 1)
            })
        );
        var state = CreateState(worldObjects: worldObjects, startPosition: new GridPosition(2, 2));

        var result = pipeline.Execute(
            new TakeContainerItemStackActionRequest(containerId, new ItemId("canned_beans"), 1),
            state
        );

        Assert.False(result.Succeeded);
        Assert.Equal(0, state.Player.Inventory.CountOf(new ItemId("canned_beans")));
        Assert.Contains("Search Fridge first.", result.Messages);
    }

    [Fact]
    public void TakeContainerItemFailsWithoutRemovingLootWhenInventoryGridIsFull()
    {
        var pipeline = CreatePipeline(CreateWorldObjectCatalog());
        var containerId = new WorldObjectInstanceId("test_fridge_01");
        var worldObjects = new TileObjectMap();
        worldObjects.Place(
            new GridPosition(2, 1),
            PrototypeWorldObjects.Fridge,
            WorldObjectFacing.North,
            WorldObjectFootprint.SingleTile,
            instanceId: containerId,
            containerLoot: new WorldObjectContainerLootSpec(new[]
            {
                new GroundItemStack(new ItemId("canned_beans"), 1)
            })
        );
        var state = CreateState(worldObjects: worldObjects, startPosition: new GridPosition(2, 2));
        pipeline.Execute(new SearchContainerActionRequest(containerId), state);
        for (var index = 0; index < 200; index++)
        {
            state.Player.Inventory.Add(new ItemId($"filler_{index}"));
        }

        var result = pipeline.Execute(
            new TakeContainerItemStackActionRequest(containerId, new ItemId("canned_beans"), 1),
            state
        );

        Assert.False(result.Succeeded);
        Assert.Equal(0, state.Player.Inventory.CountOf(new ItemId("canned_beans")));
        Assert.Equal(1, Assert.Single(state.LocalMap.ContainerStates.RealizedContainers).CountOf(new ItemId("canned_beans")));
        Assert.Contains("Not enough inventory grid space.", result.Messages);
    }

    [Fact]
    public void EquipMovesItemFromInventoryToSlotWithoutAdvancingTime()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeItems.BaseballCap);

        var result = pipeline.Execute(new EquipItemActionRequest(PrototypeItems.BaseballCap, EquipmentSlotId.Head), state
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

        var result = pipeline.Execute(new EquipItemActionRequest(PrototypeItems.BaseballCap, EquipmentSlotId.Head), state
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

        var result = pipeline.Execute(new EquipItemActionRequest(PrototypeItems.RunningShoes, EquipmentSlotId.Head), state
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

        var result = pipeline.Execute(new EquipItemActionRequest(PrototypeItems.BaseballCap, new EquipmentSlotId("Tail")), state
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

        var result = pipeline.Execute(new EquipItemActionRequest(PrototypeItems.BaseballCap, EquipmentSlotId.Head), state
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

        var result = pipeline.Execute(new InspectItemActionRequest(PrototypeItems.Stone), state);

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

        var result = pipeline.Execute(new DropItemStackActionRequest(PrototypeItems.Stone, 1), state);

        Assert.True(result.Succeeded);
        Assert.Equal(GameActionPipeline.DropItemTickCost, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(2, state.Player.Inventory.CountOf(PrototypeItems.Stone));
        Assert.Contains(
            state.LocalMap.GroundItems.ItemsAt(new GridPosition(2, 2)),
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

        var result = pipeline.Execute(new DropItemStackActionRequest(PrototypeItems.Stone, 3), state);

        Assert.True(result.Succeeded);
        Assert.Equal(0, state.Player.Inventory.CountOf(PrototypeItems.Stone));
        Assert.Contains(
            state.LocalMap.GroundItems.ItemsAt(new GridPosition(2, 2)),
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

        var result = pipeline.Execute(new DropItemStackActionRequest(PrototypeItems.Stone, 3), state);

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(2, state.Player.Inventory.CountOf(PrototypeItems.Stone));
        Assert.Empty(state.LocalMap.GroundItems.ItemsAt(state.Player.Position));
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

        var result = pipeline.Execute(new UnequipItemActionRequest(EquipmentSlotId.Head), state);

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
        catalog.Add(new ItemDefinition(new ItemId("canned_beans"), "Canned beans", "", "Food"));
        catalog.Add(new ItemDefinition(
            PrototypeItems.WaterBottle,
            "Water bottle",
            "",
            "Food",
            inventorySize: new InventoryItemSize(1, 2)
        ));
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
        catalog.Add(new ItemDefinition(
            PrototypeFirearms.Ammo9mmStandard,
            "9mm standard rounds",
            "",
            "Ammunition"
        ));

        return new GameActionPipeline(catalog, worldObjectCatalog, npcCatalog: CreateNpcCatalog());
    }

    private static WorldObjectCatalog CreateWorldObjectCatalog()
    {
        var catalog = new WorldObjectCatalog();
        catalog.Add(new WorldObjectDefinition(PrototypeWorldObjects.Wall, "Wall", "", "Structure", blocksMovement: true));
        catalog.Add(new WorldObjectDefinition(PrototypeWorldObjects.Chair, "Chair", "", "Furniture"));
        catalog.Add(new WorldObjectDefinition(
            PrototypeWorldObjects.Fridge,
            "Fridge",
            "",
            "Appliance",
            blocksMovement: true,
            container: new WorldObjectContainerDefinition("fridge_basic")
        ));
        return catalog;
    }

    private static NpcCatalog CreateNpcCatalog()
    {
        var catalog = new NpcCatalog();
        catalog.Add(new NpcDefinition(
            PrototypeNpcs.AutomatedTurretDefinition,
            "Automated turret",
            "",
            "Machine",
            maximumHealth: 120,
            tags: new[] { "turret" },
            behavior: new NpcBehaviorProfile(
                NpcBehaviorKind.Inert,
                GameActionPipeline.AutomatedTurretRangeTiles,
                new[] { "automated_hazard" }
            )
        ));

        return catalog;
    }

    private static NpcRoster CreateAutomatedTurretRoster(GridPosition position, bool disabled = false)
    {
        var roster = new NpcRoster();
        roster.Add(new NpcState(
            PrototypeNpcs.GasStationTurret,
            PrototypeNpcs.AutomatedTurretDefinition,
            "Automated turret",
            position,
            currentHealth: disabled ? 0 : 120,
            maximumHealth: 120
        ));

        return roster;
    }

    private static PrototypeGameState CreateState(
        TileItemMap? groundItems = null,
        TileObjectMap? worldObjects = null,
        NpcRoster? npcs = null,
        GridPosition? startPosition = null,
        GridBounds? bounds = null
    )
    {
        var mapBounds = bounds ?? new GridBounds(5, 5);
        return new PrototypeGameState(
            mapBounds,
            groundItems ?? new TileItemMap(),
            new TileSurfaceMap(mapBounds, PrototypeSurfaces.Concrete),
            worldObjects ?? new TileObjectMap(),
            npcs ?? new NpcRoster(),
            startPosition ?? new GridPosition(2, 2)
        );
    }
}
