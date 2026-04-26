namespace SurvivalGame.Domain;

public sealed class LocalMapBuilder
{
    private readonly TileSurfaceCatalog _surfaceCatalog;
    private readonly WorldObjectCatalog _worldObjectCatalog;
    private readonly StructureCatalog _structureCatalog;
    private readonly ItemCatalog _itemCatalog;
    private readonly NpcCatalog _npcCatalog;
    private readonly TileSurfaceMap _surfaces;
    private readonly TileItemMap _groundItems = new();
    private readonly TileObjectMap _worldObjects = new();
    private readonly StructureEdgeMap _structures;
    private readonly NpcRoster _npcs = new();

    public LocalMapBuilder(
        SiteId id,
        string displayName,
        GridBounds bounds,
        GridPosition startPosition,
        SurfaceId defaultSurfaceId,
        TileSurfaceCatalog surfaceCatalog,
        WorldObjectCatalog worldObjectCatalog,
        StructureCatalog structureCatalog,
        ItemCatalog itemCatalog,
        NpcCatalog npcCatalog)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Local site display name cannot be empty.", nameof(displayName));
        }

        ArgumentNullException.ThrowIfNull(defaultSurfaceId);
        ArgumentNullException.ThrowIfNull(surfaceCatalog);
        ArgumentNullException.ThrowIfNull(worldObjectCatalog);
        ArgumentNullException.ThrowIfNull(structureCatalog);
        ArgumentNullException.ThrowIfNull(itemCatalog);
        ArgumentNullException.ThrowIfNull(npcCatalog);

        if (!bounds.Contains(startPosition))
        {
            throw new ArgumentOutOfRangeException(nameof(startPosition), "Local site start position must be inside map bounds.");
        }

        _surfaceCatalog = surfaceCatalog;
        _worldObjectCatalog = worldObjectCatalog;
        _structureCatalog = structureCatalog;
        _itemCatalog = itemCatalog;
        _npcCatalog = npcCatalog;

        Id = id;
        DisplayName = displayName.Trim();
        Bounds = bounds;
        StartPosition = startPosition;

        EnsureSurfaceDefined(defaultSurfaceId);
        _surfaces = new TileSurfaceMap(bounds, defaultSurfaceId);
        _structures = new StructureEdgeMap(bounds);
    }

    public SiteId Id { get; }

    public string DisplayName { get; }

    public GridBounds Bounds { get; }

    public GridPosition StartPosition { get; }

    public void SetSurface(GridPosition position, SurfaceId surfaceId)
    {
        EnsureInsideBounds(position, "Surface position");
        EnsureSurfaceDefined(surfaceId);

        _surfaces.SetSurface(position, surfaceId);
    }

    public void PlaceWorldObject(
        GridPosition position,
        WorldObjectId objectId,
        WorldObjectFacing facing = WorldObjectFacing.North,
        WorldObjectInstanceId? instanceId = null,
        WorldObjectContainerLootSpec? containerLoot = null)
    {
        EnsureInsideBounds(position, "World object position");
        var definition = GetWorldObjectDefinition(objectId);
        EnsureContainerLootIsValid(definition, containerLoot);

        _worldObjects.Place(
            position,
            objectId,
            facing,
            definition.Footprint,
            Bounds,
            instanceId,
            containerLoot
        );
    }

    public void PlaceStructureEdge(
        GridPosition position,
        StructureEdgeDirection direction,
        StructureId structureId)
    {
        EnsureInsideBounds(position, "Structure edge tile position");
        EnsureStructureDefined(structureId);

        _structures.Place(position, direction, structureId);
    }

    public void PlaceGroundItem(GridPosition position, ItemId itemId, int quantity = 1)
    {
        EnsureInsideBounds(position, "Ground item position");
        EnsureItemDefined(itemId);

        _groundItems.Place(position, itemId, quantity);
    }

    public void PlaceNpc(GridPosition position, NpcId instanceId, NpcDefinitionId definitionId)
    {
        ArgumentNullException.ThrowIfNull(instanceId);
        EnsureInsideBounds(position, "NPC position");

        var definition = _npcCatalog.Get(definitionId);
        _npcs.Add(definition.CreateState(instanceId, position));
    }

    public PrototypeLocalSite Build()
    {
        return new PrototypeLocalSite(
            Id,
            DisplayName,
            Bounds,
            StartPosition,
            _groundItems,
            _surfaces,
            _worldObjects,
            _structures,
            _npcs
        );
    }

    private void EnsureInsideBounds(GridPosition position, string subject)
    {
        if (!Bounds.Contains(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position), $"{subject} must be inside map bounds.");
        }
    }

    private void EnsureSurfaceDefined(SurfaceId surfaceId)
    {
        _surfaceCatalog.Get(surfaceId);
    }

    private WorldObjectDefinition GetWorldObjectDefinition(WorldObjectId objectId)
    {
        return _worldObjectCatalog.Get(objectId);
    }

    private void EnsureStructureDefined(StructureId structureId)
    {
        _structureCatalog.Get(structureId);
    }

    private void EnsureItemDefined(ItemId itemId)
    {
        _itemCatalog.Get(itemId);
    }

    private void EnsureContainerLootIsValid(
        WorldObjectDefinition definition,
        WorldObjectContainerLootSpec? containerLoot)
    {
        if (containerLoot is null)
        {
            return;
        }

        if (!definition.IsContainer)
        {
            throw new InvalidOperationException(
                $"World object '{definition.Id}' cannot define container loot because it is not a container."
            );
        }

        foreach (var stack in containerLoot.FixedStacks)
        {
            EnsureItemDefined(stack.ItemId);
        }
    }
}
