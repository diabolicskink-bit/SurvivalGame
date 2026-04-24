namespace SurvivalGame.Domain;

public sealed class ItemCatalog
{
    private readonly Dictionary<ItemId, ItemDefinition> _items = new();

    public IReadOnlyCollection<ItemDefinition> Items => _items.Values.ToArray();

    public void Add(ItemDefinition item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!_items.TryAdd(item.Id, item))
        {
            throw new InvalidOperationException($"Item '{item.Id}' is already defined.");
        }
    }

    public bool TryGet(ItemId id, out ItemDefinition item)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (_items.TryGetValue(id, out var foundItem))
        {
            item = foundItem;
            return true;
        }

        item = null!;
        return false;
    }

    public ItemDefinition Get(ItemId id)
    {
        if (TryGet(id, out var item))
        {
            return item;
        }

        throw new KeyNotFoundException($"Item '{id}' is not defined.");
    }
}
