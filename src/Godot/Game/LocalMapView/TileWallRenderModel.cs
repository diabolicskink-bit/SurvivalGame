using System;
using Godot;
using SurvivalGame.Domain;

internal static class TileWallRenderModel
{
    private const float WallHeightTiles = 0.45f;

    internal static bool TryGetKind(WorldObjectId objectId, out TileWallKind kind)
    {
        kind = objectId.Value switch
        {
            "wall" => TileWallKind.Solid,
            "wooden_door" => TileWallKind.WoodenDoor,
            "window" => TileWallKind.Window,
            "glass_door" => TileWallKind.GlassDoor,
            _ => TileWallKind.None
        };

        return kind != TileWallKind.None;
    }

    internal static TileWallRenderData Create(
        PlacedWorldObject placedObject,
        TileObjectMap objectMap,
        GridViewport? viewport,
        int cellSize)
    {
        if (!TryGetKind(placedObject.ObjectId, out var kind))
        {
            throw new ArgumentException(
                $"World object '{placedObject.ObjectId.Value}' is not a tile wall.",
                nameof(placedObject)
            );
        }

        var footprintRect = GetFootprintRect(placedObject, viewport, cellSize);
        var neighbors = GetNeighbors(objectMap, placedObject.Position);
        var geometry = GetGeometry(footprintRect, neighbors, cellSize);
        var body = GetConnectedBody(footprintRect, neighbors, cellSize);
        var height = GetHeight(cellSize);
        var left = body.Position.X;
        var top = body.Position.Y - height;
        var right = body.End.X;
        var bottom = body.End.Y;
        var renderBounds = new Rect2(left, top, right - left, bottom - top)
            .Grow(GetConnectedOverlap(cellSize));
        var sortFloorContactY = placedObject.Position.Y + placedObject.EffectiveFootprint.Height - 1;

        return new TileWallRenderData(
            kind,
            neighbors,
            footprintRect,
            renderBounds,
            geometry,
            ResolveOrientation(neighbors),
            sortFloorContactY
        );
    }

    internal static TileWallGeometry GetGeometry(Rect2 rect, TileWallNeighbors neighbors, int cellSize)
    {
        return GetGeometry(rect, neighbors, cellSize, GetConnectedOverlap(cellSize));
    }

    internal static Rect2 GetConnectedBody(Rect2 rect, TileWallNeighbors neighbors, int cellSize)
    {
        return GetConnectedBody(rect, neighbors, GetConnectedOverlap(cellSize));
    }

    internal static float GetHeight(int cellSize)
    {
        return Mathf.Max(8.0f, Mathf.Round(cellSize * WallHeightTiles));
    }

    internal static TileWallOrientation ResolveOrientation(TileWallNeighbors neighbors)
    {
        var horizontalCount = (neighbors.HasFlag(TileWallNeighbors.West) ? 1 : 0)
            + (neighbors.HasFlag(TileWallNeighbors.East) ? 1 : 0);
        var verticalCount = (neighbors.HasFlag(TileWallNeighbors.North) ? 1 : 0)
            + (neighbors.HasFlag(TileWallNeighbors.South) ? 1 : 0);

        return verticalCount > horizontalCount
            ? TileWallOrientation.Vertical
            : TileWallOrientation.Horizontal;
    }

    private static TileWallNeighbors GetNeighbors(TileObjectMap objectMap, GridPosition position)
    {
        var neighbors = TileWallNeighbors.None;

        if (HasTileWallAt(objectMap, new GridPosition(position.X, position.Y - 1)))
        {
            neighbors |= TileWallNeighbors.North;
        }

        if (HasTileWallAt(objectMap, new GridPosition(position.X + 1, position.Y)))
        {
            neighbors |= TileWallNeighbors.East;
        }

        if (HasTileWallAt(objectMap, new GridPosition(position.X, position.Y + 1)))
        {
            neighbors |= TileWallNeighbors.South;
        }

        if (HasTileWallAt(objectMap, new GridPosition(position.X - 1, position.Y)))
        {
            neighbors |= TileWallNeighbors.West;
        }

        return neighbors;
    }

    private static bool HasTileWallAt(TileObjectMap objectMap, GridPosition position)
    {
        return objectMap.TryGetObjectAt(position, out var objectId)
            && TryGetKind(objectId, out _);
    }

    private static TileWallGeometry GetGeometry(
        Rect2 rect,
        TileWallNeighbors neighbors,
        int cellSize,
        float connectedOverlap)
    {
        var footprint = GetConnectedBody(rect, neighbors, connectedOverlap);
        var height = GetHeight(cellSize);
        var top = new Rect2(
            footprint.Position - new Vector2(0.0f, height),
            footprint.Size
        );
        var frontFace = new Rect2(
            new Vector2(footprint.Position.X, footprint.End.Y - height),
            new Vector2(footprint.Size.X, height)
        );

        return new TileWallGeometry(footprint, top, frontFace);
    }

    private static Rect2 GetConnectedBody(Rect2 rect, TileWallNeighbors neighbors, float connectedOverlap)
    {
        var left = neighbors.HasFlag(TileWallNeighbors.West) ? rect.Position.X - connectedOverlap : rect.Position.X;
        var top = neighbors.HasFlag(TileWallNeighbors.North) ? rect.Position.Y - connectedOverlap : rect.Position.Y;
        var right = neighbors.HasFlag(TileWallNeighbors.East) ? rect.End.X + connectedOverlap : rect.End.X;
        var bottom = neighbors.HasFlag(TileWallNeighbors.South) ? rect.End.Y + connectedOverlap : rect.End.Y;

        return new Rect2(left, top, right - left, bottom - top);
    }

    private static float GetConnectedOverlap(int cellSize)
    {
        return Mathf.Max(1.0f, Mathf.Round(cellSize * 0.04f));
    }

    private static Rect2 GetFootprintRect(PlacedWorldObject placedObject, GridViewport? viewport, int cellSize)
    {
        var viewportPosition = MapToViewportUnchecked(placedObject.Position, viewport);
        var footprint = placedObject.EffectiveFootprint;
        return new Rect2(
            viewportPosition.X * cellSize,
            viewportPosition.Y * cellSize,
            footprint.Width * cellSize,
            footprint.Height * cellSize
        );
    }

    private static GridPosition MapToViewportUnchecked(GridPosition mapPosition, GridViewport? viewport)
    {
        return viewport is null
            ? mapPosition
            : new GridPosition(mapPosition.X - viewport.Value.Origin.X, mapPosition.Y - viewport.Value.Origin.Y);
    }
}

internal enum TileWallKind
{
    None,
    Solid,
    WoodenDoor,
    Window,
    GlassDoor
}

[Flags]
internal enum TileWallNeighbors
{
    None = 0,
    North = 1,
    East = 2,
    South = 4,
    West = 8
}

internal enum TileWallOrientation
{
    Horizontal,
    Vertical
}

internal readonly record struct TileWallGeometry(Rect2 Footprint, Rect2 Top, Rect2 FrontFace);

internal readonly record struct TileWallRenderData(
    TileWallKind Kind,
    TileWallNeighbors Neighbors,
    Rect2 FootprintRect,
    Rect2 RenderBounds,
    TileWallGeometry Geometry,
    TileWallOrientation Orientation,
    float SortFloorContactY
);
