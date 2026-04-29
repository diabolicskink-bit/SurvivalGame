namespace SurvivalGame.Domain;

public sealed class TravelCargoStore
{
    private readonly Dictionary<ItemId, int> _stacks = new();
    private readonly HashSet<StatefulItemId> _statefulItems = new();

    public IReadOnlyList<GroundItemStack> StackItems => _stacks
        .OrderBy(stack => stack.Key.Value, StringComparer.OrdinalIgnoreCase)
        .Select(stack => new GroundItemStack(stack.Key, stack.Value))
        .ToArray();

    public IReadOnlyCollection<StatefulItemId> StatefulItemIds => _statefulItems.ToArray();

    public bool IsEmpty => _stacks.Count == 0 && _statefulItems.Count == 0;

    public int CountOf(ItemId itemId)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        return _stacks.GetValueOrDefault(itemId);
    }

    public void StowStack(ItemId itemId, int quantity)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        ValidatePositiveQuantity(quantity);

        _stacks[itemId] = CountOf(itemId) + quantity;
    }

    public bool TryTakeStack(ItemId itemId, int quantity, out GroundItemStack stack)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        ValidatePositiveQuantity(quantity);

        var current = CountOf(itemId);
        if (current < quantity)
        {
            stack = default;
            return false;
        }

        var remaining = current - quantity;
        if (remaining == 0)
        {
            _stacks.Remove(itemId);
        }
        else
        {
            _stacks[itemId] = remaining;
        }

        stack = new GroundItemStack(itemId, quantity);
        return true;
    }

    public void StowStatefulItem(StatefulItemId itemId)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        _statefulItems.Add(itemId);
    }

    public bool ContainsStatefulItem(StatefulItemId itemId)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        return _statefulItems.Contains(itemId);
    }

    public bool TryTakeStatefulItem(StatefulItemId itemId)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        return _statefulItems.Remove(itemId);
    }

    private static void ValidatePositiveQuantity(int quantity)
    {
        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");
        }
    }
}
