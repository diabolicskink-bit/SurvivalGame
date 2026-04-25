using System.Collections.Generic;
using System.Linq;
using Godot;
using SurvivalGame.Domain;

public partial class GroundItemLayer : Node2D
{
    private const float SpriteMaxWidth = 28.0f;
    private const float SpriteMaxHeight = 18.0f;

    private readonly Dictionary<string, Texture2D?> _spriteCache = new();
    private int _cellSize = 32;
    private TileItemMap? _itemMap;
    private StatefulItemStore? _statefulItems;
    private ItemCatalog? _itemCatalog;
    private string? _siteId;

    public void Configure(
        TileItemMap itemMap,
        ItemCatalog itemCatalog,
        int cellSize,
        StatefulItemStore? statefulItems = null,
        string? siteId = null)
    {
        _itemMap = itemMap;
        _statefulItems = statefulItems;
        _itemCatalog = itemCatalog;
        _cellSize = cellSize;
        _siteId = siteId;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_itemMap is null)
        {
            return;
        }

        foreach (var placedItem in _itemMap.AllItems)
        {
            DrawItemMarker(placedItem);
        }

        if (_statefulItems is null)
        {
            return;
        }

        foreach (var item in _statefulItems.OnGroundInSite(_siteId))
        {
            if (item.Location.Position is not null)
            {
                DrawItemMarker(new PlacedItemStack(item.Location.Position.Value, new GroundItemStack(item.ItemId, item.Quantity)));
            }
        }
    }

    private void DrawItemMarker(PlacedItemStack placedItem)
    {
        var center = CellToBoardPosition(placedItem.Position);
        if (TryGetItemSprite(placedItem.Stack.ItemId, out var sprite))
        {
            DrawItemSprite(center, sprite);
            return;
        }

        var radius = _cellSize * 0.18f;
        var color = GetItemColor(placedItem.Stack.ItemId);

        DrawCircle(center + new Vector2(2, 3), radius, new Color(0.01f, 0.012f, 0.01f, 0.45f));
        DrawCircle(center, radius, color);
        DrawCircle(center, radius * 0.55f, color.Lightened(0.25f));
    }

    private bool TryGetItemSprite(ItemId itemId, out Texture2D sprite)
    {
        sprite = null!;

        if (_itemCatalog is null || !_itemCatalog.TryGet(itemId, out var item) || item.SpriteId is null)
        {
            return false;
        }

        if (!_spriteCache.TryGetValue(item.SpriteId, out var cachedSprite))
        {
            var spritePath = $"res://data/sprites/items/{item.SpriteId}.png";
            cachedSprite = ResourceLoader.Exists(spritePath)
                ? GD.Load<Texture2D>(spritePath)
                : null;
            _spriteCache[item.SpriteId] = cachedSprite;
        }

        if (cachedSprite is null)
        {
            return false;
        }

        sprite = cachedSprite;
        return true;
    }

    private void DrawItemSprite(Vector2 center, Texture2D sprite)
    {
        var textureSize = sprite.GetSize();
        var scale = Mathf.Min(SpriteMaxWidth / textureSize.X, SpriteMaxHeight / textureSize.Y);
        var size = textureSize * scale;
        var rect = new Rect2(center - (size / 2.0f), size);

        DrawRect(new Rect2(rect.Position + new Vector2(2, 3), rect.Size), new Color(0.01f, 0.012f, 0.01f, 0.35f), true);
        DrawTextureRect(sprite, rect, false);
    }

    private Color GetItemColor(ItemId itemId)
    {
        if (_itemCatalog is null || !_itemCatalog.TryGet(itemId, out var item))
        {
            return new Color(0.75f, 0.75f, 0.7f);
        }

        if (item.TypePath.IsA(PrototypeItems.Weapon))
        {
            return new Color(0.78f, 0.28f, 0.26f);
        }

        if (item.TypePath.IsA(PrototypeItems.Food) || item.TypePath.IsA(PrototypeItems.Medical))
        {
            return new Color(0.28f, 0.56f, 0.78f);
        }

        if (item.TypePath.IsA(PrototypeItems.Material))
        {
            return new Color(0.72f, 0.66f, 0.38f);
        }

        return new Color(0.68f, 0.7f, 0.62f);
    }

    private Vector2 CellToBoardPosition(GridPosition cell)
    {
        return new Vector2(
            (cell.X + 0.5f) * _cellSize,
            (cell.Y + 0.5f) * _cellSize
        );
    }
}
