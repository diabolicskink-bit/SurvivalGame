using System;
using System.Collections.Generic;
using Godot;
using SurvivalGame.Domain;

public partial class GridView : Node2D
{
    private readonly Dictionary<string, Texture2D?> _spriteCache = new();
    private Vector2I _mapSize = new(19, 13);
    private int _cellSize = 32;
    private GridViewport? _viewport;
    private TileSurfaceMap? _surfaceMap;
    private TileSurfaceCatalog? _surfaceCatalog;

    public void Configure(Vector2I mapSize, int cellSize)
    {
        _mapSize = mapSize;
        _cellSize = cellSize;
        _viewport = GridViewport.Create(
            new GridBounds(mapSize.X, mapSize.Y),
            new GridPosition(mapSize.X / 2, mapSize.Y / 2),
            mapSize.X,
            mapSize.Y
        );
        _surfaceMap = null;
        _surfaceCatalog = null;
        QueueRedraw();
    }

    public void Configure(TileSurfaceMap surfaceMap, TileSurfaceCatalog surfaceCatalog, int cellSize)
    {
        Configure(
            surfaceMap,
            surfaceCatalog,
            cellSize,
            GridViewport.Create(surfaceMap.Bounds, surfaceMap.Bounds.Center, surfaceMap.Bounds.Width, surfaceMap.Bounds.Height)
        );
    }

    public void Configure(TileSurfaceMap surfaceMap, TileSurfaceCatalog surfaceCatalog, int cellSize, GridViewport viewport)
    {
        _surfaceMap = surfaceMap;
        _surfaceCatalog = surfaceCatalog;
        _viewport = viewport;
        _mapSize = new Vector2I(viewport.Width, viewport.Height);
        _cellSize = cellSize;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var tileA = new Color(0.105f, 0.125f, 0.115f);
        var tileB = new Color(0.085f, 0.105f, 0.098f);
        var gridLine = new Color(0.18f, 0.24f, 0.22f);

        for (var y = 0; y < _mapSize.Y; y++)
        {
            for (var x = 0; x < _mapSize.X; x++)
            {
                var viewportPosition = new GridPosition(x, y);
                var rect = new Rect2(x * _cellSize, y * _cellSize, _cellSize, _cellSize);
                if (!TryViewportToMap(viewportPosition, out var mapPosition))
                {
                    DrawRect(rect, GetPaddingColor(x, y), true);
                    DrawRect(rect, gridLine, false, 1.0f);
                    continue;
                }

                if (TryGetSurfaceSprite(mapPosition, out var sprite))
                {
                    DrawTextureRect(sprite, rect, false);
                }
                else
                {
                    DrawRect(rect, GetTileColor(mapPosition, (x + y) % 2 == 0 ? tileA : tileB), true);
                }

                DrawRect(rect, gridLine, false, 1.0f);
            }
        }
    }

    private bool TryViewportToMap(GridPosition viewportPosition, out GridPosition mapPosition)
    {
        if (_viewport is not null)
        {
            return _viewport.Value.TryViewportToMap(viewportPosition, out mapPosition);
        }

        mapPosition = viewportPosition;
        return true;
    }

    private bool TryGetSurfaceSprite(GridPosition position, out Texture2D sprite)
    {
        sprite = null!;

        if (_surfaceMap is null || _surfaceCatalog is null)
        {
            return false;
        }

        var surfaceId = _surfaceMap.GetSurfaceId(position);
        if (!_surfaceCatalog.TryGet(surfaceId, out var surface) || surface.SpriteId is null)
        {
            return false;
        }

        if (!_spriteCache.TryGetValue(surface.SpriteId, out var cachedSprite))
        {
            var spritePath = $"res://data/sprites/surfaces/{surface.SpriteId}.png";
            cachedSprite = LoadSpriteTexture(spritePath);
            _spriteCache[surface.SpriteId] = cachedSprite;
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

    private Color GetTileColor(GridPosition position, Color fallback)
    {
        if (_surfaceMap is null || _surfaceCatalog is null)
        {
            return fallback;
        }

        var surfaceId = _surfaceMap.GetSurfaceId(position);
        if (!_surfaceCatalog.TryGet(surfaceId, out var surface))
        {
            return fallback;
        }

        var color = ParseHtmlColor(surface.MapColor, fallback);
        return (position.X + position.Y) % 2 == 0 ? color : color.Darkened(0.08f);
    }

    private static Color GetPaddingColor(int x, int y)
    {
        var baseColor = (x + y) % 2 == 0
            ? new Color(0.035f, 0.042f, 0.046f)
            : new Color(0.028f, 0.034f, 0.038f);
        return baseColor;
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
