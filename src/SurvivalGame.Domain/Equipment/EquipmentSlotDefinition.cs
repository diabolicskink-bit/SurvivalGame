namespace SurvivalGame.Domain;

public sealed record EquipmentSlotDefinition
{
    public EquipmentSlotDefinition(
        EquipmentSlotId id,
        string displayName,
        EquipmentSlotGroup group,
        IEnumerable<ItemTypePath> acceptedItemTypes
    )
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Equipment slot display name cannot be empty.", nameof(displayName));
        }

        var acceptedTypes = (acceptedItemTypes ?? throw new ArgumentNullException(nameof(acceptedItemTypes)))
            .Distinct()
            .ToArray();

        if (acceptedTypes.Length == 0)
        {
            throw new ArgumentException("Equipment slots must accept at least one item type path.", nameof(acceptedItemTypes));
        }

        Id = id;
        DisplayName = displayName.Trim();
        Group = group;
        AcceptedItemTypes = acceptedTypes;
    }

    public EquipmentSlotId Id { get; }

    public string DisplayName { get; }

    public EquipmentSlotGroup Group { get; }

    public IReadOnlyList<ItemTypePath> AcceptedItemTypes { get; }

    public bool Accepts(ItemTypePath itemTypePath)
    {
        ArgumentNullException.ThrowIfNull(itemTypePath);

        return AcceptedItemTypes.Any(itemTypePath.IsA);
    }
}
