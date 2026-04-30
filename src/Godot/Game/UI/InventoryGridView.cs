using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SurvivalGame.Domain;

public partial class InventoryGridView : Control
{
    private const int CellCountX = 20;
    private const int CellCountY = 10;
    private const int ItemFontSize = 12;
    private const float CellGap = 1f;

    private readonly Dictionary<string, Texture2D?> _spriteCache = new();
    private readonly List<GridVisualItem> _items = new();
    private SelectedItemRef? _hoveredItem;

    public event Action<SelectedItemRef, Vector2>? ItemActionRequested;
    public event Action<SelectedItemRef, Vector2>? ItemHovered;
    public event Action<SelectedItemRef>? ItemHoverEnded;

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(560, 280);
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        SizeFlagsVertical = SizeFlags.ExpandFill;
        MouseFilter = MouseFilterEnum.Stop;
        MouseExited += ClearHoveredItem;
    }

    public void Display(
        PlayerInventory inventory,
        ItemCatalog itemCatalog,
        StatefulItemStore statefulItems,
        SelectedItemRef? selectedItem,
        Func<ItemId, bool> includeItem)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(itemCatalog);
        ArgumentNullException.ThrowIfNull(statefulItems);
        ArgumentNullException.ThrowIfNull(includeItem);

        _items.Clear();

        var previewContainer = new ItemContainer(
            new ContainerId("inventory_panel_preview"),
            "Inventory Preview",
            PlayerInventory.InventorySize
        );

        foreach (var stack in inventory.Items.Where(stack => includeItem(stack.ItemId)))
        {
            var itemRef = ContainerItemRef.Stack(stack.ItemId);
            var placement = inventory.Container.TryGetPlacement(itemRef, out var foundPlacement)
                ? foundPlacement
                : null;
            if (placement is null)
            {
                continue;
            }

            previewContainer.TryPlace(itemRef, placement.Size, placement.Position);
            var selectedRef = SelectedItemRef.InventoryStack(stack.ItemId);
            _items.Add(new GridVisualItem(
                selectedRef,
                FormatStack(stack, itemCatalog),
                GetItemSpriteId(stack.ItemId, itemCatalog),
                placement.Position,
                placement.Size,
                GetItemColor(stack.ItemId, itemCatalog),
                selectedItem == selectedRef
            ));
        }

        foreach (var item in statefulItems.InPlayerInventory().Where(item => includeItem(item.ItemId)))
        {
            var itemRef = ContainerItemRef.Stateful(item.Id);
            ItemContainerPlacement? placement = inventory.Container.TryGetPlacement(itemRef, out var foundPlacement)
                ? foundPlacement
                : null;

            if (placement is null)
            {
                var size = GetInventorySize(item.ItemId, itemCatalog);
                if (!previewContainer.TryAutoPlace(itemRef, size)
                    || !previewContainer.TryGetPlacement(itemRef, out var previewPlacement))
                {
                    continue;
                }

                placement = previewPlacement;
            }
            else
            {
                previewContainer.TryPlace(itemRef, placement.Size, placement.Position);
            }

            var selectedRef = SelectedItemRef.StatefulItem(item.Id);
            _items.Add(new GridVisualItem(
                selectedRef,
                FormatStatefulItem(item, itemCatalog),
                GetItemSpriteId(item.ItemId, itemCatalog),
                placement.Position,
                placement.Size,
                GetItemColor(item.ItemId, itemCatalog),
                selectedItem == selectedRef
            ));
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        var metrics = CalculateMetrics();
        var gridRect = new Rect2(metrics.Origin, new Vector2(metrics.CellSize * CellCountX, metrics.CellSize * CellCountY));
        DrawRect(gridRect, new Color(0.035f, 0.044f, 0.046f, 0.98f), filled: true);
        DrawRect(gridRect, new Color(0.18f, 0.24f, 0.22f), filled: false, width: 1f);

        for (var y = 0; y < CellCountY; y++)
        {
            for (var x = 0; x < CellCountX; x++)
            {
                var cellRect = new Rect2(
                    metrics.Origin + new Vector2(x * metrics.CellSize, y * metrics.CellSize),
                    new Vector2(metrics.CellSize, metrics.CellSize)
                ).Grow(-CellGap);
                DrawRect(cellRect, new Color(0.07f, 0.083f, 0.078f, 0.86f), filled: true);
                DrawRect(cellRect, new Color(0.13f, 0.17f, 0.155f, 0.85f), filled: false, width: 1f);
            }
        }

        foreach (var item in _items)
        {
            DrawItem(item, metrics);
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            UpdateHoveredItem(mouseMotion.Position);
            return;
        }

        if (@event is not InputEventMouseButton mouseButton
            || !mouseButton.Pressed
            || mouseButton.ButtonIndex != MouseButton.Right)
        {
            return;
        }

        if (TryGetItemAt(mouseButton.Position, out var item))
        {
            ItemActionRequested?.Invoke(item.Ref, GetViewport().GetMousePosition());
            AcceptEvent();
        }
    }

    private void DrawItem(GridVisualItem item, GridMetrics metrics)
    {
        var rect = GetItemRect(item, metrics).Grow(-2f);
        var hasSprite = TryGetItemSprite(item.SpriteId, out var sprite);
        var fill = hasSprite
            ? new Color(0.075f, 0.087f, 0.082f, 0.96f)
            : item.Selected
                ? item.Color.Lightened(0.18f)
                : item.Color;
        var border = item.Selected
            ? new Color(0.92f, 0.95f, 0.82f)
            : hasSprite
                ? item.Color.Lightened(0.16f)
                : new Color(0.22f, 0.30f, 0.27f);

        DrawRect(rect, fill, filled: true);
        DrawRect(rect, border, filled: false, width: item.Selected ? 2f : 1f);

        if (hasSprite)
        {
            DrawItemSprite(sprite, rect);
            return;
        }

        var textPosition = rect.Position + new Vector2(5, Math.Min(16, rect.Size.Y - 5));
        DrawString(
            ThemeDB.FallbackFont,
            textPosition,
            item.Label,
            HorizontalAlignment.Left,
            width: Math.Max(8, rect.Size.X - 8),
            fontSize: ItemFontSize,
            modulate: new Color(0.92f, 0.96f, 0.9f)
        );
    }

    private bool TryGetItemSprite(string? spriteId, out Texture2D sprite)
    {
        sprite = null!;
        if (string.IsNullOrWhiteSpace(spriteId))
        {
            return false;
        }

        if (!_spriteCache.TryGetValue(spriteId, out var cachedSprite))
        {
            var spritePath = $"res://data/sprites/items/{spriteId}.png";
            cachedSprite = LoadSpriteTexture(spritePath);
            _spriteCache[spriteId] = cachedSprite;
        }

        if (cachedSprite is null)
        {
            return false;
        }

        sprite = cachedSprite;
        return true;
    }

    private static Texture2D? LoadSpriteTexture(string spritePath)
    {
        if (ResourceLoader.Exists(spritePath))
        {
            return GD.Load<Texture2D>(spritePath);
        }

        var image = Image.LoadFromFile(ProjectSettings.GlobalizePath(spritePath));
        return image is null || image.IsEmpty()
            ? null
            : ImageTexture.CreateFromImage(image);
    }

    private void DrawItemSprite(Texture2D sprite, Rect2 itemRect)
    {
        var spriteArea = itemRect.Grow(-4f);
        if (spriteArea.Size.X <= 0 || spriteArea.Size.Y <= 0)
        {
            return;
        }

        var size = GetAspectFitSize(sprite.GetSize(), spriteArea.Size);
        var rect = new Rect2(
            spriteArea.Position + ((spriteArea.Size - size) / 2f),
            size
        );

        DrawTextureRect(sprite, rect, false);
    }

    private Rect2 GetItemRect(GridVisualItem item, GridMetrics metrics)
    {
        return new Rect2(
            metrics.Origin + new Vector2(item.Position.X * metrics.CellSize, item.Position.Y * metrics.CellSize),
            new Vector2(item.Size.Width * metrics.CellSize, item.Size.Height * metrics.CellSize)
        );
    }

    private void UpdateHoveredItem(Vector2 localPosition)
    {
        if (TryGetItemAt(localPosition, out var item))
        {
            if (_hoveredItem != item.Ref)
            {
                ClearHoveredItem();
                _hoveredItem = item.Ref;
            }

            ItemHovered?.Invoke(item.Ref, GetViewport().GetMousePosition());
            return;
        }

        ClearHoveredItem();
    }

    private bool TryGetItemAt(Vector2 localPosition, out GridVisualItem item)
    {
        var metrics = CalculateMetrics();
        foreach (var candidate in _items.AsEnumerable().Reverse())
        {
            if (GetItemRect(candidate, metrics).HasPoint(localPosition))
            {
                item = candidate;
                return true;
            }
        }

        item = null!;
        return false;
    }

    private void ClearHoveredItem()
    {
        if (_hoveredItem is null)
        {
            return;
        }

        var previousItem = _hoveredItem;
        _hoveredItem = null;
        ItemHoverEnded?.Invoke(previousItem);
    }

    private GridMetrics CalculateMetrics()
    {
        var availableWidth = Math.Max(1, Size.X);
        var availableHeight = Math.Max(1, Size.Y);
        var cellSize = MathF.Floor(Math.Min(availableWidth / CellCountX, availableHeight / CellCountY));
        cellSize = Math.Max(14, cellSize);
        var gridSize = new Vector2(cellSize * CellCountX, cellSize * CellCountY);
        var origin = new Vector2(
            Math.Max(0, (availableWidth - gridSize.X) / 2f),
            Math.Max(0, (availableHeight - gridSize.Y) / 2f)
        );

        return new GridMetrics(origin, cellSize);
    }

    private static string FormatStack(InventoryItemStack stack, ItemCatalog itemCatalog)
    {
        var itemName = stack.ItemId.ToString();
        if (itemCatalog.TryGet(stack.ItemId, out var item))
        {
            itemName = item.DisplayName;
        }

        return $"{itemName} x{stack.Quantity}";
    }

    private static string FormatStatefulItem(StatefulItem item, ItemCatalog itemCatalog)
    {
        var itemName = item.ItemId.ToString();
        if (itemCatalog.TryGet(item.ItemId, out var definition))
        {
            itemName = definition.DisplayName;
        }

        return item.FeedDevice is null
            ? $"{itemName} [{item.Id}]"
            : $"{itemName} [{item.Id}] {item.FeedDevice.LoadedCount}/{item.FeedDevice.Capacity}";
    }

    private static InventoryItemSize GetInventorySize(ItemId itemId, ItemCatalog itemCatalog)
    {
        return itemCatalog.TryGet(itemId, out var definition)
            ? definition.InventorySize
            : InventoryItemSize.Default;
    }

    private static string? GetItemSpriteId(ItemId itemId, ItemCatalog itemCatalog)
    {
        return itemCatalog.TryGet(itemId, out var definition)
            ? definition.SpriteId
            : null;
    }

    private static Vector2 GetAspectFitSize(Vector2 textureSize, Vector2 maxSize)
    {
        var scale = Mathf.Min(maxSize.X / textureSize.X, maxSize.Y / textureSize.Y);
        return textureSize * scale;
    }

    private static Color GetItemColor(ItemId itemId, ItemCatalog itemCatalog)
    {
        if (!itemCatalog.TryGet(itemId, out var item))
        {
            return new Color(0.24f, 0.28f, 0.25f);
        }

        return item.Category.ToLowerInvariant() switch
        {
            "weapon" => new Color(0.42f, 0.24f, 0.22f),
            "ammunition" => new Color(0.42f, 0.36f, 0.18f),
            "feeddevice" => new Color(0.35f, 0.31f, 0.25f),
            "food" => new Color(0.22f, 0.36f, 0.27f),
            "medical" => new Color(0.32f, 0.42f, 0.38f),
            "clothing" => new Color(0.24f, 0.30f, 0.42f),
            "armor" => new Color(0.30f, 0.30f, 0.34f),
            "container" => new Color(0.30f, 0.25f, 0.18f),
            "tool" => new Color(0.32f, 0.30f, 0.20f),
            _ => new Color(0.24f, 0.28f, 0.25f)
        };
    }

    private sealed record GridVisualItem(
        SelectedItemRef Ref,
        string Label,
        string? SpriteId,
        InventoryGridPosition Position,
        InventoryItemSize Size,
        Color Color,
        bool Selected
    );

    private readonly record struct GridMetrics(Vector2 Origin, float CellSize);
}
