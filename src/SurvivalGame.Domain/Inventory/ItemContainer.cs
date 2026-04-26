namespace SurvivalGame.Domain;

public sealed class ItemContainer
{
    private readonly Dictionary<ContainerItemRef, ItemContainerPlacement> _placements = new();

    public ItemContainer(ContainerId id, string displayName, InventoryItemSize size)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Container display name cannot be empty.", nameof(displayName));
        }

        Id = id;
        DisplayName = displayName.Trim();
        Width = size.Width;
        Height = size.Height;
    }

    private ItemContainer(ContainerId id, string displayName, int width, int height)
    {
        Id = id;
        DisplayName = displayName;
        Width = width;
        Height = height;
    }

    public ContainerId Id { get; }

    public string DisplayName { get; }

    public int Width { get; }

    public int Height { get; }

    public IReadOnlyList<ItemContainerPlacement> Placements => _placements.Values
        .OrderBy(placement => placement.Position.Y)
        .ThenBy(placement => placement.Position.X)
        .ThenBy(placement => placement.Item.ToString())
        .ToArray();

    public bool IsEmpty => _placements.Count == 0;

    public bool Contains(ContainerItemRef item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return _placements.ContainsKey(item);
    }

    public bool TryGetPlacement(ContainerItemRef item, out ItemContainerPlacement placement)
    {
        ArgumentNullException.ThrowIfNull(item);
        return _placements.TryGetValue(item, out placement!);
    }

    public bool CanPlace(InventoryItemSize size, InventoryGridPosition position, ContainerItemRef? ignoredItem = null)
    {
        if (!IsInsideBounds(size, position))
        {
            return false;
        }

        return _placements.Values.All(existing =>
            ignoredItem is not null && existing.Item == ignoredItem
                || !Overlaps(position, size, existing.Position, existing.Size)
        );
    }

    public bool HasSpaceFor(InventoryItemSize size)
    {
        return FindFirstOpenPosition(size) is not null;
    }

    public bool TryPlace(ContainerItemRef item, InventoryItemSize size, InventoryGridPosition position)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (_placements.ContainsKey(item) || !CanPlace(size, position))
        {
            return false;
        }

        _placements.Add(item, new ItemContainerPlacement(item, position, size));
        return true;
    }

    public bool TryAutoPlace(ContainerItemRef item, InventoryItemSize size)
    {
        var position = FindFirstOpenPosition(size);
        return position is not null && TryPlace(item, size, position.Value);
    }

    public bool TryMove(ContainerItemRef item, InventoryGridPosition position)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!_placements.TryGetValue(item, out var existing) || !CanPlace(existing.Size, position, item))
        {
            return false;
        }

        _placements[item] = existing with { Position = position };
        return true;
    }

    public bool Remove(ContainerItemRef item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return _placements.Remove(item);
    }

    public ItemContainer Copy()
    {
        var copy = new ItemContainer(Id, DisplayName, Width, Height);
        foreach (var placement in _placements.Values)
        {
            copy._placements.Add(placement.Item, placement);
        }

        return copy;
    }

    private InventoryGridPosition? FindFirstOpenPosition(InventoryItemSize size)
    {
        for (var y = 0; y <= Height - size.Height; y++)
        {
            for (var x = 0; x <= Width - size.Width; x++)
            {
                var position = new InventoryGridPosition(x, y);
                if (CanPlace(size, position))
                {
                    return position;
                }
            }
        }

        return null;
    }

    private bool IsInsideBounds(InventoryItemSize size, InventoryGridPosition position)
    {
        return position.X + size.Width <= Width
            && position.Y + size.Height <= Height;
    }

    private static bool Overlaps(
        InventoryGridPosition leftPosition,
        InventoryItemSize leftSize,
        InventoryGridPosition rightPosition,
        InventoryItemSize rightSize)
    {
        return leftPosition.X < rightPosition.X + rightSize.Width
            && leftPosition.X + leftSize.Width > rightPosition.X
            && leftPosition.Y < rightPosition.Y + rightSize.Height
            && leftPosition.Y + leftSize.Height > rightPosition.Y;
    }
}
