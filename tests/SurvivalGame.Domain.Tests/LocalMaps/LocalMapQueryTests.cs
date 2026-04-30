using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class LocalMapQueryTests
{
    [Fact]
    public void MovementReportsBlockersInCurrentRuleOrder()
    {
        var worldObjects = new TileObjectMap();
        worldObjects.Place(new GridPosition(2, 1), new WorldObjectId("crate"));

        var npcs = new NpcRoster();
        npcs.Add(new NpcState(new NpcId("guard"), "Guard", new GridPosition(2, 1)));

        var query = new LocalMapQuery(
            CreateLocalMapState(new GridBounds(4, 4), worldObjects, npcs),
            CreateWorldObjectCatalog()
        );

        var blocked = query.TryGetMovementBlocker(
            new GridPosition(1, 1),
            new GridPosition(2, 1),
            out var blocker);

        Assert.True(blocked);
        Assert.Equal(LocalMapBlockerKind.WorldObject, blocker.Kind);
        Assert.Equal("Crate", blocker.Name);
    }

    [Fact]
    public void MovementAndStandingUseDestinationOccupancyRules()
    {
        var worldObjects = new TileObjectMap();
        worldObjects.Place(new GridPosition(2, 1), new WorldObjectId("chair"));

        var npcs = new NpcRoster();
        npcs.Add(new NpcState(
            new NpcId("observer"),
            new NpcDefinitionId("observer"),
            "Observer",
            new GridPosition(1, 2),
            blocksMovement: false
        ));
        npcs.Add(new NpcState(new NpcId("guard"), "Guard", new GridPosition(2, 2)));

        var query = new LocalMapQuery(
            CreateLocalMapState(new GridBounds(4, 4), worldObjects, npcs: npcs),
            CreateWorldObjectCatalog()
        );

        Assert.False(query.TryGetMovementBlocker(new GridPosition(1, 1), new GridPosition(2, 1), out _));
        Assert.False(query.TryGetStandBlocker(new GridPosition(1, 2), out _));

        Assert.True(query.TryGetStandBlocker(new GridPosition(2, 2), out var npcBlocker));
        Assert.Equal(LocalMapBlockerKind.Npc, npcBlocker.Kind);
        Assert.Equal("Guard", npcBlocker.Name);

        Assert.True(query.TryGetStandBlocker(new GridPosition(-1, 2), out var boundsBlocker));
        Assert.Equal(LocalMapBlockerKind.OutOfBounds, boundsBlocker.Kind);
    }

    [Fact]
    public void NearbyWorldObjectPlacementsAreCardinalAndDeduplicated()
    {
        var worldObjects = new TileObjectMap();
        worldObjects.Place(
            new GridPosition(2, 1),
            new WorldObjectId("vehicle"),
            WorldObjectFacing.North,
            new WorldObjectFootprint(2, 2),
            new GridBounds(5, 5),
            new WorldObjectInstanceId("vehicle_01")
        );
        worldObjects.Place(
            new GridPosition(1, 1),
            new WorldObjectId("diagonal_crate"),
            WorldObjectFacing.North,
            WorldObjectFootprint.SingleTile,
            new GridBounds(5, 5),
            new WorldObjectInstanceId("diagonal_crate_01")
        );

        var query = new LocalMapQuery(CreateLocalMapState(new GridBounds(5, 5), worldObjects));

        var withOrigin = query.GetNearbyWorldObjectPlacements(new GridPosition(2, 2), includeOrigin: true).ToArray();
        var withoutOrigin = query.GetNearbyWorldObjectPlacements(new GridPosition(2, 2), includeOrigin: false).ToArray();

        Assert.Single(withOrigin);
        Assert.Equal(new WorldObjectInstanceId("vehicle_01"), withOrigin[0].InstanceId);
        Assert.Single(withoutOrigin);
        Assert.Equal(new WorldObjectInstanceId("vehicle_01"), withoutOrigin[0].InstanceId);
    }

    [Fact]
    public void PlacementProximityUsesAnyOccupiedTile()
    {
        var placement = new PlacedWorldObject(
            new GridPosition(2, 2),
            new WorldObjectId("vehicle"),
            new WorldObjectInstanceId("vehicle_01"),
            WorldObjectFacing.North,
            new WorldObjectFootprint(2, 2),
            null
        );
        var query = new LocalMapQuery(CreateLocalMapState(new GridBounds(6, 6)));

        Assert.True(query.IsNearPlacement(new GridPosition(4, 3), placement));
        Assert.False(query.IsNearPlacement(new GridPosition(5, 3), placement));
    }

    [Fact]
    public void SightBlockingPreservesObjectEndpointAndCornerTraceRules()
    {
        var worldObjects = new TileObjectMap();
        worldObjects.Place(new GridPosition(2, 2), new WorldObjectId("blocker"));
        var endpointOnly = new LocalMapQuery(
            CreateLocalMapState(new GridBounds(4, 4), worldObjects),
            CreateWorldObjectCatalog()
        );

        Assert.False(endpointOnly.TryFindSightBlocker(
            new GridPosition(0, 0),
            new GridPosition(2, 2),
            out _));

        worldObjects.Place(new GridPosition(1, 0), new WorldObjectId("blocker"));
        var cornerTrace = new LocalMapQuery(
            CreateLocalMapState(new GridBounds(4, 4), worldObjects),
            CreateWorldObjectCatalog()
        );

        Assert.True(cornerTrace.TryFindSightBlocker(
            new GridPosition(0, 0),
            new GridPosition(2, 2),
            out var blocker));
        Assert.Equal(LocalMapBlockerKind.WorldObject, blocker.Kind);
        Assert.Equal("Blocker", blocker.Name);
    }

    private static LocalMapState CreateLocalMapState(
        GridBounds bounds,
        TileObjectMap? worldObjects = null,
        NpcRoster? npcs = null)
    {
        return new LocalMapState(
            new LocalMap(bounds, new TileSurfaceMap(bounds, PrototypeSurfaces.Grass)),
            new TileItemMap(),
            worldObjects ?? new TileObjectMap(),
            npcs
        );
    }

    private static WorldObjectCatalog CreateWorldObjectCatalog()
    {
        var catalog = new WorldObjectCatalog();
        catalog.Add(new WorldObjectDefinition(new WorldObjectId("blocker"), "Blocker", "", "Fixture", blocksMovement: true, blocksSight: true));
        catalog.Add(new WorldObjectDefinition(new WorldObjectId("crate"), "Crate", "", "Fixture", blocksMovement: true));
        catalog.Add(new WorldObjectDefinition(new WorldObjectId("chair"), "Chair", "", "Furniture"));
        catalog.Add(new WorldObjectDefinition(new WorldObjectId("vehicle"), "Vehicle", "", "Vehicle", blocksMovement: true));
        catalog.Add(new WorldObjectDefinition(new WorldObjectId("diagonal_crate"), "Diagonal crate", "", "Fixture", blocksMovement: true));
        return catalog;
    }

}
