using System;
using System.Collections.Generic;
using Godot;
using SurvivalGame.Domain;

public partial class WorldObjectLayer : Node2D
{
    private const float SpriteMaxSizeRatio = 0.84f;

    private readonly Dictionary<string, Texture2D?> _spriteCache = new();
    private int _cellSize = 32;
    private TileObjectMap? _objectMap;
    private WorldObjectCatalog? _objectCatalog;

    public void Configure(TileObjectMap objectMap, WorldObjectCatalog objectCatalog, int cellSize)
    {
        _objectMap = objectMap;
        _objectCatalog = objectCatalog;
        _cellSize = cellSize;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_objectMap is null)
        {
            return;
        }

        foreach (var placedObject in _objectMap.AllObjects)
        {
            DrawObjectMarker(placedObject);
        }
    }

    private void DrawObjectMarker(PlacedWorldObject placedObject)
    {
        var center = CellToBoardPosition(placedObject.Position);
        if (TryGetObjectSprite(placedObject.ObjectId, out var sprite))
        {
            DrawObjectSprite(center, sprite);
            return;
        }

        var rect = new Rect2(
            placedObject.Position.X * _cellSize,
            placedObject.Position.Y * _cellSize,
            _cellSize,
            _cellSize
        );
        var inset = Mathf.Max(3.0f, _cellSize * 0.12f);
        var objectRect = rect.Grow(-inset);
        var color = GetObjectColor(placedObject.ObjectId);

        DrawRect(
            new Rect2(objectRect.Position + new Vector2(2, 3), objectRect.Size),
            new Color(0.01f, 0.012f, 0.01f, 0.35f),
            true
        );
        DrawRect(objectRect, color, true);
        DrawRect(objectRect, color.Lightened(0.25f), false, 1.5f);
    }

    private bool TryGetObjectSprite(WorldObjectId objectId, out Texture2D sprite)
    {
        sprite = null!;

        if (_objectCatalog is null || !_objectCatalog.TryGet(objectId, out var worldObject) || worldObject.SpriteId is null)
        {
            return false;
        }

        if (!_spriteCache.TryGetValue(worldObject.SpriteId, out var cachedSprite))
        {
            var spritePath = $"res://data/sprites/world_objects/{worldObject.SpriteId}.png";
            cachedSprite = LoadSpriteTexture(spritePath);
            _spriteCache[worldObject.SpriteId] = cachedSprite;
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

    private void DrawObjectSprite(Vector2 center, Texture2D sprite)
    {
        var textureSize = sprite.GetSize();
        var maxSize = _cellSize * SpriteMaxSizeRatio;
        var scale = Mathf.Min(maxSize / textureSize.X, maxSize / textureSize.Y);
        var size = textureSize * scale;
        var rect = new Rect2(center - (size / 2.0f), size);

        DrawTextureRect(sprite, rect, false);
    }

    private Color GetObjectColor(WorldObjectId objectId)
    {
        if (_objectCatalog is null || !_objectCatalog.TryGet(objectId, out var worldObject))
        {
            return new Color(0.55f, 0.58f, 0.52f);
        }

        return ParseHtmlColor(worldObject.MapColor, new Color(0.55f, 0.58f, 0.52f));
    }

    private Vector2 CellToBoardPosition(GridPosition cell)
    {
        return new Vector2(
            (cell.X + 0.5f) * _cellSize,
            (cell.Y + 0.5f) * _cellSize
        );
    }

    private static Color ParseHtmlColor(string? value, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var hex = value.Trim().TrimStart('#');
        if (hex.Length != 6)
        {
            return fallback;
        }

        try
        {
            var red = Convert.ToInt32(hex.Substring(0, 2), 16) / 255.0f;
            var green = Convert.ToInt32(hex.Substring(2, 2), 16) / 255.0f;
            var blue = Convert.ToInt32(hex.Substring(4, 2), 16) / 255.0f;
            return new Color(red, green, blue);
        }
        catch (FormatException)
        {
            return fallback;
        }
    }
}
