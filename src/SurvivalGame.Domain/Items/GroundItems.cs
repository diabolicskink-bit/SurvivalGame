namespace SurvivalGame.Domain;

public readonly record struct GroundItemStack(ItemId ItemId, int Quantity);

public readonly record struct PlacedItemStack(GridPosition Position, GroundItemStack Stack);

public sealed class TileItemMap
{
    private readonly Dictionary<GridPosition, List<GroundItemStack>> _itemsByPosition = new();

    public IReadOnlyList<PlacedItemStack> AllItems
    {
        get
        {
            var placedItems = new List<PlacedItemStack>();

            foreach (var entry in _itemsByPosition)
            {
                foreach (var stack in entry.Value)
                {
                    placedItems.Add(new PlacedItemStack(entry.Key, stack));
                }
            }

            return placedItems;
        }
    }

    public bool IsEmpty => _itemsByPosition.Count == 0;

    public IReadOnlyList<GroundItemStack> ItemsAt(GridPosition position)
    {
        return _itemsByPosition.TryGetValue(position, out var stacks)
            ? stacks.ToArray()
            : Array.Empty<GroundItemStack>();
    }

    public IReadOnlyList<GroundItemStack> TakeAllAt(GridPosition position)
    {
        if (!_itemsByPosition.Remove(position, out var stacks))
        {
            return Array.Empty<GroundItemStack>();
        }

        return stacks.ToArray();
    }

    public void Place(GridPosition position, ItemId itemId, int quantity = 1)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        ValidatePositiveQuantity(quantity);

        if (!_itemsByPosition.TryGetValue(position, out var stacks))
        {
            stacks = new List<GroundItemStack>();
            _itemsByPosition[position] = stacks;
        }

        var existingIndex = stacks.FindIndex(stack => stack.ItemId == itemId);
        if (existingIndex >= 0)
        {
            var existing = stacks[existingIndex];
            stacks[existingIndex] = existing with { Quantity = existing.Quantity + quantity };
            return;
        }

        stacks.Add(new GroundItemStack(itemId, quantity));
    }

    private static void ValidatePositiveQuantity(int quantity)
    {
        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");
        }
    }
}
