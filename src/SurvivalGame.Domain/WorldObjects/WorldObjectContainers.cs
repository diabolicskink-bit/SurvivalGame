namespace SurvivalGame.Domain;

public sealed record WorldObjectInstanceId
{
    public WorldObjectInstanceId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("World object instance id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }
}

public sealed record LootTableId
{
    public LootTableId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Loot table id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }
}

public sealed record WorldObjectContainerDefinition
{
    public const int DefaultSearchTickCost = 75;

    public WorldObjectContainerDefinition(string profileId, int searchTickCost = DefaultSearchTickCost)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("Container profile id cannot be empty.", nameof(profileId));
        }

        if (searchTickCost < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(searchTickCost), "Container search tick cost cannot be negative.");
        }

        ProfileId = profileId.Trim();
        SearchTickCost = searchTickCost;
    }

    public string ProfileId { get; }

    public int SearchTickCost { get; }
}

public sealed class WorldObjectContainerLootSpec
{
    private static readonly WorldObjectContainerLootSpec EmptySpec = new(
        Array.Empty<GroundItemStack>(),
        Array.Empty<LootTableId>()
    );

    public WorldObjectContainerLootSpec(
        IEnumerable<GroundItemStack>? fixedStacks = null,
        IEnumerable<LootTableId>? lootTables = null)
    {
        FixedStacks = NormalizeStacks(fixedStacks).ToArray();
        LootTables = (lootTables ?? Array.Empty<LootTableId>()).ToArray();
    }

    public static WorldObjectContainerLootSpec Empty => EmptySpec;

    public IReadOnlyList<GroundItemStack> FixedStacks { get; }

    public IReadOnlyList<LootTableId> LootTables { get; }

    public bool IsEmpty => FixedStacks.Count == 0 && LootTables.Count == 0;

    private static IEnumerable<GroundItemStack> NormalizeStacks(IEnumerable<GroundItemStack>? stacks)
    {
        return (stacks ?? Array.Empty<GroundItemStack>())
            .GroupBy(stack =>
            {
                ArgumentNullException.ThrowIfNull(stack.ItemId);
                if (stack.Quantity < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(stacks), "Container loot quantities must be at least 1.");
                }

                return stack.ItemId;
            })
            .Select(group => new GroundItemStack(group.Key, group.Sum(stack => stack.Quantity)));
    }
}

public sealed class WorldObjectContainerState
{
    private readonly List<GroundItemStack> _remainingStacks;

    public WorldObjectContainerState(WorldObjectInstanceId instanceId, IEnumerable<GroundItemStack>? remainingStacks = null)
    {
        ArgumentNullException.ThrowIfNull(instanceId);

        InstanceId = instanceId;
        IsSearched = true;
        _remainingStacks = NormalizeStacks(remainingStacks).ToList();
    }

    public WorldObjectInstanceId InstanceId { get; }

    public bool IsSearched { get; }

    public IReadOnlyList<GroundItemStack> RemainingStacks => _remainingStacks.ToArray();

    public bool IsEmpty => _remainingStacks.Count == 0;

    public int CountOf(ItemId itemId)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        return _remainingStacks.FirstOrDefault(stack => stack.ItemId == itemId).Quantity;
    }

    public bool TryTake(ItemId itemId, int quantity, out GroundItemStack takenStack)
    {
        ArgumentNullException.ThrowIfNull(itemId);

        if (quantity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Take quantity must be at least 1.");
        }

        var existingIndex = _remainingStacks.FindIndex(stack => stack.ItemId == itemId);
        if (existingIndex < 0)
        {
            takenStack = default;
            return false;
        }

        var existing = _remainingStacks[existingIndex];
        if (existing.Quantity < quantity)
        {
            takenStack = default;
            return false;
        }

        takenStack = new GroundItemStack(itemId, quantity);
        var remainingQuantity = existing.Quantity - quantity;
        if (remainingQuantity == 0)
        {
            _remainingStacks.RemoveAt(existingIndex);
            return true;
        }

        _remainingStacks[existingIndex] = existing with { Quantity = remainingQuantity };
        return true;
    }

    private static IEnumerable<GroundItemStack> NormalizeStacks(IEnumerable<GroundItemStack>? stacks)
    {
        return (stacks ?? Array.Empty<GroundItemStack>())
            .GroupBy(stack =>
            {
                ArgumentNullException.ThrowIfNull(stack.ItemId);
                if (stack.Quantity < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(stacks), "Container item quantities must be at least 1.");
                }

                return stack.ItemId;
            })
            .Select(group => new GroundItemStack(group.Key, group.Sum(stack => stack.Quantity)));
    }
}

public sealed class WorldObjectContainerStateStore
{
    private readonly Dictionary<WorldObjectInstanceId, WorldObjectContainerState> _states = new();

    public IReadOnlyCollection<WorldObjectContainerState> RealizedContainers => _states.Values.ToArray();

    public bool TryGet(WorldObjectInstanceId instanceId, out WorldObjectContainerState state)
    {
        ArgumentNullException.ThrowIfNull(instanceId);

        if (_states.TryGetValue(instanceId, out var foundState))
        {
            state = foundState;
            return true;
        }

        state = null!;
        return false;
    }

    public WorldObjectContainerState GetOrRealize(
        WorldObjectInstanceId instanceId,
        WorldObjectContainerLootSpec? lootSpec = null)
    {
        ArgumentNullException.ThrowIfNull(instanceId);

        if (TryGet(instanceId, out var state))
        {
            return state;
        }

        state = new WorldObjectContainerState(
            instanceId,
            lootSpec?.FixedStacks ?? Array.Empty<GroundItemStack>()
        );
        _states.Add(instanceId, state);
        return state;
    }
}
