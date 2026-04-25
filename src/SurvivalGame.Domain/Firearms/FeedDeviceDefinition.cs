namespace SurvivalGame.Domain;

public sealed record FeedDeviceDefinition
{
    public FeedDeviceDefinition(
        ItemId itemId,
        string name,
        FeedDeviceKind kind,
        AmmoSizeId ammoSize,
        int capacity,
        IEnumerable<string>? compatibleWeaponFamilies = null
    )
    {
        ArgumentNullException.ThrowIfNull(itemId);
        ArgumentNullException.ThrowIfNull(ammoSize);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Feed device name cannot be empty.", nameof(name));
        }

        if (capacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Feed device capacity must be at least 1.");
        }

        ItemId = itemId;
        Name = name.Trim();
        Kind = kind;
        AmmoSize = ammoSize;
        Capacity = capacity;
        CompatibleWeaponFamilies = NormalizeFamilies(compatibleWeaponFamilies);
    }

    public ItemId ItemId { get; }

    public string Name { get; }

    public FeedDeviceKind Kind { get; }

    public AmmoSizeId AmmoSize { get; }

    public int Capacity { get; }

    public IReadOnlyList<string> CompatibleWeaponFamilies { get; }

    public bool IsCompatibleWith(WeaponDefinition weapon)
    {
        ArgumentNullException.ThrowIfNull(weapon);

        return Kind == weapon.FeedKind
            && weapon.AcceptsAmmoSize(AmmoSize)
            && CompatibleWeaponFamilies.Any(family =>
                string.Equals(family, weapon.WeaponFamily, StringComparison.OrdinalIgnoreCase)
            );
    }

    public FeedDeviceState CreateState()
    {
        return new FeedDeviceState(ItemId, Name, Kind, AmmoSize, Capacity);
    }

    private static IReadOnlyList<string> NormalizeFamilies(IEnumerable<string>? families)
    {
        return (families ?? Array.Empty<string>())
            .Select(family =>
            {
                if (string.IsNullOrWhiteSpace(family))
                {
                    throw new ArgumentException("Compatible weapon families cannot contain empty values.");
                }

                return family.Trim();
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
