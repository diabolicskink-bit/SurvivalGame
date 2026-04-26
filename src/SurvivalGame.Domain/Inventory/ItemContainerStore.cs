namespace SurvivalGame.Domain;

public sealed class ItemContainerStore
{
    private readonly Dictionary<ContainerId, ItemContainer> _containers = new();

    public IReadOnlyCollection<ItemContainer> Containers => _containers.Values.ToArray();

    public void Add(ItemContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);

        if (!_containers.TryAdd(container.Id, container))
        {
            throw new InvalidOperationException($"Container '{container.Id}' is already tracked.");
        }
    }

    public bool TryGet(ContainerId id, out ItemContainer container)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (_containers.TryGetValue(id, out var foundContainer))
        {
            container = foundContainer;
            return true;
        }

        container = null!;
        return false;
    }

    public ItemContainer Get(ContainerId id)
    {
        if (TryGet(id, out var container))
        {
            return container;
        }

        throw new KeyNotFoundException($"Container '{id}' is not tracked.");
    }
}
