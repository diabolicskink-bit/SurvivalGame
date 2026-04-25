namespace SurvivalGame.Domain;

public sealed record AmmunitionDefinition
{
    public AmmunitionDefinition(ItemId itemId, string name, AmmoSizeId size, string variant)
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

        ItemId = itemId;
        Name = name.Trim();
        Size = size;
        Variant = variant.Trim();
    }

    public ItemId ItemId { get; }

    public string Name { get; }

    public AmmoSizeId Size { get; }

    public string Variant { get; }
}
