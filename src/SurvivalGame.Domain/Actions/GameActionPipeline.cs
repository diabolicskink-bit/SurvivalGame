namespace SurvivalGame.Domain;

public enum GameActionKind
{
    Wait,
    Move,
    Pickup,
    InspectItem,
    DropItemStack,
    EquipItem,
    UnequipItem,
    LoadFeedDevice,
    UnloadFeedDevice,
    InsertFeedDevice,
    RemoveFeedDevice,
    LoadWeapon,
    ReloadWeapon,
    TestFire,
    PickupStatefulItem,
    DropStatefulItem,
    InspectStatefulItem,
    EquipStatefulItem,
    UnequipStatefulItem,
    LoadStatefulFeedDevice,
    UnloadStatefulFeedDevice,
    InsertStatefulFeedDevice,
    RemoveStatefulFeedDevice,
    LoadStatefulWeapon,
    ReloadStatefulWeapon,
    TestFireStatefulWeapon,
    ShootNpc,
    RefuelVehicle
}

public abstract record GameActionRequest(GameActionKind Kind);

public sealed record WaitActionRequest() : GameActionRequest(GameActionKind.Wait);

public sealed record MoveActionRequest(GridOffset Direction) : GameActionRequest(GameActionKind.Move);

public sealed record PickupActionRequest() : GameActionRequest(GameActionKind.Pickup);

public sealed record InspectItemActionRequest(ItemId ItemId)
    : GameActionRequest(GameActionKind.InspectItem);

public sealed record DropItemStackActionRequest(ItemId ItemId, int Quantity)
    : GameActionRequest(GameActionKind.DropItemStack);

public sealed record EquipItemActionRequest(ItemId ItemId, EquipmentSlotId SlotId) : GameActionRequest(GameActionKind.EquipItem);

public sealed record UnequipItemActionRequest(EquipmentSlotId SlotId)
    : GameActionRequest(GameActionKind.UnequipItem);

public sealed record LoadFeedDeviceActionRequest(ItemId FeedDeviceItemId, ItemId AmmunitionItemId)
    : GameActionRequest(GameActionKind.LoadFeedDevice);

public sealed record UnloadFeedDeviceActionRequest(ItemId FeedDeviceItemId)
    : GameActionRequest(GameActionKind.UnloadFeedDevice);

public sealed record InsertFeedDeviceActionRequest(ItemId WeaponItemId, ItemId FeedDeviceItemId)
    : GameActionRequest(GameActionKind.InsertFeedDevice);

public sealed record RemoveFeedDeviceActionRequest(ItemId WeaponItemId)
    : GameActionRequest(GameActionKind.RemoveFeedDevice);

public sealed record LoadWeaponActionRequest(ItemId WeaponItemId, ItemId AmmunitionItemId)
    : GameActionRequest(GameActionKind.LoadWeapon);

public sealed record ReloadWeaponActionRequest(ItemId WeaponItemId, ItemId AmmunitionItemId)
    : GameActionRequest(GameActionKind.ReloadWeapon);

public sealed record TestFireActionRequest(ItemId WeaponItemId)
    : GameActionRequest(GameActionKind.TestFire);

public sealed record PickupStatefulItemActionRequest(StatefulItemId ItemId)
    : GameActionRequest(GameActionKind.PickupStatefulItem);

public sealed record DropStatefulItemActionRequest(StatefulItemId ItemId)
    : GameActionRequest(GameActionKind.DropStatefulItem);

public sealed record InspectStatefulItemActionRequest(StatefulItemId ItemId)
    : GameActionRequest(GameActionKind.InspectStatefulItem);

public sealed record EquipStatefulItemActionRequest(StatefulItemId ItemId, EquipmentSlotId SlotId)
    : GameActionRequest(GameActionKind.EquipStatefulItem);

public sealed record UnequipStatefulItemActionRequest(StatefulItemId ItemId)
    : GameActionRequest(GameActionKind.UnequipStatefulItem);

public sealed record LoadStatefulFeedDeviceActionRequest(StatefulItemId FeedDeviceItemId, ItemId AmmunitionItemId)
    : GameActionRequest(GameActionKind.LoadStatefulFeedDevice);

public sealed record UnloadStatefulFeedDeviceActionRequest(StatefulItemId FeedDeviceItemId)
    : GameActionRequest(GameActionKind.UnloadStatefulFeedDevice);

public sealed record InsertStatefulFeedDeviceActionRequest(StatefulItemId WeaponItemId, StatefulItemId FeedDeviceItemId)
    : GameActionRequest(GameActionKind.InsertStatefulFeedDevice);

public sealed record RemoveStatefulFeedDeviceActionRequest(StatefulItemId WeaponItemId)
    : GameActionRequest(GameActionKind.RemoveStatefulFeedDevice);

public sealed record LoadStatefulWeaponActionRequest(StatefulItemId WeaponItemId, ItemId AmmunitionItemId)
    : GameActionRequest(GameActionKind.LoadStatefulWeapon);

public sealed record ReloadStatefulWeaponActionRequest(StatefulItemId WeaponItemId, ItemId AmmunitionItemId)
    : GameActionRequest(GameActionKind.ReloadStatefulWeapon);

public sealed record TestFireStatefulWeaponActionRequest(StatefulItemId WeaponItemId)
    : GameActionRequest(GameActionKind.TestFireStatefulWeapon);

public sealed record ShootNpcActionRequest(NpcId TargetNpcId)
    : GameActionRequest(GameActionKind.ShootNpc);

public sealed record RefuelVehicleActionRequest() : GameActionRequest(GameActionKind.RefuelVehicle);

public sealed record AvailableAction(GameActionKind Kind, string Label, GameActionRequest? Request = null);

public sealed record GameActionResult(bool Succeeded, int ElapsedTicks, IReadOnlyList<string> Messages)
{
    public static GameActionResult Success(int elapsedTicks, params string[] messages)
    {
        if (elapsedTicks < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedTicks), "Elapsed tick cost cannot be negative.");
        }

        return new GameActionResult(true, elapsedTicks, messages);
    }

    public static GameActionResult Failure(params string[] messages)
    {
        return new GameActionResult(false, 0, messages);
    }
}

public sealed class GameActionPipeline
{
    public const int MoveTickCost = 100;
    public const int WaitTickCost = 100;
    public const int PickupTickCost = 50;
    public const int InspectItemTickCost = 0;
    public const int DropItemTickCost = 0;
    public const int EquipItemTickCost = 0;
    public const int UnequipItemTickCost = 0;
    public const int ShootTickCost = 100;
    public const int RefuelVehicleTickCost = 100;
    public const int AutomatedTurretRangeTiles = 5;
    public const int AutomatedTurretTickInterval = 75;
    public const int AutomatedTurretDamage = 10;

    private readonly ItemCatalog _itemCatalog;
    private readonly WorldObjectCatalog? _worldObjectCatalog;
    private readonly FirearmActionService? _firearmActions;
    private readonly VehicleFuelState? _vehicleFuelState;

    public GameActionPipeline(
        ItemCatalog itemCatalog,
        WorldObjectCatalog? worldObjectCatalog = null,
        FirearmCatalog? firearmCatalog = null,
        VehicleFuelState? vehicleFuelState = null
    )
    {
        ArgumentNullException.ThrowIfNull(itemCatalog);
        _itemCatalog = itemCatalog;
        _worldObjectCatalog = worldObjectCatalog;
        _firearmActions = firearmCatalog is null ? null : new FirearmActionService(firearmCatalog, itemCatalog);
        _vehicleFuelState = vehicleFuelState;
    }

    public IReadOnlyList<AvailableAction> GetAvailableActions(PrototypeGameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var actions = new List<AvailableAction>
        {
            new(GameActionKind.Wait, "Wait", new WaitActionRequest())
        };

        if (state.LocalMap.GroundItems.ItemsAt(state.Player.Position).Count > 0)
        {
            actions.Add(new AvailableAction(GameActionKind.Pickup, "Pick Up", new PickupActionRequest()));
        }

        if (CanRefuelVehicle(state))
        {
            actions.Add(new AvailableAction(
                GameActionKind.RefuelVehicle,
                "Refuel Vehicle",
                new RefuelVehicleActionRequest()
            ));
        }

        actions.AddRange(GetAvailableStatefulItemActions(state));
        actions.AddRange(GetAvailableStackItemActions(state));
        actions.AddRange(GetAvailableEquipActions(state));
        if (_firearmActions is not null)
        {
            actions.AddRange(_firearmActions.GetAvailableActions(state));
            actions.AddRange(_firearmActions.GetAvailableStatefulActions(state, _itemCatalog));
        }

        return actions;
    }

    public GameActionResult Execute(PrototypeGameState state, GameActionRequest request)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(request);

        var startingElapsedTicks = state.Time.ElapsedTicks;
        var result = request switch
        {
            WaitActionRequest => Wait(state),
            MoveActionRequest move => Move(state, move.Direction),
            PickupActionRequest => Pickup(state),
            InspectItemActionRequest inspect => InspectItem(state, inspect.ItemId),
            DropItemStackActionRequest dropStack => DropItemStack(state, dropStack.ItemId, dropStack.Quantity),
            EquipItemActionRequest equip => EquipItem(state, equip.ItemId, equip.SlotId),
            UnequipItemActionRequest unequip => UnequipItem(state, unequip.SlotId),
            LoadFeedDeviceActionRequest loadFeed => ExecuteFirearmAction(
                state,
                service => service.LoadFeedDevice(state, loadFeed.FeedDeviceItemId, loadFeed.AmmunitionItemId)
            ),
            UnloadFeedDeviceActionRequest unloadFeed => ExecuteFirearmAction(
                state,
                service => service.UnloadFeedDevice(state, unloadFeed.FeedDeviceItemId)
            ),
            InsertFeedDeviceActionRequest insertFeed => ExecuteFirearmAction(
                state,
                service => service.InsertFeedDevice(state, insertFeed.WeaponItemId, insertFeed.FeedDeviceItemId)
            ),
            RemoveFeedDeviceActionRequest removeFeed => ExecuteFirearmAction(
                state,
                service => service.RemoveFeedDevice(state, removeFeed.WeaponItemId)
            ),
            LoadWeaponActionRequest loadWeapon => ExecuteFirearmAction(
                state,
                service => service.LoadWeapon(state, loadWeapon.WeaponItemId, loadWeapon.AmmunitionItemId)
            ),
            ReloadWeaponActionRequest reloadWeapon => ExecuteFirearmAction(
                state,
                service => service.ReloadWeapon(state, reloadWeapon.WeaponItemId, reloadWeapon.AmmunitionItemId)
            ),
            TestFireActionRequest testFire => ExecuteFirearmAction(
                state,
                service => service.TestFire(state, testFire.WeaponItemId)
            ),
            PickupStatefulItemActionRequest pickupStateful => PickupStatefulItem(state, pickupStateful.ItemId),
            DropStatefulItemActionRequest dropStateful => DropStatefulItem(state, dropStateful.ItemId),
            InspectStatefulItemActionRequest inspectStateful => InspectStatefulItem(state, inspectStateful.ItemId),
            EquipStatefulItemActionRequest equipStateful => EquipStatefulItem(state, equipStateful.ItemId, equipStateful.SlotId),
            UnequipStatefulItemActionRequest unequipStateful => UnequipStatefulItem(state, unequipStateful.ItemId),
            LoadStatefulFeedDeviceActionRequest loadStatefulFeed => ExecuteFirearmAction(
                state,
                service => service.LoadStatefulFeedDevice(state, loadStatefulFeed.FeedDeviceItemId, loadStatefulFeed.AmmunitionItemId)
            ),
            UnloadStatefulFeedDeviceActionRequest unloadStatefulFeed => ExecuteFirearmAction(
                state,
                service => service.UnloadStatefulFeedDevice(state, unloadStatefulFeed.FeedDeviceItemId)
            ),
            InsertStatefulFeedDeviceActionRequest insertStatefulFeed => ExecuteFirearmAction(
                state,
                service => service.InsertStatefulFeedDevice(state, insertStatefulFeed.WeaponItemId, insertStatefulFeed.FeedDeviceItemId)
            ),
            RemoveStatefulFeedDeviceActionRequest removeStatefulFeed => ExecuteFirearmAction(
                state,
                service => service.RemoveStatefulFeedDevice(state, removeStatefulFeed.WeaponItemId)
            ),
            LoadStatefulWeaponActionRequest loadStatefulWeapon => ExecuteFirearmAction(
                state,
                service => service.LoadStatefulWeapon(state, loadStatefulWeapon.WeaponItemId, loadStatefulWeapon.AmmunitionItemId)
            ),
            ReloadStatefulWeaponActionRequest reloadStatefulWeapon => ExecuteFirearmAction(
                state,
                service => service.ReloadStatefulWeapon(state, reloadStatefulWeapon.WeaponItemId, reloadStatefulWeapon.AmmunitionItemId)
            ),
            TestFireStatefulWeaponActionRequest testStatefulWeapon => ExecuteFirearmAction(
                state,
                service => service.TestFireStatefulWeapon(state, testStatefulWeapon.WeaponItemId)
            ),
            ShootNpcActionRequest shootNpc => ShootNpc(state, shootNpc.TargetNpcId),
            RefuelVehicleActionRequest => RefuelVehicle(state),
            _ => GameActionResult.Failure("That action is not supported.")
        };

        return ResolveAutomatedTurretFire(state, startingElapsedTicks, result);
    }

    private static GameActionResult ResolveAutomatedTurretFire(
        PrototypeGameState state,
        int startingElapsedTicks,
        GameActionResult result)
    {
        if (!result.Succeeded || result.ElapsedTicks <= 0)
        {
            return result;
        }

        var crossedIntervals = CountCrossedTurretIntervals(startingElapsedTicks, state.Time.ElapsedTicks);
        if (crossedIntervals <= 0)
        {
            return result;
        }

        var turretsInRange = state.LocalMap.Npcs.AllNpcs
            .Where(npc => npc.DefinitionId == PrototypeNpcs.AutomatedTurretDefinition
                && !npc.IsDisabled
                && TileDistance(npc.Position, state.Player.Position) <= AutomatedTurretRangeTiles)
            .ToArray();
        if (turretsInRange.Length == 0)
        {
            return result;
        }

        var messages = result.Messages.ToList();
        foreach (var turret in turretsInRange)
        {
            for (var shot = 0; shot < crossedIntervals; shot++)
            {
                var dealtDamage = state.Player.Vitals.TakeDamage(AutomatedTurretDamage);
                messages.Add(
                    $"Automated turret at {turret.Position.X}, {turret.Position.Y} hits you for {dealtDamage} damage. "
                    + $"Health: {state.Player.Vitals.Health.Current}/{state.Player.Vitals.Health.Maximum}."
                );
            }
        }

        return new GameActionResult(result.Succeeded, result.ElapsedTicks, messages);
    }

    private static int CountCrossedTurretIntervals(int startingElapsedTicks, int endingElapsedTicks)
    {
        if (endingElapsedTicks <= startingElapsedTicks)
        {
            return 0;
        }

        return (endingElapsedTicks / AutomatedTurretTickInterval)
            - (startingElapsedTicks / AutomatedTurretTickInterval);
    }

    private static int TileDistance(GridPosition from, GridPosition to)
    {
        return Math.Max(Math.Abs(from.X - to.X), Math.Abs(from.Y - to.Y));
    }

    private GameActionResult ExecuteFirearmAction(
        PrototypeGameState state,
        Func<FirearmActionService, GameActionResult> action)
    {
        if (_firearmActions is null)
        {
            return GameActionResult.Failure("Firearm actions are not available.");
        }

        var result = action(_firearmActions);
        if (!result.Succeeded || result.ElapsedTicks == 0)
        {
            if (result.Succeeded)
            {
                SynchronizeStatefulInventoryPlacements(state);
            }

            return result;
        }

        state.AdvanceTime(result.ElapsedTicks);
        SynchronizeStatefulInventoryPlacements(state);
        return GameActionResult.Success(
            result.ElapsedTicks,
            result.Messages.Concat(new[] { $"Time +{result.ElapsedTicks}." }).ToArray()
        );
    }

    private GameActionResult RefuelVehicle(PrototypeGameState state)
    {
        if (_vehicleFuelState is null)
        {
            return GameActionResult.Failure("No vehicle fuel state is available.");
        }

        if (_vehicleFuelState.IsFull)
        {
            return GameActionResult.Failure("Vehicle fuel is already full.");
        }

        if (!IsAdjacentToFuelPump(state))
        {
            return GameActionResult.Failure("You need to stand next to a fuel pump.");
        }

        _vehicleFuelState.Refill();
        state.AdvanceTime(RefuelVehicleTickCost);

        return GameActionResult.Success(
            RefuelVehicleTickCost,
            $"Refueled vehicle to {_vehicleFuelState.CurrentFuel:0.0}/{_vehicleFuelState.Capacity:0.0}. Time +{RefuelVehicleTickCost}."
        );
    }

    private GameActionResult ShootNpc(PrototypeGameState state, NpcId targetNpcId)
    {
        if (_firearmActions is null)
        {
            return GameActionResult.Failure("Firearm actions are not available.");
        }

        var result = _firearmActions.ShootEquippedNpc(state, targetNpcId);
        if (!result.Succeeded)
        {
            return result;
        }

        state.AdvanceTime(ShootTickCost);
        return GameActionResult.Success(
            ShootTickCost,
            result.Messages.Concat(new[] { $"Time +{ShootTickCost}." }).ToArray()
        );
    }

    private IEnumerable<AvailableAction> GetAvailableEquipActions(PrototypeGameState state)
    {
        foreach (var stack in state.Player.Inventory.Items)
        {
            if (!_itemCatalog.TryGet(stack.ItemId, out var item) || !item.AllowsAction("equip"))
            {
                continue;
            }

            foreach (var slot in state.Player.Equipment.Slots)
            {
                if (!state.Player.Equipment.IsEmpty(slot.Id) || !slot.Accepts(item.TypePath))
                {
                    continue;
                }

                yield return new AvailableAction(
                    GameActionKind.EquipItem,
                    $"Equip {item.Name} ({slot.DisplayName})",
                    new EquipItemActionRequest(item.Id, slot.Id)
                );
            }
        }
    }

    private IEnumerable<AvailableAction> GetAvailableStackItemActions(PrototypeGameState state)
    {
        foreach (var stack in state.Player.Inventory.Items)
        {
            var itemName = GetItemName(stack.ItemId);
            yield return new AvailableAction(
                GameActionKind.InspectItem,
                $"Inspect {itemName}",
                new InspectItemActionRequest(stack.ItemId)
            );

            yield return new AvailableAction(
                GameActionKind.DropItemStack,
                $"Drop one {itemName}",
                new DropItemStackActionRequest(stack.ItemId, 1)
            );

            if (stack.Quantity > 1)
            {
                yield return new AvailableAction(
                    GameActionKind.DropItemStack,
                    $"Drop all {stack.Quantity} {itemName}",
                    new DropItemStackActionRequest(stack.ItemId, stack.Quantity)
                );
            }
        }

        foreach (var slot in state.Player.Equipment.Slots)
        {
            if (!state.Player.Equipment.TryGetEquippedItem(slot.Id, out var equippedItem))
            {
                continue;
            }

            var itemName = GetItemName(equippedItem.ItemId);
            yield return new AvailableAction(
                GameActionKind.InspectItem,
                $"Inspect {itemName}",
                new InspectItemActionRequest(equippedItem.ItemId)
            );
            yield return new AvailableAction(
                GameActionKind.UnequipItem,
                $"Unequip {itemName}",
                new UnequipItemActionRequest(slot.Id)
            );
        }
    }

    private IEnumerable<AvailableAction> GetAvailableStatefulItemActions(PrototypeGameState state)
    {
        foreach (var item in state.StatefulItems.OnGround(state.Player.Position, state.SiteId))
        {
            yield return new AvailableAction(
                GameActionKind.PickupStatefulItem,
                $"Pick up {FormatStatefulItem(item)}",
                new PickupStatefulItemActionRequest(item.Id)
            );
            yield return new AvailableAction(
                GameActionKind.InspectStatefulItem,
                $"Inspect {FormatStatefulItem(item)}",
                new InspectStatefulItemActionRequest(item.Id)
            );
        }

        foreach (var item in state.StatefulItems.InPlayerInventory())
        {
            yield return new AvailableAction(
                GameActionKind.InspectStatefulItem,
                $"Inspect {FormatStatefulItem(item)}",
                new InspectStatefulItemActionRequest(item.Id)
            );
            yield return new AvailableAction(
                GameActionKind.DropStatefulItem,
                $"Drop {FormatStatefulItem(item)}",
                new DropStatefulItemActionRequest(item.Id)
            );

            if (!_itemCatalog.TryGet(item.ItemId, out var definition) || !definition.AllowsAction("equip"))
            {
                continue;
            }

            foreach (var slot in state.Player.Equipment.Slots)
            {
                if (!IsSlotFree(state, slot.Id) || !slot.Accepts(definition.TypePath))
                {
                    continue;
                }

                yield return new AvailableAction(
                    GameActionKind.EquipStatefulItem,
                    $"Equip {FormatStatefulItem(item)} ({slot.DisplayName})",
                    new EquipStatefulItemActionRequest(item.Id, slot.Id)
                );
            }
        }

        foreach (var item in state.StatefulItems.Equipped())
        {
            yield return new AvailableAction(
                GameActionKind.InspectStatefulItem,
                $"Inspect {FormatStatefulItem(item)}",
                new InspectStatefulItemActionRequest(item.Id)
            );
            yield return new AvailableAction(
                GameActionKind.UnequipStatefulItem,
                $"Unequip {FormatStatefulItem(item)}",
                new UnequipStatefulItemActionRequest(item.Id)
            );
        }
    }

    private static GameActionResult Wait(PrototypeGameState state)
    {
        state.AdvanceTime(WaitTickCost);
        return GameActionResult.Success(WaitTickCost, $"You wait. Time +{WaitTickCost}.");
    }

    private GameActionResult Move(PrototypeGameState state, GridOffset direction)
    {
        if (direction == GridOffset.Zero)
        {
            return GameActionResult.Failure("No movement direction selected.");
        }

        var nextPosition = state.Player.Position + direction;
        if (!state.LocalMap.Map.Contains(nextPosition))
        {
            return GameActionResult.Failure("Cannot move there.");
        }

        if (IsBlockedByWorldObject(state, nextPosition, out var blockerName))
        {
            return GameActionResult.Failure($"Blocked by {blockerName}.");
        }

        if (IsBlockedByNpc(state, nextPosition, out var npcName))
        {
            return GameActionResult.Failure($"Blocked by {npcName}.");
        }

        state.SetPlayerPosition(nextPosition);
        state.AdvanceTime(MoveTickCost);
        return GameActionResult.Success(MoveTickCost, $"Moved to {nextPosition.X}, {nextPosition.Y}. Time +{MoveTickCost}.");
    }

    private bool IsBlockedByWorldObject(PrototypeGameState state, GridPosition position, out string blockerName)
    {
        blockerName = "something";

        if (!state.LocalMap.WorldObjects.TryGetObjectAt(position, out var objectId))
        {
            return false;
        }

        if (_worldObjectCatalog is null || !_worldObjectCatalog.TryGet(objectId, out var worldObject))
        {
            blockerName = objectId.ToString();
            return true;
        }

        blockerName = worldObject.Name;
        return worldObject.BlocksMovement;
    }

    private static bool IsBlockedByNpc(PrototypeGameState state, GridPosition position, out string npcName)
    {
        if (state.LocalMap.Npcs.TryGetAt(position, out var npc) && npc.BlocksMovement)
        {
            npcName = npc.Name;
            return true;
        }

        npcName = string.Empty;
        return false;
    }

    private GameActionResult Pickup(PrototypeGameState state)
    {
        var availableStacks = state.LocalMap.GroundItems.ItemsAt(state.Player.Position);
        if (availableStacks.Count == 0)
        {
            return GameActionResult.Failure("There is nothing here to pick up.");
        }

        var requiredSpaces = availableStacks.Select(stack => (
            stack.ItemId,
            GetInventorySize(stack.ItemId),
            UsesGrid: UsesInventoryGrid(stack.ItemId)
        ));
        if (!state.Player.Inventory.CanAddAll(requiredSpaces))
        {
            return GameActionResult.Failure("Not enough inventory grid space.");
        }

        var itemStacks = state.LocalMap.GroundItems.TakeAllAt(state.Player.Position);
        if (itemStacks.Count == 0)
        {
            return GameActionResult.Failure("There is nothing here to pick up.");
        }

        foreach (var stack in itemStacks)
        {
            state.Player.Inventory.Add(
                stack.ItemId,
                stack.Quantity,
                GetInventorySize(stack.ItemId),
                UsesInventoryGrid(stack.ItemId)
            );
        }

        state.AdvanceTime(PickupTickCost);
        var messages = itemStacks
            .Select(stack => $"Picked up {FormatStack(stack)}. Time +{PickupTickCost}.")
            .ToArray();

        return GameActionResult.Success(PickupTickCost, messages);
    }

    private GameActionResult PickupStatefulItem(PrototypeGameState state, StatefulItemId itemId)
    {
        var item = state.StatefulItems.Get(itemId);
        if (item.Location.Kind != StatefulItemLocationKind.Ground
            || item.Location.Position != state.Player.Position
            || !string.Equals(item.Location.SiteId, state.SiteId, StringComparison.OrdinalIgnoreCase))
        {
            return GameActionResult.Failure("That item is not on this tile.");
        }

        if (!TryPlaceStatefulItemInInventory(state, item))
        {
            return GameActionResult.Failure("Not enough inventory grid space.");
        }

        state.StatefulItems.MoveToInventory(item.Id);
        state.AdvanceTime(PickupTickCost);

        return GameActionResult.Success(
            PickupTickCost,
            $"Picked up {FormatStatefulItem(item)}. Time +{PickupTickCost}."
        );
    }

    private GameActionResult DropStatefulItem(PrototypeGameState state, StatefulItemId itemId)
    {
        var item = state.StatefulItems.Get(itemId);
        if (item.Location.Kind != StatefulItemLocationKind.PlayerInventory)
        {
            return GameActionResult.Failure("That item is not freely available to drop.");
        }

        state.StatefulItems.MoveToGround(item.Id, state.Player.Position, state.SiteId);
        state.Player.Inventory.Container.Remove(ContainerItemRef.Stateful(item.Id));
        return GameActionResult.Success(
            0,
            $"Dropped {FormatStatefulItem(item)}."
        );
    }

    private GameActionResult InspectItem(PrototypeGameState state, ItemId itemId)
    {
        if (state.Player.Inventory.CountOf(itemId) < 1 && !state.Player.Equipment.ContainsItem(itemId))
        {
            return GameActionResult.Failure("That item is not available.");
        }

        return GameActionResult.Success(InspectItemTickCost, DescribeStackItem(itemId, state));
    }

    private GameActionResult DropItemStack(PrototypeGameState state, ItemId itemId, int quantity)
    {
        if (quantity < 1)
        {
            return GameActionResult.Failure("Drop quantity must be at least 1.");
        }

        var currentQuantity = state.Player.Inventory.CountOf(itemId);
        if (currentQuantity < quantity)
        {
            return GameActionResult.Failure("You do not have enough of that item to drop.");
        }

        state.Player.Inventory.TryRemove(itemId, quantity);
        state.LocalMap.GroundItems.Place(state.Player.Position, itemId, quantity);

        return GameActionResult.Success(
            DropItemTickCost,
            $"Dropped {FormatStack(new GroundItemStack(itemId, quantity))}."
        );
    }

    private GameActionResult EquipStatefulItem(PrototypeGameState state, StatefulItemId itemId, EquipmentSlotId slotId)
    {
        var item = state.StatefulItems.Get(itemId);
        if (item.Location.Kind != StatefulItemLocationKind.PlayerInventory)
        {
            return GameActionResult.Failure("That item is not in your inventory.");
        }

        if (!_itemCatalog.TryGet(item.ItemId, out var definition))
        {
            return GameActionResult.Failure($"Unknown item: {item.ItemId}.");
        }

        if (!definition.AllowsAction("equip"))
        {
            return GameActionResult.Failure($"{definition.Name} cannot be equipped.");
        }

        if (!state.Player.Equipment.SlotCatalog.TryGet(slotId, out var slot))
        {
            return GameActionResult.Failure($"Unknown equipment slot: {slotId}.");
        }

        if (!slot.Accepts(definition.TypePath))
        {
            return GameActionResult.Failure($"{definition.Name} cannot be equipped in {slot.DisplayName}.");
        }

        if (!IsSlotFree(state, slot.Id))
        {
            return GameActionResult.Failure($"{slot.DisplayName} is already occupied.");
        }

        state.StatefulItems.MoveToEquipment(item.Id, slot.Id);
        state.Player.Inventory.Container.Remove(ContainerItemRef.Stateful(item.Id));
        return GameActionResult.Success(
            EquipItemTickCost,
            $"Equipped {FormatStatefulItem(item)} to {slot.DisplayName}."
        );
    }

    private GameActionResult UnequipStatefulItem(PrototypeGameState state, StatefulItemId itemId)
    {
        var item = state.StatefulItems.Get(itemId);
        if (item.Location.Kind != StatefulItemLocationKind.Equipment)
        {
            return GameActionResult.Failure("That item is not equipped.");
        }

        if (!TryPlaceStatefulItemInInventory(state, item))
        {
            return GameActionResult.Failure("Not enough inventory grid space.");
        }

        state.StatefulItems.MoveToInventory(item.Id);
        return GameActionResult.Success(
            UnequipItemTickCost,
            $"Unequipped {FormatStatefulItem(item)}."
        );
    }

    private GameActionResult InspectStatefulItem(PrototypeGameState state, StatefulItemId itemId)
    {
        var item = state.StatefulItems.Get(itemId);
        var messages = new List<string>
        {
            DescribeStatefulItem(item)
        };

        if (item.Contents.Count == 0)
        {
            messages.Add("Contents: empty.");
        }
        else
        {
            var contents = item.Contents
                .Select(id => state.StatefulItems.TryGet(id, out var content) ? FormatStatefulItem(content) : id.ToString());
            messages.Add($"Contents: {string.Join(", ", contents)}.");
        }

        return GameActionResult.Success(0, messages.ToArray());
    }

    private GameActionResult EquipItem(PrototypeGameState state, ItemId itemId, EquipmentSlotId slotId)
    {
        if (state.Player.Inventory.CountOf(itemId) < 1)
        {
            return GameActionResult.Failure("That item is not in your inventory.");
        }

        if (!_itemCatalog.TryGet(itemId, out var item))
        {
            return GameActionResult.Failure($"Unknown item: {itemId}.");
        }

        if (!item.AllowsAction("equip"))
        {
            return GameActionResult.Failure($"{item.Name} cannot be equipped.");
        }

        if (!state.Player.Equipment.SlotCatalog.TryGet(slotId, out var slot))
        {
            return GameActionResult.Failure($"Unknown equipment slot: {slotId}.");
        }

        var equippedItem = new EquippedItemRef(item.Id, item.TypePath);

        if (!slot.Accepts(equippedItem.ItemTypePath))
        {
            return GameActionResult.Failure($"{item.Name} cannot be equipped in {slot.DisplayName}.");
        }

        if (!state.Player.Equipment.IsEmpty(slotId))
        {
            return GameActionResult.Failure($"{slot.DisplayName} is already occupied.");
        }

        if (!state.Player.Inventory.TryRemove(item.Id))
        {
            return GameActionResult.Failure("That item is not in your inventory.");
        }

        state.Player.Equipment.OccupySlot(slotId, equippedItem);

        return GameActionResult.Success(
            EquipItemTickCost,
            $"Equipped {item.Name} to {slot.DisplayName}."
        );
    }

    private GameActionResult UnequipItem(PrototypeGameState state, EquipmentSlotId slotId)
    {
        if (!state.Player.Equipment.SlotCatalog.TryGet(slotId, out var slot))
        {
            return GameActionResult.Failure($"Unknown equipment slot: {slotId}.");
        }

        if (!state.Player.Equipment.TryGetEquippedItem(slotId, out var existingItem))
        {
            return GameActionResult.Failure($"{slot.DisplayName} is empty.");
        }

        if (!state.Player.Inventory.CanAdd(
            existingItem.ItemId,
            GetInventorySize(existingItem.ItemId),
            UsesInventoryGrid(existingItem.ItemId)))
        {
            return GameActionResult.Failure("Not enough inventory grid space.");
        }

        if (!state.Player.Equipment.TryUnequipSlot(slotId, out var equippedItem))
        {
            return GameActionResult.Failure($"{slot.DisplayName} is empty.");
        }

        state.Player.Inventory.Add(
            equippedItem.ItemId,
            size: GetInventorySize(equippedItem.ItemId),
            usesGrid: UsesInventoryGrid(equippedItem.ItemId)
        );

        return GameActionResult.Success(
            UnequipItemTickCost,
            $"Unequipped {GetItemName(equippedItem.ItemId)} from {slot.DisplayName}."
        );
    }

    private string FormatStack(GroundItemStack stack)
    {
        var itemName = _itemCatalog.TryGet(stack.ItemId, out var item)
            ? item.Name
            : stack.ItemId.ToString();

        return stack.Quantity == 1 ? itemName : $"{stack.Quantity} x {itemName}";
    }

    private string GetItemName(ItemId itemId)
    {
        return _itemCatalog.TryGet(itemId, out var item)
            ? item.Name
            : itemId.ToString();
    }

    private InventoryItemSize GetInventorySize(ItemId itemId)
    {
        return _itemCatalog.TryGet(itemId, out var item)
            ? item.InventorySize
            : InventoryItemSize.Default;
    }

    private bool UsesInventoryGrid(ItemId itemId)
    {
        return !_itemCatalog.TryGet(itemId, out var item) || InventoryGridRules.UsesGrid(item);
    }

    private bool TryPlaceStatefulItemInInventory(PrototypeGameState state, StatefulItem item)
    {
        var itemRef = ContainerItemRef.Stateful(item.Id);
        return state.Player.Inventory.Container.Contains(itemRef)
            || state.Player.Inventory.Container.TryAutoPlace(itemRef, GetInventorySize(item.ItemId));
    }

    private void SynchronizeStatefulInventoryPlacements(PrototypeGameState state)
    {
        var freelyCarriedItems = state.StatefulItems.InPlayerInventory();
        var carriedIds = freelyCarriedItems
            .Select(item => item.Id)
            .ToHashSet();

        foreach (var placement in state.Player.Inventory.Container.Placements
            .Where(placement => placement.Item.Kind == ContainerItemRefKind.Stateful
                && placement.Item.StatefulItemId is not null
                && !carriedIds.Contains(placement.Item.StatefulItemId.Value))
            .ToArray())
        {
            state.Player.Inventory.Container.Remove(placement.Item);
        }

        foreach (var item in freelyCarriedItems)
        {
            TryPlaceStatefulItemInInventory(state, item);
        }
    }

    private string DescribeStackItem(ItemId itemId, PrototypeGameState state)
    {
        var quantity = state.Player.Inventory.CountOf(itemId);
        var equippedSlots = state.Player.Equipment.Slots
            .Where(slot => state.Player.Equipment.TryGetEquippedItem(slot.Id, out var equippedItem)
                && equippedItem.ItemId == itemId)
            .Select(slot => slot.DisplayName)
            .ToArray();
        var location = quantity > 0 && equippedSlots.Length > 0
            ? $"Inventory x{quantity}; Equipped: {string.Join(", ", equippedSlots)}"
            : quantity > 0
                ? $"Inventory x{quantity}"
                : $"Equipped: {string.Join(", ", equippedSlots)}";

        if (!_itemCatalog.TryGet(itemId, out var definition))
        {
            return $"{itemId}. Location: {location}.";
        }

        var tags = definition.Tags.Count == 0
            ? "none"
            : string.Join(", ", definition.Tags);
        var description = string.IsNullOrWhiteSpace(definition.Description)
            ? string.Empty
            : $" {definition.Description}";

        return $"{definition.DisplayName} - {definition.Category}. Tags: {tags}. Location: {location}.{description}";
    }

    private bool IsSlotFree(PrototypeGameState state, EquipmentSlotId slotId)
    {
        return state.Player.Equipment.IsEmpty(slotId)
            && state.StatefulItems.EquippedIn(slotId) is null;
    }

    private string FormatStatefulItem(StatefulItem item)
    {
        var name = _itemCatalog.TryGet(item.ItemId, out var definition)
            ? definition.DisplayName
            : item.ItemId.ToString();

        var stateText = item.FeedDevice is not null
            ? $" {FormatFeedState(item.FeedDevice)}"
            : string.Empty;

        return $"{name} [{item.Id}]{stateText}";
    }

    private string DescribeStatefulItem(StatefulItem item)
    {
        var definition = _itemCatalog.TryGet(item.ItemId, out var foundDefinition)
            ? foundDefinition
            : null;
        var name = definition?.DisplayName ?? item.ItemId.ToString();
        var category = definition?.Category ?? "Unknown";
        var tags = definition is null || definition.Tags.Count == 0
            ? "none"
            : string.Join(", ", definition.Tags);
        var location = FormatLocation(item.Location);
        var details = $"{name} [{item.Id}] - {category}. Tags: {tags}. Condition: {item.Condition}. Location: {location}.";

        if (item.FeedDevice is not null)
        {
            details += $" Feed: {FormatFeedState(item.FeedDevice)} accepts {item.FeedDevice.AmmoSize}.";
        }

        if (item.Weapon is not null)
        {
            var feed = item.Weapon.BuiltInFeed;
            var inserted = item.Weapon.InsertedFeedDeviceItemId?.ToString() ?? "none";
            details += feed is null
                ? $" Inserted feed: {inserted}."
                : $" Built-in feed: {FormatFeedState(feed)}.";
        }

        if (!string.IsNullOrWhiteSpace(definition?.Description))
        {
            details += $" {definition.Description}";
        }

        return details;
    }

    private static string FormatFeedState(FeedDeviceState feedDevice)
    {
        var loaded = feedDevice.LoadedAmmunitionVariant is null
            ? "empty"
            : $"{feedDevice.LoadedCount}/{feedDevice.Capacity} {feedDevice.LoadedAmmunitionVariant}";

        return $"({loaded})";
    }

    private static string FormatLocation(StatefulItemLocation location)
    {
        return location.Kind switch
        {
            StatefulItemLocationKind.PlayerInventory => "inventory",
            StatefulItemLocationKind.Ground => location.SiteId is null
                ? $"ground {location.Position?.X}, {location.Position?.Y}"
                : $"ground {location.SiteId} {location.Position?.X}, {location.Position?.Y}",
            StatefulItemLocationKind.Equipment => $"equipment {location.EquipmentSlotId}",
            StatefulItemLocationKind.Inserted => $"inserted in {location.ParentItemId}",
            StatefulItemLocationKind.Contained => $"inside {location.ParentItemId}",
            _ => location.Kind.ToString()
        };
    }

    private bool CanRefuelVehicle(PrototypeGameState state)
    {
        return _vehicleFuelState is not null
            && !_vehicleFuelState.IsFull
            && IsAdjacentToFuelPump(state);
    }

    private static bool IsAdjacentToFuelPump(PrototypeGameState state)
    {
        var offsets = new[]
        {
            GridOffset.Up,
            GridOffset.Down,
            GridOffset.Left,
            GridOffset.Right
        };

        return offsets.Any(offset =>
        {
            var position = state.Player.Position + offset;
            return state.LocalMap.Map.Contains(position)
                && state.LocalMap.WorldObjects.TryGetObjectAt(position, out var objectId)
                && objectId == PrototypeWorldObjects.FuelPump;
        });
    }
}
