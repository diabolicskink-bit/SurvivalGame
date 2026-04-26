namespace SurvivalGame.Domain;

public sealed record WeaponModDefinition
{
    public WeaponModDefinition(
        ItemId itemId,
        string name,
        WeaponModSlotId slot,
        IEnumerable<string> compatibleWeaponFamilies,
        int effectiveRangeBonus = 0,
        int maximumRangeBonus = 0,
        int damageBonus = 0
    )
    {
        ArgumentNullException.ThrowIfNull(itemId);
        ArgumentNullException.ThrowIfNull(slot);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Weapon mod name cannot be empty.", nameof(name));
        }

        var families = (compatibleWeaponFamilies ?? throw new ArgumentNullException(nameof(compatibleWeaponFamilies)))
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

        if (families.Length == 0)
        {
            throw new ArgumentException("Weapon mod must be compatible with at least one weapon family.", nameof(compatibleWeaponFamilies));
        }

        ItemId = itemId;
        Name = name.Trim();
        Slot = slot;
        CompatibleWeaponFamilies = families;
        EffectiveRangeBonus = effectiveRangeBonus;
        MaximumRangeBonus = maximumRangeBonus;
        DamageBonus = damageBonus;
    }

    public ItemId ItemId { get; }

    public string Name { get; }

    public WeaponModSlotId Slot { get; }

    public IReadOnlyList<string> CompatibleWeaponFamilies { get; }

    public int EffectiveRangeBonus { get; }

    public int MaximumRangeBonus { get; }

    public int DamageBonus { get; }

    public bool IsCompatibleWith(WeaponDefinition weapon)
    {
        ArgumentNullException.ThrowIfNull(weapon);
        return CompatibleWeaponFamilies.Contains(weapon.WeaponFamily, StringComparer.OrdinalIgnoreCase);
    }

    public bool HasAnyEffect =>
        EffectiveRangeBonus != 0
        || MaximumRangeBonus != 0
        || DamageBonus != 0;
}
