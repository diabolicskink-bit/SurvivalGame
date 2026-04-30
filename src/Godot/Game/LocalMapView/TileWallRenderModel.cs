using System;
using System.Collections.Generic;
using Godot;
using SurvivalGame.Domain;

internal static class TileWallRenderModel
{
    private const float WallHeightTiles = 0.45f;
    private static readonly TileWallNeighborProbe[] NeighborProbes =
    {
        new(0, -1, TileWallNeighbors.North),
        new(1, 0, TileWallNeighbors.East),
        new(0, 1, TileWallNeighbors.South),
        new(-1, 0, TileWallNeighbors.West)
    };

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

    internal static TileWallRenderContext CreateContext(
        IReadOnlyList<PlacedWorldObject> placedObjects,
        GridViewport? viewport,
        int cellSize)
    {
        var tileWallPositions = new HashSet<GridPosition>();
        foreach (var placedObject in placedObjects)
        {
            if (!TryGetKind(placedObject.ObjectId, out _))
            {
                continue;
            }

            foreach (var occupiedPosition in placedObject.OccupiedPositions())
            {
                tileWallPositions.Add(occupiedPosition);
            }
        }

        return new TileWallRenderContext(tileWallPositions, viewport, cellSize);
    }

    internal static TileWallRenderData Create(PlacedWorldObject placedObject, TileWallRenderContext context)
    {
        if (!TryGetKind(placedObject.ObjectId, out var kind))
        {
            throw new ArgumentException(
                $"World object '{placedObject.ObjectId.Value}' is not a tile wall.",
                nameof(placedObject)
            );
        }

        var footprintRect = GetFootprintRect(placedObject, context.Viewport, context.CellSize);
        var neighbors = GetNeighbors(context.TileWallPositions, placedObject.Position);
        var geometry = GetGeometry(footprintRect, neighbors, context.CellSize);
        var body = GetConnectedBody(footprintRect, neighbors, context.CellSize);
        var height = GetHeight(context.CellSize);
        var left = body.Position.X;
        var top = body.Position.Y - height;
        var right = body.End.X;
        var bottom = body.End.Y;
        var renderBounds = new Rect2(left, top, right - left, bottom - top)
            .Grow(GetConnectedOverlap(context.CellSize));
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

    private static TileWallNeighbors GetNeighbors(IReadOnlySet<GridPosition> tileWallPositions, GridPosition position)
    {
        var neighbors = TileWallNeighbors.None;
        foreach (var probe in NeighborProbes)
        {
            var neighborPosition = new GridPosition(position.X + probe.OffsetX, position.Y + probe.OffsetY);
            if (tileWallPositions.Contains(neighborPosition))
            {
                neighbors |= probe.Neighbor;
            }
        }

        return neighbors;
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

internal readonly record struct TileWallRenderContext(
    IReadOnlySet<GridPosition> TileWallPositions,
    GridViewport? Viewport,
    int CellSize
);

internal readonly record struct TileWallNeighborProbe(int OffsetX, int OffsetY, TileWallNeighbors Neighbor);

internal readonly record struct TileWallRenderData(
    TileWallKind Kind,
    TileWallNeighbors Neighbors,
    Rect2 FootprintRect,
    Rect2 RenderBounds,
    TileWallGeometry Geometry,
    TileWallOrientation Orientation,
    float SortFloorContactY
);
