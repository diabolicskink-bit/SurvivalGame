namespace SurvivalGame.Domain;

public sealed record EquippedItemRef
{
    public EquippedItemRef(ItemId itemId, ItemTypePath itemTypePath)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        ArgumentNullException.ThrowIfNull(itemTypePath);

        ItemId = itemId;
        ItemTypePath = itemTypePath;
    }

    public ItemId ItemId { get; }

    public ItemTypePath ItemTypePath { get; }
}
