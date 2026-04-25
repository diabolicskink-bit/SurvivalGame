namespace SurvivalGame.Domain;

public enum GameActionKind
{
    Wait,
    Move,
    Pickup,
    EquipItem,
    LoadFeedDevice,
    UnloadFeedDevice,
    InsertFeedDevice,
    RemoveFeedDevice,
    LoadWeapon,
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
    TestFireStatefulWeapon
}

public abstract record GameActionRequest(GameActionKind Kind);

public sealed record WaitActionRequest() : GameActionRequest(GameActionKind.Wait);

public sealed record MoveActionRequest(GridOffset Direction) : GameActionRequest(GameActionKind.Move);

public sealed record PickupActionRequest() : GameActionRequest(GameActionKind.Pickup);

public sealed record EquipItemActionRequest(ItemId ItemId, EquipmentSlotId SlotId) : GameActionRequest(GameActionKind.EquipItem);

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

public sealed record TestFireStatefulWeaponActionRequest(StatefulItemId WeaponItemId)
    : GameActionRequest(GameActionKind.TestFireStatefulWeapon);

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

    private readonly ItemCatalog _itemCatalog;
    private readonly WorldObjectCatalog? _worldObjectCatalog;
    private readonly FirearmActionService? _firearmActions;

    public GameActionPipeline(
        ItemCatalog itemCatalog,
        WorldObjectCatalog? worldObjectCatalog = null,
        FirearmCatalog? firearmCatalog = null
    )
    {
        ArgumentNullException.ThrowIfNull(itemCatalog);
        _itemCatalog = itemCatalog;
        _worldObjectCatalog = worldObjectCatalog;
        _firearmActions = firearmCatalog is null ? null : new FirearmActionService(firearmCatalog);
    }

    public IReadOnlyList<AvailableAction> GetAvailableActions(PrototypeGameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var actions = new List<AvailableAction>
        {
            new(GameActionKind.Wait, "Wait", new WaitActionRequest())
        };

        if (state.World.GroundItems.ItemsAt(state.Player.Position).Count > 0)
        {
            actions.Add(new AvailableAction(GameActionKind.Pickup, "Pick Up", new PickupActionRequest()));
        }

        actions.AddRange(GetAvailableStatefulItemActions(state));
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

        return request switch
        {
            WaitActionRequest => Wait(state),
            MoveActionRequest move => Move(state, move.Direction),
            PickupActionRequest => Pickup(state),
            EquipItemActionRequest equip => EquipItem(state, equip.ItemId, equip.SlotId),
            LoadFeedDeviceActionRequest loadFeed => ExecuteFirearmAction(
                service => service.LoadFeedDevice(state, loadFeed.FeedDeviceItemId, loadFeed.AmmunitionItemId)
            ),
            UnloadFeedDeviceActionRequest unloadFeed => ExecuteFirearmAction(
                service => service.UnloadFeedDevice(state, unloadFeed.FeedDeviceItemId)
            ),
            InsertFeedDeviceActionRequest insertFeed => ExecuteFirearmAction(
                service => service.InsertFeedDevice(state, insertFeed.WeaponItemId, insertFeed.FeedDeviceItemId)
            ),
            RemoveFeedDeviceActionRequest removeFeed => ExecuteFirearmAction(
                service => service.RemoveFeedDevice(state, removeFeed.WeaponItemId)
            ),
            LoadWeaponActionRequest loadWeapon => ExecuteFirearmAction(
                service => service.LoadWeapon(state, loadWeapon.WeaponItemId, loadWeapon.AmmunitionItemId)
            ),
            TestFireActionRequest testFire => ExecuteFirearmAction(
                service => service.TestFire(state, testFire.WeaponItemId)
            ),
            PickupStatefulItemActionRequest pickupStateful => PickupStatefulItem(state, pickupStateful.ItemId),
            DropStatefulItemActionRequest dropStateful => DropStatefulItem(state, dropStateful.ItemId),
            InspectStatefulItemActionRequest inspectStateful => InspectStatefulItem(state, inspectStateful.ItemId),
            EquipStatefulItemActionRequest equipStateful => EquipStatefulItem(state, equipStateful.ItemId, equipStateful.SlotId),
            UnequipStatefulItemActionRequest unequipStateful => UnequipStatefulItem(state, unequipStateful.ItemId),
            LoadStatefulFeedDeviceActionRequest loadStatefulFeed => ExecuteFirearmAction(
                service => service.LoadStatefulFeedDevice(state, loadStatefulFeed.FeedDeviceItemId, loadStatefulFeed.AmmunitionItemId)
            ),
            UnloadStatefulFeedDeviceActionRequest unloadStatefulFeed => ExecuteFirearmAction(
                service => service.UnloadStatefulFeedDevice(state, unloadStatefulFeed.FeedDeviceItemId)
            ),
            InsertStatefulFeedDeviceActionRequest insertStatefulFeed => ExecuteFirearmAction(
                service => service.InsertStatefulFeedDevice(state, insertStatefulFeed.WeaponItemId, insertStatefulFeed.FeedDeviceItemId)
            ),
            RemoveStatefulFeedDeviceActionRequest removeStatefulFeed => ExecuteFirearmAction(
                service => service.RemoveStatefulFeedDevice(state, removeStatefulFeed.WeaponItemId)
            ),
            LoadStatefulWeaponActionRequest loadStatefulWeapon => ExecuteFirearmAction(
                service => service.LoadStatefulWeapon(state, loadStatefulWeapon.WeaponItemId, loadStatefulWeapon.AmmunitionItemId)
            ),
            TestFireStatefulWeaponActionRequest testStatefulWeapon => ExecuteFirearmAction(
                service => service.TestFireStatefulWeapon(state, testStatefulWeapon.WeaponItemId)
            ),
            _ => GameActionResult.Failure("That action is not supported.")
        };
    }

    private GameActionResult ExecuteFirearmAction(Func<FirearmActionService, GameActionResult> action)
    {
        return _firearmActions is null
            ? GameActionResult.Failure("Firearm actions are not available.")
            : action(_firearmActions);
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

    private IEnumerable<AvailableAction> GetAvailableStatefulItemActions(PrototypeGameState state)
    {
        foreach (var item in state.StatefulItems.OnGround(state.Player.Position))
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
        if (!state.World.Map.Contains(nextPosition))
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

        if (!state.World.WorldObjects.TryGetObjectAt(position, out var objectId))
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
        if (state.World.Npcs.TryGetAt(position, out var npc))
        {
            npcName = npc.Name;
            return true;
        }

        npcName = string.Empty;
        return false;
    }

    private GameActionResult Pickup(PrototypeGameState state)
    {
        var itemStacks = state.World.GroundItems.TakeAllAt(state.Player.Position);
        if (itemStacks.Count == 0)
        {
            return GameActionResult.Failure("There is nothing here to pick up.");
        }

        foreach (var stack in itemStacks)
        {
            state.Player.Inventory.Add(stack.ItemId, stack.Quantity);
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
        if (item.Location.Kind != StatefulItemLocationKind.Ground || item.Location.Position != state.Player.Position)
        {
            return GameActionResult.Failure("That item is not on this tile.");
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

        state.StatefulItems.MoveToGround(item.Id, state.Player.Position);
        return GameActionResult.Success(
            0,
            $"Dropped {FormatStatefulItem(item)}."
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
        return GameActionResult.Success(
            0,
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

        state.StatefulItems.MoveToInventory(item.Id);
        return GameActionResult.Success(
            0,
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
            0,
            $"Equipped {item.Name} to {slot.DisplayName}."
        );
    }

    private string FormatStack(GroundItemStack stack)
    {
        var itemName = _itemCatalog.TryGet(stack.ItemId, out var item)
            ? item.Name
            : stack.ItemId.ToString();

        return stack.Quantity == 1 ? itemName : $"{stack.Quantity} x {itemName}";
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
            StatefulItemLocationKind.Ground => $"ground {location.Position?.X}, {location.Position?.Y}",
            StatefulItemLocationKind.Equipment => $"equipment {location.EquipmentSlotId}",
            StatefulItemLocationKind.Inserted => $"inserted in {location.ParentItemId}",
            StatefulItemLocationKind.Contained => $"inside {location.ParentItemId}",
            _ => location.Kind.ToString()
        };
    }
}
