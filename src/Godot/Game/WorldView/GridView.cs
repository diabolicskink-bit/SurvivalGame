using System;
using Godot;
using SurvivalGame.Domain;

public partial class GridView : Node2D
{
    private Vector2I _mapSize = new(19, 13);
    private int _cellSize = 32;
    private TileSurfaceMap? _surfaceMap;
    private TileSurfaceCatalog? _surfaceCatalog;

    public void Configure(Vector2I mapSize, int cellSize)
    {
        _mapSize = mapSize;
        _cellSize = cellSize;
        _surfaceMap = null;
        _surfaceCatalog = null;
        QueueRedraw();
    }

    public void Configure(TileSurfaceMap surfaceMap, TileSurfaceCatalog surfaceCatalog, int cellSize)
    {
        _surfaceMap = surfaceMap;
        _surfaceCatalog = surfaceCatalog;
        _mapSize = new Vector2I(surfaceMap.Bounds.Width, surfaceMap.Bounds.Height);
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
                var rect = new Rect2(x * _cellSize, y * _cellSize, _cellSize, _cellSize);
                DrawRect(rect, GetTileColor(x, y, (x + y) % 2 == 0 ? tileA : tileB), true);
                DrawRect(rect, gridLine, false, 1.0f);
            }
        }
    }

    private Color GetTileColor(int x, int y, Color fallback)
    {
        if (_surfaceMap is null || _surfaceCatalog is null)
        {
            return fallback;
        }

        var surfaceId = _surfaceMap.GetSurfaceId(new GridPosition(x, y));
        if (!_surfaceCatalog.TryGet(surfaceId, out var surface))
        {
            return fallback;
        }

        var color = ParseHtmlColor(surface.MapColor, fallback);
        return (x + y) % 2 == 0 ? color : color.Darkened(0.08f);
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
