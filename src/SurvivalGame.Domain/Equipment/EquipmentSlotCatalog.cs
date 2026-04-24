namespace SurvivalGame.Domain;

public sealed class EquipmentSlotCatalog
{
    private readonly Dictionary<EquipmentSlotId, EquipmentSlotDefinition> _slots = new();

    public IReadOnlyCollection<EquipmentSlotDefinition> Slots => _slots.Values.ToArray();

    public static EquipmentSlotCatalog CreateDefault()
    {
        var catalog = new EquipmentSlotCatalog();

        catalog.Add(new EquipmentSlotDefinition(
            EquipmentSlotId.MainHand,
            "Main hand",
            EquipmentSlotGroup.Hands,
            new[] { new ItemTypePath("Weapon"), new ItemTypePath("Tool") }
        ));
        catalog.Add(new EquipmentSlotDefinition(
            EquipmentSlotId.OffHand,
            "Off hand",
            EquipmentSlotGroup.Hands,
            new[] { new ItemTypePath("Weapon"), new ItemTypePath("Tool") }
        ));
        catalog.Add(new EquipmentSlotDefinition(
            EquipmentSlotId.Head,
            "Head",
            EquipmentSlotGroup.Worn,
            new[] { new ItemTypePath("Clothing", "Head") }
        ));
        catalog.Add(new EquipmentSlotDefinition(
            EquipmentSlotId.Body,
            "Body",
            EquipmentSlotGroup.Worn,
            new[] { new ItemTypePath("Clothing", "Body") }
        ));
        catalog.Add(new EquipmentSlotDefinition(
            EquipmentSlotId.Legs,
            "Legs",
            EquipmentSlotGroup.Worn,
            new[] { new ItemTypePath("Clothing", "Legs") }
        ));
        catalog.Add(new EquipmentSlotDefinition(
            EquipmentSlotId.Feet,
            "Feet",
            EquipmentSlotGroup.Worn,
            new[] { new ItemTypePath("Clothing", "Feet") }
        ));
        catalog.Add(new EquipmentSlotDefinition(
            EquipmentSlotId.Back,
            "Back",
            EquipmentSlotGroup.Carried,
            new[] { new ItemTypePath("Clothing", "Back"), new ItemTypePath("Equipment", "Back") }
        ));

        return catalog;
    }

    public void Add(EquipmentSlotDefinition slot)
    {
        ArgumentNullException.ThrowIfNull(slot);

        if (!_slots.TryAdd(slot.Id, slot))
        {
            throw new InvalidOperationException($"Equipment slot '{slot.Id}' is already defined.");
        }
    }

    public bool Contains(EquipmentSlotId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _slots.ContainsKey(id);
    }

    public bool TryGet(EquipmentSlotId id, out EquipmentSlotDefinition slot)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (_slots.TryGetValue(id, out var foundSlot))
        {
            slot = foundSlot;
            return true;
        }

        slot = null!;
        return false;
    }

    public EquipmentSlotDefinition Get(EquipmentSlotId id)
    {
        if (TryGet(id, out var slot))
        {
            return slot;
        }

        throw new KeyNotFoundException($"Equipment slot '{id}' is not defined.");
    }
}
