using System;
using System.Collections.Generic;
using Godot;
using SurvivalGame.Domain;

public partial class MapEntityLayer : Node2D
{
    private static readonly SpriteRenderProfile DefaultNpcSpriteRender = new(0.9f, 0.9f);
    private static readonly Color TileGlassColor = new(0.45f, 0.7f, 0.8f, 0.82f);

    private readonly Dictionary<string, Texture2D?> _objectSpriteCache = new();
    private readonly Dictionary<string, Texture2D?> _structureSpriteCache = new();
    private readonly Dictionary<string, Texture2D?> _npcSpriteCache = new();
    private readonly StructureRenderResolver _structureRenderResolver = new();
    private int _cellSize = 32;
    private GridViewport? _viewport;
    private TileObjectMap? _objectMap;
    private WorldObjectCatalog? _objectCatalog;
    private StructureEdgeMap? _structureMap;
    private StructureCatalog? _structureCatalog;
    private NpcRoster? _npcs;
    private NpcCatalog? _npcCatalog;
    private PlayerState? _player;

    public void Configure(
        TileObjectMap objectMap,
        WorldObjectCatalog objectCatalog,
        StructureEdgeMap structureMap,
        StructureCatalog structureCatalog,
        NpcRoster npcs,
        NpcCatalog npcCatalog,
        PlayerState player,
        int cellSize,
        GridViewport viewport)
    {
        _objectMap = objectMap;
        _objectCatalog = objectCatalog;
        _structureMap = structureMap;
        _structureCatalog = structureCatalog;
        _npcs = npcs;
        _npcCatalog = npcCatalog;
        _player = player;
        _cellSize = cellSize;
        _viewport = viewport;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var commands = new List<EntityDrawCommand>();
        AddStructureCommands(commands);
        AddWorldObjectCommands(commands);
        AddNpcCommands(commands);
        AddPlayerCommand(commands);

        commands.Sort((left, right) =>
        {
            var sortComparison = left.SortKey.CompareTo(right.SortKey);
            return sortComparison != 0
                ? sortComparison
                : left.Priority.CompareTo(right.Priority);
        });

        foreach (var command in commands)
        {
            command.Draw();
        }
    }

    private void AddWorldObjectCommands(List<EntityDrawCommand> commands)
    {
        if (_objectMap is null)
        {
            return;
        }

        foreach (var placedObject in _objectMap.AllObjects)
        {
            var isTileWall = TileWallRenderModel.TryGetKind(placedObject.ObjectId, out _);
            if (!isTileWall && IsRenderedAsEdgeStructure(placedObject.ObjectId))
            {
                continue;
            }

            TileWallRenderData? tileWall = isTileWall
                ? TileWallRenderModel.Create(placedObject, _objectMap, _viewport, _cellSize)
                : null;
            var definition = TryGetWorldObjectDefinition(placedObject.ObjectId);
            var render = GetObjectRenderProfile(placedObject, definition);
            Texture2D? sprite = null;
            if (!isTileWall && TryGetObjectSprite(definition, out var loadedSprite))
            {
                sprite = loadedSprite;
            }

            var rect = tileWall is { } wall
                ? wall.RenderBounds
                : sprite is not null
                    ? GetWorldObjectRenderBounds(placedObject, render, sprite)
                    : GetWorldObjectFootprintRect(placedObject);
            if (!IntersectsViewport(rect))
            {
                continue;
            }

            var sortFloorContactY = tileWall is { } sortWall
                ? sortWall.SortFloorContactY
                : placedObject.Position.Y + placedObject.EffectiveFootprint.Height - 1;
            var sortKey = sortFloorContactY + render.SortOffsetYTiles;
            commands.Add(new EntityDrawCommand(
                sortKey,
                Priority: 0,
                () => DrawWorldObject(placedObject, definition, render, sprite, tileWall)
            ));
        }
    }

    private bool IsRenderedAsEdgeStructure(WorldObjectId objectId)
    {
        return _structureMap is { IsEmpty: false }
            && _structureCatalog is not null
            && _structureCatalog.TryGet(new StructureId(objectId.Value), out _);
    }

    private void AddStructureCommands(List<EntityDrawCommand> commands)
    {
        if (_structureMap is null || _structureCatalog is null)
        {
            return;
        }

        foreach (var edge in _structureMap.AllEdges)
        {
            var renderInfo = _structureRenderResolver.Resolve(edge, _structureMap, _structureCatalog);
            var rect = GetStructureRenderBounds(renderInfo);
            if (!IntersectsViewport(rect))
            {
                continue;
            }

            var sortKey = GetStructureSortKey(renderInfo);
            commands.Add(new EntityDrawCommand(
                sortKey,
                Priority: -1,
                () => DrawStructure(renderInfo)
            ));
        }
    }

    private void AddNpcCommands(List<EntityDrawCommand> commands)
    {
        if (_npcs is null)
        {
            return;
        }

        foreach (var npc in _npcs.AllNpcs)
        {
            var definition = TryGetNpcDefinition(npc.DefinitionId);
            var render = definition?.SpriteRender ?? DefaultNpcSpriteRender;
            if (!TryGetNpcSprite(definition, out var sprite))
            {
                render = new SpriteRenderProfile(1f, 1f);
            }

            var rect = GetRenderRect(npc.Position, render, sprite);
            if (!IntersectsViewport(rect))
            {
                continue;
            }

            commands.Add(new EntityDrawCommand(
                npc.Position.Y + render.SortOffsetYTiles,
                Priority: 1,
                () => DrawNpc(npc, definition, render, sprite)
            ));
        }
    }

    private void AddPlayerCommand(List<EntityDrawCommand> commands)
    {
        if (_player is null || !TryMapToViewport(_player.Position, out _))
        {
            return;
        }

        commands.Add(new EntityDrawCommand(
            _player.Position.Y + 0.5f,
            Priority: 2,
            () => DrawPlayerMarker(_player.Position)
        ));
    }

    private WorldObjectDefinition? TryGetWorldObjectDefinition(WorldObjectId objectId)
    {
        return _objectCatalog is not null && _objectCatalog.TryGet(objectId, out var definition)
            ? definition
            : null;
    }

    private NpcDefinition? TryGetNpcDefinition(NpcDefinitionId definitionId)
    {
        return _npcCatalog is not null && _npcCatalog.TryGet(definitionId, out var definition)
            ? definition
            : null;
    }

    private bool TryGetObjectSprite(WorldObjectDefinition? definition, out Texture2D sprite)
    {
        sprite = null!;
        if (definition?.SpriteId is null)
        {
            return false;
        }

        if (!_objectSpriteCache.TryGetValue(definition.SpriteId, out var cachedSprite))
        {
            var spritePath = $"res://data/sprites/world_objects/{definition.SpriteId}.png";
            cachedSprite = LoadSpriteTexture(spritePath);
            _objectSpriteCache[definition.SpriteId] = cachedSprite;
        }

        if (cachedSprite is null)
        {
            return false;
        }

        sprite = cachedSprite;
        return true;
    }

    private bool TryGetStructureSprite(StructureRenderInfo renderInfo, out Texture2D sprite)
    {
        sprite = null!;
        var spriteId = renderInfo.SpriteId;

        if (!_structureSpriteCache.TryGetValue(spriteId, out var cachedSprite))
        {
            var spritePath = $"res://data/sprites/structures/{spriteId}.png";
            cachedSprite = LoadSpriteTexture(spritePath);
            _structureSpriteCache[spriteId] = cachedSprite;
        }

        if (cachedSprite is null)
        {
            return false;
        }

        sprite = cachedSprite;
        return true;
    }

    private bool TryGetNpcSprite(NpcDefinition? definition, out Texture2D sprite)
    {
        sprite = null!;
        if (definition?.SpriteId is null)
        {
            return false;
        }

        if (!_npcSpriteCache.TryGetValue(definition.SpriteId, out var cachedSprite))
        {
            var spritePath = $"res://data/sprites/npcs/{definition.SpriteId}.png";
            cachedSprite = LoadSpriteTexture(spritePath);
            _npcSpriteCache[definition.SpriteId] = cachedSprite;
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

    private void DrawWorldObject(
        PlacedWorldObject placedObject,
        WorldObjectDefinition? definition,
        SpriteRenderProfile render,
        Texture2D? sprite,
        TileWallRenderData? tileWall)
    {
        if (tileWall is { } wall)
        {
            DrawTileWallObject(wall, definition);
            return;
        }

        if (sprite is not null)
        {
            DrawWorldObjectSprite(placedObject, render, sprite);
            return;
        }

        var rect = GetWorldObjectFootprintRect(placedObject);
        var inset = Mathf.Min(
            Mathf.Max(3.0f, _cellSize * 0.12f),
            Mathf.Min(rect.Size.X, rect.Size.Y) * 0.18f
        );
        var objectRect = rect.Grow(-inset);
        var color = definition is null
            ? new Color(0.55f, 0.58f, 0.52f)
            : ParseHtmlColor(definition.MapColor, new Color(0.55f, 0.58f, 0.52f));

        DrawRect(
            new Rect2(objectRect.Position + new Vector2(2, 3), objectRect.Size),
            new Color(0.01f, 0.012f, 0.01f, 0.35f),
            true
        );
        DrawRect(objectRect, color, true);
        DrawRect(objectRect, color.Lightened(0.25f), false, 1.5f);
    }

    private void DrawTileWallObject(TileWallRenderData wall, WorldObjectDefinition? definition)
    {
        var color = definition is null
            ? new Color(0.48f, 0.5f, 0.47f)
            : ParseHtmlColor(definition.MapColor, new Color(0.48f, 0.5f, 0.47f));

        if (wall.Kind == TileWallKind.GlassDoor)
        {
            DrawTileGlassDoor(wall, color);
            return;
        }

        DrawTileWallFaces(wall.Geometry, wall.Neighbors, color);

        if (wall.Kind == TileWallKind.Window)
        {
            DrawTileWindow(wall.Geometry, wall.Orientation);
        }
        else if (wall.Kind == TileWallKind.WoodenDoor)
        {
            DrawTileWoodenDoor(wall.Geometry, wall.Orientation);
        }
    }

    private void DrawTileWallFaces(TileWallGeometry geometry, TileWallNeighbors neighbors, Color color)
    {
        var topColor = color.Lightened(0.18f);
        var frontColor = color.Darkened(0.05f);
        var westColor = color.Lightened(0.03f);
        var eastColor = color.Darkened(0.18f);

        if (!neighbors.HasFlag(TileWallNeighbors.West))
        {
            DrawColoredPolygon(new[]
            {
                geometry.Top.Position,
                new Vector2(geometry.Top.Position.X, geometry.Top.End.Y),
                new Vector2(geometry.Footprint.Position.X, geometry.Footprint.End.Y),
                geometry.Footprint.Position
            }, westColor);
        }

        if (!neighbors.HasFlag(TileWallNeighbors.East))
        {
            DrawColoredPolygon(new[]
            {
                new Vector2(geometry.Top.End.X, geometry.Top.Position.Y),
                geometry.Top.End,
                geometry.Footprint.End,
                new Vector2(geometry.Footprint.End.X, geometry.Footprint.Position.Y)
            }, eastColor);
        }

        DrawRect(geometry.Top, topColor, true);
        DrawRect(geometry.FrontFace, frontColor, true);

        if (!neighbors.HasFlag(TileWallNeighbors.South))
        {
            var foundationHeight = Mathf.Max(2.0f, Mathf.Round(_cellSize * 0.07f));
            DrawRect(
                new Rect2(
                    new Vector2(geometry.Footprint.Position.X, geometry.Footprint.End.Y - foundationHeight),
                    new Vector2(geometry.Footprint.Size.X, foundationHeight)
                ),
                color.Darkened(0.28f),
                true
            );
        }
    }

    private void DrawTileWindow(TileWallGeometry geometry, TileWallOrientation orientation)
    {
        var face = geometry.FrontFace;
        var insetX = orientation == TileWallOrientation.Horizontal ? face.Size.X * 0.2f : face.Size.X * 0.32f;
        var insetY = Mathf.Max(2.0f, face.Size.Y * 0.22f);
        var glassRect = new Rect2(
            face.Position + new Vector2(insetX, insetY),
            face.Size - new Vector2(insetX * 2.0f, insetY * 2.0f)
        );

        DrawRect(glassRect, TileGlassColor, true);
        DrawRect(glassRect, new Color(0.09f, 0.13f, 0.14f), false, Mathf.Max(1.0f, _cellSize * 0.04f));
        DrawLine(
            glassRect.Position + new Vector2(glassRect.Size.X * 0.18f, glassRect.Size.Y * 0.24f),
            glassRect.Position + new Vector2(glassRect.Size.X * 0.62f, glassRect.Size.Y * 0.24f),
            new Color(0.82f, 0.94f, 0.95f, 0.78f),
            1.0f
        );
    }

    private void DrawTileWoodenDoor(TileWallGeometry geometry, TileWallOrientation orientation)
    {
        var face = geometry.FrontFace;
        var insetX = orientation == TileWallOrientation.Horizontal ? face.Size.X * 0.13f : face.Size.X * 0.26f;
        var insetY = Mathf.Max(1.5f, face.Size.Y * 0.08f);
        var doorRect = new Rect2(
            face.Position + new Vector2(insetX, insetY),
            face.Size - new Vector2(insetX * 2.0f, insetY * 2.0f)
        );
        var wood = new Color(0.52f, 0.33f, 0.18f);
        var panelInset = Mathf.Max(2.0f, _cellSize * 0.08f);

        DrawRect(doorRect, wood, true);
        DrawRect(doorRect, new Color(0.22f, 0.13f, 0.07f), false, Mathf.Max(1.0f, _cellSize * 0.04f));
        DrawRect(doorRect.Grow(-panelInset), wood.Lightened(0.14f), false, 1.0f);

        var knob = orientation == TileWallOrientation.Horizontal
            ? doorRect.Position + new Vector2(doorRect.Size.X * 0.78f, doorRect.Size.Y * 0.56f)
            : doorRect.Position + new Vector2(doorRect.Size.X * 0.62f, doorRect.Size.Y * 0.78f);
        DrawCircle(knob, Mathf.Max(1.5f, _cellSize * 0.045f), new Color(0.84f, 0.67f, 0.35f));
    }

    private void DrawTileGlassDoor(TileWallRenderData wall, Color color)
    {
        var body = TileWallRenderModel.GetConnectedBody(wall.FootprintRect, TileWallNeighbors.None, _cellSize);
        var postWidth = Mathf.Max(3.0f, _cellSize * 0.1f);
        var thresholdWidth = Mathf.Max(2.0f, _cellSize * 0.08f);
        var frameColor = color.Lightened(0.16f);
        var darkFrame = color.Darkened(0.2f);
        var glassCue = new Color(TileGlassColor.R, TileGlassColor.G, TileGlassColor.B, 0.54f);

        if (wall.Orientation == TileWallOrientation.Horizontal)
        {
            var leftPost = new Rect2(body.Position, new Vector2(postWidth, body.Size.Y));
            var rightPost = new Rect2(
                new Vector2(body.End.X - postWidth, body.Position.Y),
                new Vector2(postWidth, body.Size.Y)
            );
            DrawTileDoorJamb(leftPost, frameColor);
            DrawTileDoorJamb(rightPost, darkFrame);

            var thresholdY = body.End.Y - thresholdWidth;
            DrawRect(
                new Rect2(
                    new Vector2(leftPost.End.X, thresholdY),
                    new Vector2(rightPost.Position.X - leftPost.End.X, thresholdWidth)
                ),
                darkFrame,
                true
            );

            var pane = new Rect2(
                body.Position + new Vector2(body.Size.X * 0.46f, -TileWallRenderModel.GetHeight(_cellSize) * 0.7f),
                new Vector2(body.Size.X * 0.12f, TileWallRenderModel.GetHeight(_cellSize) * 0.72f)
            );
            DrawRect(pane, glassCue, true);
        }
        else
        {
            var topPost = new Rect2(body.Position, new Vector2(body.Size.X, postWidth));
            var bottomPost = new Rect2(
                new Vector2(body.Position.X, body.End.Y - postWidth),
                new Vector2(body.Size.X, postWidth)
            );
            DrawTileDoorJamb(topPost, frameColor);
            DrawTileDoorJamb(bottomPost, darkFrame);

            var thresholdX = body.Position.X + (body.Size.X / 2.0f) - (thresholdWidth / 2.0f);
            DrawRect(
                new Rect2(
                    new Vector2(thresholdX, topPost.End.Y),
                    new Vector2(thresholdWidth, bottomPost.Position.Y - topPost.End.Y)
                ),
                darkFrame,
                true
            );

            var pane = new Rect2(
                body.Position + new Vector2(body.Size.X * 0.22f, body.Size.Y * 0.46f),
                new Vector2(body.Size.X * 0.56f, body.Size.Y * 0.12f)
            );
            DrawRect(pane, glassCue, true);
        }
    }

    private void DrawTileDoorJamb(Rect2 baseRect, Color color)
    {
        var geometry = TileWallRenderModel.GetGeometry(baseRect, TileWallNeighbors.None, _cellSize);
        DrawTileWallFaces(geometry, TileWallNeighbors.None, color);
    }

    private void DrawStructure(StructureRenderInfo renderInfo)
    {
        if (TryGetStructureSprite(renderInfo, out var sprite))
        {
            DrawTextureRect(sprite, GetStructureRenderBounds(renderInfo), false);
            return;
        }

        DrawStructureFallback(renderInfo);
    }

    private void DrawStructureFallback(StructureRenderInfo renderInfo)
    {
        var color = ParseHtmlColor(renderInfo.Definition.MapColor, new Color(0.45f, 0.47f, 0.42f));
        if (renderInfo.Orientation == StructureVisualOrientation.Front)
        {
            DrawFrontStructureFallback(renderInfo, color);
        }
        else
        {
            DrawSideStructureFallback(renderInfo, color);
        }
    }

    private void DrawFrontStructureFallback(StructureRenderInfo renderInfo, Color color)
    {
        var key = renderInfo.Edge.Key;
        var origin = _viewport?.Origin ?? new GridPosition(0, 0);
        var x = ((key.X - origin.X) * _cellSize) - 0.5f;
        var baseY = (key.Y - origin.Y) * _cellSize;
        var width = _cellSize + 1.0f;
        var faceHeight = _cellSize * 0.62f;
        var capHeight = _cellSize * 0.16f;
        var faceRect = new Rect2(x, baseY - faceHeight, width, faceHeight);
        var capRect = new Rect2(x, baseY - faceHeight - capHeight, width, capHeight);

        DrawRect(new Rect2(x + 2, baseY - 2, width, 5), new Color(0.01f, 0.012f, 0.01f, 0.32f), true);

        if (!renderInfo.Definition.BlocksMovement)
        {
            var postWidth = Mathf.Max(3.0f, _cellSize * 0.12f);
            if (!OpeningConnectsBefore(renderInfo))
            {
                DrawRect(new Rect2(x, faceRect.Position.Y, postWidth, faceRect.Size.Y), color.Darkened(0.08f), true);
            }

            if (!OpeningConnectsAfter(renderInfo))
            {
                DrawRect(new Rect2(x + width - postWidth, faceRect.Position.Y, postWidth, faceRect.Size.Y), color.Darkened(0.08f), true);
            }

            DrawLine(new Vector2(x, baseY), new Vector2(x + width, baseY), color.Lightened(0.2f), 2.0f);
            return;
        }

        DrawRect(faceRect, color, true);
        DrawRect(capRect, color.Lightened(0.16f), true);
        DrawLine(faceRect.Position, new Vector2(faceRect.End.X, faceRect.Position.Y), color.Lightened(0.2f), 1.0f);
        DrawLine(new Vector2(faceRect.Position.X, faceRect.End.Y), faceRect.End, color.Darkened(0.28f), 1.0f);
        DrawLine(capRect.Position, new Vector2(capRect.End.X, capRect.Position.Y), color.Lightened(0.28f), 1.0f);

        if (renderInfo.Variant is "start" or "single")
        {
            DrawLine(faceRect.Position, new Vector2(faceRect.Position.X, faceRect.End.Y), color.Darkened(0.18f), 1.0f);
            DrawLine(capRect.Position, new Vector2(capRect.Position.X, capRect.End.Y), color.Darkened(0.12f), 1.0f);
        }

        if (renderInfo.Variant is "end" or "single")
        {
            DrawLine(new Vector2(faceRect.End.X, faceRect.Position.Y), faceRect.End, color.Darkened(0.3f), 1.0f);
            DrawLine(new Vector2(capRect.End.X, capRect.Position.Y), capRect.End, color.Darkened(0.2f), 1.0f);
        }

        if (renderInfo.Definition.PieceKind.Contains("window", StringComparison.OrdinalIgnoreCase))
        {
            DrawStructureWindowInset(faceRect, renderInfo.Definition.PieceKind);
        }
    }

    private void DrawSideStructureFallback(StructureRenderInfo renderInfo, Color color)
    {
        var key = renderInfo.Edge.Key;
        var origin = _viewport?.Origin ?? new GridPosition(0, 0);
        var baseX = (key.X - origin.X) * _cellSize;
        var topY = ((key.Y - origin.Y) * _cellSize) - 0.5f;
        var bottomY = topY + _cellSize + 1.0f;
        var halfWidth = Mathf.Max(4.0f, _cellSize * 0.13f);
        var skew = Mathf.Max(2.0f, _cellSize * 0.09f);

        var leftTop = new Vector2(baseX - halfWidth, topY + skew);
        var rightTop = new Vector2(baseX + halfWidth, topY - skew);
        var rightBottom = new Vector2(baseX + halfWidth, bottomY - skew);
        var leftBottom = new Vector2(baseX - halfWidth, bottomY + skew);

        var sideFace = new[] { leftTop, rightTop, rightBottom, leftBottom };
        var shadowOffset = new Vector2(2.0f, 3.0f);

        if (!renderInfo.Definition.BlocksMovement)
        {
            var jambLength = Mathf.Max(5.0f, _cellSize * 0.16f);
            if (!OpeningConnectsBefore(renderInfo))
            {
                DrawLine(new Vector2(baseX, topY), new Vector2(baseX, topY + jambLength), color.Lightened(0.18f), 2.0f);
            }

            if (!OpeningConnectsAfter(renderInfo))
            {
                DrawLine(
                    new Vector2(baseX, bottomY - jambLength),
                    new Vector2(baseX, bottomY),
                    color.Darkened(0.12f),
                    2.0f
                );
            }

            return;
        }

        DrawColoredPolygon(new[]
        {
            leftTop + shadowOffset,
            rightTop + shadowOffset,
            rightBottom + shadowOffset,
            leftBottom + shadowOffset
        }, new Color(0.01f, 0.012f, 0.01f, 0.28f));

        DrawColoredPolygon(sideFace, color.Darkened(0.04f));
        DrawLine(leftTop, leftBottom, color.Lightened(0.08f), 1.0f);
        DrawLine(rightTop, rightBottom, color.Darkened(0.3f), 1.0f);

        if (renderInfo.Variant is "start" or "single")
        {
            DrawLine(leftTop, rightTop, color.Lightened(0.24f), 2.0f);
        }

        if (renderInfo.Variant is "end" or "single")
        {
            DrawLine(leftBottom, rightBottom, color.Darkened(0.24f), 2.0f);
        }

        if (renderInfo.Definition.PieceKind.Contains("window", StringComparison.OrdinalIgnoreCase))
        {
            var windowTop = topY + (_cellSize * 0.28f);
            DrawLine(
                new Vector2(baseX, windowTop),
                new Vector2(baseX, windowTop + (_cellSize * 0.42f)),
                new Color(0.46f, 0.66f, 0.72f),
                Mathf.Max(2.0f, _cellSize * 0.08f)
            );
        }
    }

    private bool OpeningConnectsBefore(StructureRenderInfo renderInfo)
    {
        return OpeningConnects(renderInfo.Edge.Key.NeighborBefore(), renderInfo.Edge.StructureId);
    }

    private bool OpeningConnectsAfter(StructureRenderInfo renderInfo)
    {
        return OpeningConnects(renderInfo.Edge.Key.NeighborAfter(), renderInfo.Edge.StructureId);
    }

    private bool OpeningConnects(StructureEdgeKey key, StructureId structureId)
    {
        return _structureMap is not null
            && _structureMap.TryGetEdge(key, out var neighbor)
            && neighbor.StructureId == structureId;
    }

    private void DrawStructureWindowInset(Rect2 faceRect, string pieceKind)
    {
        var inset = new Rect2(
            faceRect.Position + new Vector2(faceRect.Size.X * 0.22f, faceRect.Size.Y * 0.22f),
            new Vector2(faceRect.Size.X * 0.56f, faceRect.Size.Y * 0.42f)
        );
        var glassColor = pieceKind.Contains("boarded", StringComparison.OrdinalIgnoreCase)
            ? new Color(0.42f, 0.29f, 0.18f)
            : new Color(0.46f, 0.66f, 0.72f, 0.84f);

        DrawRect(inset, glassColor, true);
        DrawRect(inset, new Color(0.12f, 0.16f, 0.16f), false, 1.0f);

        if (pieceKind.Contains("broken", StringComparison.OrdinalIgnoreCase))
        {
            DrawLine(inset.Position, inset.End, new Color(0.85f, 0.9f, 0.88f), 1.0f);
            DrawLine(
                new Vector2(inset.End.X, inset.Position.Y),
                new Vector2(inset.Position.X, inset.End.Y),
                new Color(0.85f, 0.9f, 0.88f),
                1.0f
            );
        }
    }

    private void DrawNpc(NpcState npc, NpcDefinition? definition, SpriteRenderProfile render, Texture2D? sprite)
    {
        var center = GetRenderCenter(npc.Position, render);
        if (sprite is not null)
        {
            var shadowRadius = Mathf.Max(6.0f, _cellSize * 0.28f);
            DrawCircle(center + new Vector2(2, 4), shadowRadius, new Color(0.01f, 0.012f, 0.01f, 0.35f));

            var rect = DrawSprite(npc.Position, render, sprite);
            if (npc.IsDisabled)
            {
                DrawRect(rect, new Color(0.08f, 0.08f, 0.08f, 0.38f), true);
            }

            DrawHealthBar(CellToBoardPosition(MapToViewportUnchecked(npc.Position)), npc.Health);
            return;
        }

        DrawNpcFallbackMarker(npc, definition);
    }

    private void DrawNpcFallbackMarker(NpcState npc, NpcDefinition? definition)
    {
        var center = CellToBoardPosition(MapToViewportUnchecked(npc.Position));
        var radius = Mathf.Max(5.0f, _cellSize * 0.26f);
        var baseColor = definition is null
            ? new Color(0.78f, 0.34f, 0.22f)
            : ParseHtmlColor(definition.MapColor, new Color(0.78f, 0.34f, 0.22f));
        var outerColor = npc.IsDisabled
            ? new Color(0.28f, 0.28f, 0.25f)
            : baseColor;
        var innerColor = npc.IsDisabled
            ? new Color(0.46f, 0.45f, 0.39f)
            : baseColor.Lightened(0.32f);

        DrawCircle(center + new Vector2(2, 3), radius, new Color(0.01f, 0.012f, 0.01f, 0.45f));
        DrawCircle(center, radius, outerColor);
        DrawCircle(center, radius * 0.58f, innerColor);
        DrawLine(center + new Vector2(-radius * 0.55f, 0), center + new Vector2(radius * 0.55f, 0), new Color(0.22f, 0.09f, 0.06f), 1.5f);
        DrawLine(center + new Vector2(0, -radius * 0.55f), center + new Vector2(0, radius * 0.55f), new Color(0.22f, 0.09f, 0.06f), 1.5f);

        DrawHealthBar(center, npc.Health);
    }

    private Rect2 DrawSprite(GridPosition mapPosition, SpriteRenderProfile render, Texture2D sprite)
    {
        var rect = GetRenderRect(mapPosition, render, sprite);
        DrawTextureRect(sprite, rect, false);
        return rect;
    }

    private Rect2 DrawWorldObjectSprite(PlacedWorldObject placedObject, SpriteRenderProfile render, Texture2D sprite)
    {
        var center = GetWorldObjectRenderCenter(placedObject, render);
        var size = GetSpriteRenderSize(render, sprite);
        var rect = new Rect2(-(size / 2.0f), size);

        DrawSetTransform(center, GetFacingRotation(placedObject.Facing), Vector2.One);
        DrawTextureRect(sprite, rect, false);
        DrawSetTransform(Vector2.Zero, 0.0f, Vector2.One);

        return GetWorldObjectRenderBounds(placedObject, render, sprite);
    }

    private Rect2 GetStructureRenderBounds(StructureRenderInfo renderInfo)
    {
        var key = renderInfo.Edge.Key;
        var origin = _viewport?.Origin ?? new GridPosition(0, 0);
        var x = (key.X - origin.X) * _cellSize;
        var y = (key.Y - origin.Y) * _cellSize;

        if (renderInfo.Orientation == StructureVisualOrientation.Front)
        {
            return new Rect2(
                x,
                y - (_cellSize * 0.84f),
                _cellSize,
                _cellSize
            );
        }

        return new Rect2(
            x - (_cellSize * 0.22f),
            y - (_cellSize * 0.14f),
            _cellSize * 0.44f,
            _cellSize * 1.28f
        );
    }

    private static float GetStructureSortKey(StructureRenderInfo renderInfo)
    {
        return renderInfo.Edge.Key.Axis == StructureEdgeAxis.Horizontal
            ? renderInfo.Edge.Key.Y - 0.05f
            : renderInfo.Edge.Key.Y + 0.55f;
    }

    private Rect2 GetRenderRect(GridPosition mapPosition, SpriteRenderProfile render, Texture2D? sprite)
    {
        var center = GetRenderCenter(mapPosition, render);
        var maxSize = new Vector2(render.WidthTiles * _cellSize, render.HeightTiles * _cellSize);
        var size = sprite is null
            ? maxSize
            : GetAspectFitSize(sprite.GetSize(), maxSize);

        return new Rect2(center - (size / 2.0f), size);
    }

    private static SpriteRenderProfile GetObjectRenderProfile(
        PlacedWorldObject placedObject,
        WorldObjectDefinition? definition)
    {
        if (definition?.SpriteRender is { } render)
        {
            return render;
        }

        return new SpriteRenderProfile(placedObject.Footprint.Width, placedObject.Footprint.Height);
    }

    private Rect2 GetWorldObjectFootprintRect(PlacedWorldObject placedObject)
    {
        var viewportPosition = MapToViewportUnchecked(placedObject.Position);
        var footprint = placedObject.EffectiveFootprint;
        return new Rect2(
            viewportPosition.X * _cellSize,
            viewportPosition.Y * _cellSize,
            footprint.Width * _cellSize,
            footprint.Height * _cellSize
        );
    }

    private Rect2 GetWorldObjectRenderBounds(
        PlacedWorldObject placedObject,
        SpriteRenderProfile render,
        Texture2D sprite)
    {
        var center = GetWorldObjectRenderCenter(placedObject, render);
        var size = GetSpriteRenderSize(render, sprite);
        var screenSize = IsSideways(placedObject.Facing)
            ? new Vector2(size.Y, size.X)
            : size;

        return new Rect2(center - (screenSize / 2.0f), screenSize);
    }

    private Vector2 GetWorldObjectRenderCenter(PlacedWorldObject placedObject, SpriteRenderProfile render)
    {
        var rect = GetWorldObjectFootprintRect(placedObject);
        var center = rect.Position + (rect.Size / 2.0f);
        var offset = RotateOffset(new Vector2(render.OffsetXTiles, render.OffsetYTiles), placedObject.Facing);

        return center + (offset * _cellSize);
    }

    private Vector2 GetSpriteRenderSize(SpriteRenderProfile render, Texture2D sprite)
    {
        var maxSize = new Vector2(render.WidthTiles * _cellSize, render.HeightTiles * _cellSize);
        return GetAspectFitSize(sprite.GetSize(), maxSize);
    }

    private Vector2 GetRenderCenter(GridPosition mapPosition, SpriteRenderProfile render)
    {
        var viewportPosition = MapToViewportUnchecked(mapPosition);
        return new Vector2(
            (viewportPosition.X + 0.5f + render.OffsetXTiles) * _cellSize,
            (viewportPosition.Y + 0.5f + render.OffsetYTiles) * _cellSize
        );
    }

    private static Vector2 GetAspectFitSize(Vector2 textureSize, Vector2 maxSize)
    {
        var scale = Mathf.Min(maxSize.X / textureSize.X, maxSize.Y / textureSize.Y);
        return textureSize * scale;
    }

    private static bool IsSideways(WorldObjectFacing facing)
    {
        return facing is WorldObjectFacing.East or WorldObjectFacing.West;
    }

    private static float GetFacingRotation(WorldObjectFacing facing)
    {
        return facing switch
        {
            WorldObjectFacing.North => 0.0f,
            WorldObjectFacing.East => Mathf.Pi / 2.0f,
            WorldObjectFacing.South => Mathf.Pi,
            WorldObjectFacing.West => -Mathf.Pi / 2.0f,
            _ => 0.0f
        };
    }

    private static Vector2 RotateOffset(Vector2 offset, WorldObjectFacing facing)
    {
        return facing switch
        {
            WorldObjectFacing.North => offset,
            WorldObjectFacing.East => new Vector2(-offset.Y, offset.X),
            WorldObjectFacing.South => -offset,
            WorldObjectFacing.West => new Vector2(offset.Y, -offset.X),
            _ => offset
        };
    }

    private bool IntersectsViewport(Rect2 rect)
    {
        if (_viewport is null)
        {
            return true;
        }

        var viewportRect = new Rect2(
            Vector2.Zero,
            new Vector2(_viewport.Value.Width * _cellSize, _viewport.Value.Height * _cellSize)
        );
        return rect.Intersects(viewportRect, includeBorders: true);
    }

    private bool TryMapToViewport(GridPosition mapPosition, out GridPosition viewportPosition)
    {
        if (_viewport is not null)
        {
            return _viewport.Value.TryMapToViewport(mapPosition, out viewportPosition);
        }

        viewportPosition = mapPosition;
        return true;
    }

    private GridPosition MapToViewportUnchecked(GridPosition mapPosition)
    {
        return _viewport is null
            ? mapPosition
            : new GridPosition(mapPosition.X - _viewport.Value.Origin.X, mapPosition.Y - _viewport.Value.Origin.Y);
    }

    private Vector2 CellToBoardPosition(GridPosition cell)
    {
        return new Vector2(
            (cell.X + 0.5f) * _cellSize,
            (cell.Y + 0.5f) * _cellSize
        );
    }

    private void DrawHealthBar(Vector2 center, BoundedMeter health)
    {
        var width = Mathf.Max(12.0f, _cellSize * 0.62f);
        var height = Mathf.Max(2.0f, _cellSize * 0.09f);
        var topLeft = center + new Vector2(-width / 2.0f, _cellSize * 0.30f);
        var backgroundRect = new Rect2(topLeft, new Vector2(width, height));
        var fillRect = new Rect2(topLeft, new Vector2(width * health.Normalized, height));

        DrawRect(backgroundRect, new Color(0.08f, 0.035f, 0.03f), true);
        DrawRect(fillRect, new Color(0.76f, 0.12f, 0.1f), true);
    }

    private void DrawPlayerMarker(GridPosition mapPosition)
    {
        var center = CellToBoardPosition(MapToViewportUnchecked(mapPosition));
        DrawPlayerPolygon(center, new[]
        {
            new Vector2(-10, 10),
            new Vector2(-7, 8),
            new Vector2(7, 8),
            new Vector2(10, 10),
            new Vector2(10, 14),
            new Vector2(7, 16),
            new Vector2(-7, 16),
            new Vector2(-10, 14)
        }, new Color(0.015f, 0.019f, 0.018f, 0.42f));
        DrawPlayerPolygon(center, new[]
        {
            new Vector2(-7, 2),
            new Vector2(-2, 2),
            new Vector2(-2, 12),
            new Vector2(-7, 12)
        }, new Color(0.115f, 0.17f, 0.31f));
        DrawPlayerPolygon(center, new[]
        {
            new Vector2(2, 2),
            new Vector2(7, 2),
            new Vector2(7, 12),
            new Vector2(2, 12)
        }, new Color(0.115f, 0.17f, 0.31f));
        DrawPlayerPolygon(center, new[]
        {
            new Vector2(-9, 10),
            new Vector2(-2, 10),
            new Vector2(-2, 14),
            new Vector2(-10, 14)
        }, new Color(0.075f, 0.065f, 0.055f));
        DrawPlayerPolygon(center, new[]
        {
            new Vector2(2, 10),
            new Vector2(9, 10),
            new Vector2(10, 14),
            new Vector2(2, 14)
        }, new Color(0.075f, 0.065f, 0.055f));
        DrawPlayerPolygon(center, new[]
        {
            new Vector2(-8, -7),
            new Vector2(8, -7),
            new Vector2(10, 4),
            new Vector2(6, 9),
            new Vector2(-6, 9),
            new Vector2(-10, 4)
        }, new Color(0.36f, 0.52f, 0.34f));
        DrawPlayerPolygon(center, new[]
        {
            new Vector2(-10, -5),
            new Vector2(-7, -5),
            new Vector2(-8, 5),
            new Vector2(-12, 8),
            new Vector2(-14, 5),
            new Vector2(-11, 2)
        }, new Color(0.66f, 0.47f, 0.34f));
        DrawPlayerPolygon(center, new[]
        {
            new Vector2(7, -5),
            new Vector2(10, -5),
            new Vector2(11, 2),
            new Vector2(14, 5),
            new Vector2(12, 8),
            new Vector2(8, 5)
        }, new Color(0.66f, 0.47f, 0.34f));
        DrawPlayerPolygon(center, new[]
        {
            new Vector2(0, -19),
            new Vector2(5, -17),
            new Vector2(7, -12),
            new Vector2(5, -7),
            new Vector2(0, -5),
            new Vector2(-5, -7),
            new Vector2(-7, -12),
            new Vector2(-5, -17)
        }, new Color(0.73f, 0.54f, 0.4f));
        DrawPlayerPolygon(center, new[]
        {
            new Vector2(-6, -14),
            new Vector2(-5, -18),
            new Vector2(0, -20),
            new Vector2(5, -18),
            new Vector2(7, -13),
            new Vector2(4, -15),
            new Vector2(1, -14),
            new Vector2(-2, -15)
        }, new Color(0.11f, 0.075f, 0.045f));
    }

    private void DrawPlayerPolygon(Vector2 center, Vector2[] points, Color color)
    {
        var scale = _cellSize / 32.0f;
        var scaledPoints = new Vector2[points.Length];
        for (var index = 0; index < points.Length; index++)
        {
            scaledPoints[index] = center + (points[index] * scale);
        }

        DrawColoredPolygon(scaledPoints, color);
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

    private sealed record EntityDrawCommand(float SortKey, int Priority, Action Draw);
}
