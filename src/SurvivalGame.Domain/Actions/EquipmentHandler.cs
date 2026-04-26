namespace SurvivalGame.Domain;

public sealed class EquipmentHandler : IActionHandler
{
    public IReadOnlyList<GameActionKind> HandledKinds { get; } = new[]
    {
        GameActionKind.EquipItem,
        GameActionKind.UnequipItem,
        GameActionKind.EquipStatefulItem,
        GameActionKind.UnequipStatefulItem
    };

    public IEnumerable<AvailableAction> GetAvailableActions(GameActionContext context)
    {
        var state = context.State;
        foreach (var item in state.StatefulItems.InPlayerInventory())
        {
            if (!context.ItemCatalog.TryGet(item.ItemId, out var definition) || !definition.AllowsAction("equip"))
            {
                continue;
            }

            foreach (var slot in state.Player.Equipment.Slots)
            {
                if (!context.IsSlotFree(slot.Id) || !slot.Accepts(definition.TypePath))
                {
                    continue;
                }

                yield return new AvailableAction(
                    GameActionKind.EquipStatefulItem,
                    $"Equip {context.ItemDescriber.FormatStatefulItem(item)} ({slot.DisplayName})",
                    new EquipStatefulItemActionRequest(item.Id, slot.Id)
                );
            }
        }

        foreach (var item in state.StatefulItems.Equipped())
        {
            yield return new AvailableAction(
                GameActionKind.UnequipStatefulItem,
                $"Unequip {context.ItemDescriber.FormatStatefulItem(item)}",
                new UnequipStatefulItemActionRequest(item.Id)
            );
        }

        foreach (var stack in state.Player.Inventory.Items)
        {
            if (!context.ItemCatalog.TryGet(stack.ItemId, out var item) || !item.AllowsAction("equip"))
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

        foreach (var slot in state.Player.Equipment.Slots)
        {
            if (!state.Player.Equipment.TryGetEquippedItem(slot.Id, out var equippedItem))
            {
                continue;
            }

            var itemName = context.ItemDescriber.GetItemName(equippedItem.ItemId);
            yield return new AvailableAction(
                GameActionKind.UnequipItem,
                $"Unequip {itemName}",
                new UnequipItemActionRequest(slot.Id)
            );
        }
    }

    public GameActionResult Handle(GameActionRequest request, GameActionContext context)
    {
        return request switch
        {
            EquipItemActionRequest equip => EquipItem(context, equip.ItemId, equip.SlotId),
            UnequipItemActionRequest unequip => UnequipItem(context, unequip.SlotId),
            EquipStatefulItemActionRequest equipStateful => EquipStatefulItem(context, equipStateful.ItemId, equipStateful.SlotId),
            UnequipStatefulItemActionRequest unequipStateful => UnequipStatefulItem(context, unequipStateful.ItemId),
            _ => GameActionResult.Failure("That action is not supported.")
        };
    }

    private static GameActionResult EquipStatefulItem(GameActionContext context, StatefulItemId itemId, EquipmentSlotId slotId)
    {
        var state = context.State;
        var item = state.StatefulItems.Get(itemId);
        if (item.Location is not PlayerInventoryLocation)
        {
            return GameActionResult.Failure("That item is not in your inventory.");
        }

        if (!context.ItemCatalog.TryGet(item.ItemId, out var definition))
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

        if (!context.IsSlotFree(slot.Id))
        {
            return GameActionResult.Failure($"{slot.DisplayName} is already occupied.");
        }

        state.StatefulItems.MoveToEquipment(item.Id, slot.Id);
        state.Player.Inventory.Container.Remove(ContainerItemRef.Stateful(item.Id));
        return GameActionResult.Success(
            GameActionPipeline.EquipItemTickCost,
            $"Equipped {context.ItemDescriber.FormatStatefulItem(item)} to {slot.DisplayName}."
        );
    }

    private static GameActionResult UnequipStatefulItem(GameActionContext context, StatefulItemId itemId)
    {
        var state = context.State;
        var item = state.StatefulItems.Get(itemId);
        if (item.Location is not EquipmentLocation)
        {
            return GameActionResult.Failure("That item is not equipped.");
        }

        if (!context.TryPlaceStatefulItemInInventory(item))
        {
            return GameActionResult.Failure("Not enough inventory grid space.");
        }

        state.StatefulItems.MoveToInventory(item.Id);
        return GameActionResult.Success(
            GameActionPipeline.UnequipItemTickCost,
            $"Unequipped {context.ItemDescriber.FormatStatefulItem(item)}."
        );
    }

    private static GameActionResult EquipItem(GameActionContext context, ItemId itemId, EquipmentSlotId slotId)
    {
        var state = context.State;
        if (state.Player.Inventory.CountOf(itemId) < 1)
        {
            return GameActionResult.Failure("That item is not in your inventory.");
        }

        if (!context.ItemCatalog.TryGet(itemId, out var item))
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
            GameActionPipeline.EquipItemTickCost,
            $"Equipped {item.Name} to {slot.DisplayName}."
        );
    }

    private static GameActionResult UnequipItem(GameActionContext context, EquipmentSlotId slotId)
    {
        var state = context.State;
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
            context.ItemDescriber.GetInventorySize(existingItem.ItemId),
            context.ItemDescriber.UsesInventoryGrid(existingItem.ItemId)))
        {
            return GameActionResult.Failure("Not enough inventory grid space.");
        }

        if (!state.Player.Equipment.TryUnequipSlot(slotId, out var equippedItem))
        {
            return GameActionResult.Failure($"{slot.DisplayName} is empty.");
        }

        state.Player.Inventory.Add(
            equippedItem.ItemId,
            size: context.ItemDescriber.GetInventorySize(equippedItem.ItemId),
            usesGrid: context.ItemDescriber.UsesInventoryGrid(equippedItem.ItemId)
        );

        return GameActionResult.Success(
            GameActionPipeline.UnequipItemTickCost,
            $"Unequipped {context.ItemDescriber.GetItemName(equippedItem.ItemId)} from {slot.DisplayName}."
        );
    }
}
