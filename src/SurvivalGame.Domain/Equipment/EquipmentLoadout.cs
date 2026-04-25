namespace SurvivalGame.Domain;

public sealed class EquipmentLoadout
{
    private readonly Dictionary<EquipmentSlotId, EquippedItemRef> _equippedItems = new();
    private readonly EquipmentValidator _validator;

    public EquipmentLoadout(EquipmentSlotCatalog slotCatalog)
        : this(slotCatalog, new EquipmentValidator())
    {
    }

    public EquipmentLoadout(EquipmentSlotCatalog slotCatalog, EquipmentValidator validator)
    {
        ArgumentNullException.ThrowIfNull(slotCatalog);
        ArgumentNullException.ThrowIfNull(validator);

        SlotCatalog = slotCatalog;
        _validator = validator;
    }

    public EquipmentSlotCatalog SlotCatalog { get; }

    public IReadOnlyCollection<EquipmentSlotDefinition> Slots => SlotCatalog.Slots;

    public IReadOnlyCollection<EquippedItemRef> EquippedItems => _equippedItems.Values.ToArray();

    public static EquipmentLoadout CreateDefault()
    {
        return new EquipmentLoadout(EquipmentSlotCatalog.CreateDefault());
    }

    public bool IsEmpty(EquipmentSlotId slotId)
    {
        EnsureSlotExists(slotId);
        return !_equippedItems.ContainsKey(slotId);
    }

    public bool TryGetEquippedItem(EquipmentSlotId slotId, out EquippedItemRef item)
    {
        EnsureSlotExists(slotId);
        if (_equippedItems.TryGetValue(slotId, out var equippedItem))
        {
            item = equippedItem;
            return true;
        }

        item = null!;
        return false;
    }

    public void OccupySlot(EquipmentSlotId slotId, EquippedItemRef item)
    {
        var slot = SlotCatalog.Get(slotId);
        _validator.ValidateCanOccupy(slot, item);

        _equippedItems[slotId] = item;
    }

    public bool TryUnequipSlot(EquipmentSlotId slotId, out EquippedItemRef item)
    {
        EnsureSlotExists(slotId);
        if (!_equippedItems.Remove(slotId, out var equippedItem))
        {
            item = null!;
            return false;
        }

        item = equippedItem;
        return true;
    }

    public bool ContainsItem(ItemId itemId)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        return _equippedItems.Values.Any(item => item.ItemId == itemId);
    }

    private void EnsureSlotExists(EquipmentSlotId slotId)
    {
        if (!SlotCatalog.Contains(slotId))
        {
            throw new KeyNotFoundException($"Equipment slot '{slotId}' is not defined.");
        }
    }
}
