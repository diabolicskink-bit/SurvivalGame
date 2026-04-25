using SurvivalGame.Domain;

public enum SelectedItemKind
{
    InventoryStack,
    EquipmentItem,
    StatefulItem
}

public sealed record SelectedItemRef(
    SelectedItemKind Kind,
    ItemId? ItemId = null,
    StatefulItemId? StatefulItemId = null,
    EquipmentSlotId? EquipmentSlotId = null)
{
    public static SelectedItemRef InventoryStack(ItemId itemId)
    {
        return new SelectedItemRef(SelectedItemKind.InventoryStack, ItemId: itemId);
    }

    public static SelectedItemRef EquipmentItem(EquipmentSlotId slotId, ItemId itemId)
    {
        return new SelectedItemRef(SelectedItemKind.EquipmentItem, ItemId: itemId, EquipmentSlotId: slotId);
    }

    public static SelectedItemRef StatefulItem(StatefulItemId itemId)
    {
        return new SelectedItemRef(SelectedItemKind.StatefulItem, StatefulItemId: itemId);
    }
}
