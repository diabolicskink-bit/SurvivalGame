namespace SurvivalGame.Domain;

public sealed record AmmunitionDefinition
{
    public AmmunitionDefinition(ItemId itemId, string name, AmmoSizeId size, string variant, int damage)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        ArgumentNullException.ThrowIfNull(size);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Ammunition name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(variant))
        {
            throw new ArgumentException("Ammunition variant cannot be empty.", nameof(variant));
        }

        if (damage < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(damage), "Ammunition damage must be at least 1.");
        }

        ItemId = itemId;
        Name = name.Trim();
        Size = size;
        Variant = variant.Trim();
        Damage = damage;
    }

    public ItemId ItemId { get; }

    public string Name { get; }

    public AmmoSizeId Size { get; }

    public string Variant { get; }

    public int Damage { get; }
}
