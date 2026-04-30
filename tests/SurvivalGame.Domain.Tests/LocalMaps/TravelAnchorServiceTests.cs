using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class TravelAnchorServiceTests
{
    [Fact]
    public void EntryPositionKeepsCurrentPlayerPositionWhenAlreadyStandableAndNearAnchor()
    {
        var anchorId = new WorldObjectInstanceId("vehicle_anchor");
        var worldObjects = new TileObjectMap();
        worldObjects.Place(
            new GridPosition(2, 2),
            PrototypeWorldObjects.PlayerVehicle,
            WorldObjectFacing.North,
            new WorldObjectFootprint(2, 2),
            new GridBounds(6, 6),
            anchorId
        );
        var state = CreateState(worldObjects, new GridPosition(1, 2), new GridBounds(6, 6));

        var found = TravelAnchorService.TryFindEntryPosition(
            state,
            anchorId,
            CreateWorldObjectCatalog(),
            out var position);

        Assert.True(found);
        Assert.Equal(new GridPosition(1, 2), position);
    }

    [Fact]
    public void EntryPositionSkipsBlockedObjectAndNpcCandidates()
    {
        var anchorId = new WorldObjectInstanceId("vehicle_anchor");
        var bounds = new GridBounds(5, 5);
        var worldObjects = new TileObjectMap();
        worldObjects.Place(
            new GridPosition(1, 1),
            PrototypeWorldObjects.PlayerVehicle,
            WorldObjectFacing.North,
            new WorldObjectFootprint(2, 2),
            bounds,
            anchorId
        );
        worldObjects.Place(
            new GridPosition(3, 2),
            PrototypeWorldObjects.Wall,
            WorldObjectFacing.North,
            WorldObjectFootprint.SingleTile,
            bounds,
            new WorldObjectInstanceId("blocked_candidate")
        );

        var npcs = new NpcRoster();
        npcs.Add(new NpcState(new NpcId("guard"), "Guard", new GridPosition(2, 3)));
        var state = CreateState(worldObjects, new GridPosition(4, 4), bounds, npcs);

        var found = TravelAnchorService.TryFindEntryPosition(
            state,
            anchorId,
            CreateWorldObjectCatalog(),
            out var position);

        Assert.True(found);
        Assert.Equal(new GridPosition(3, 1), position);
    }

    [Fact]
    public void EntryPositionFailsWhenNoStandableCandidateExists()
    {
        var anchorId = new WorldObjectInstanceId("vehicle_anchor");
        var bounds = new GridBounds(3, 3);
        var worldObjects = new TileObjectMap();
        worldObjects.Place(
            new GridPosition(1, 1),
            PrototypeWorldObjects.PlayerVehicle,
            WorldObjectFacing.North,
            WorldObjectFootprint.SingleTile,
            bounds,
            anchorId
        );

        foreach (var blockerPosition in new[]
        {
            new GridPosition(1, 0),
            new GridPosition(1, 2),
            new GridPosition(0, 1),
            new GridPosition(2, 1)
        })
        {
            worldObjects.Place(
                blockerPosition,
                PrototypeWorldObjects.Wall,
                WorldObjectFacing.North,
                WorldObjectFootprint.SingleTile,
                bounds,
                new WorldObjectInstanceId($"blocker_{blockerPosition.X}_{blockerPosition.Y}")
            );
        }

        var state = CreateState(worldObjects, new GridPosition(0, 0), bounds);

        var found = TravelAnchorService.TryFindEntryPosition(
            state,
            anchorId,
            CreateWorldObjectCatalog(),
            out var position);

        Assert.False(found);
        Assert.Equal(default, position);
    }

    private static PrototypeGameState CreateState(
        TileObjectMap worldObjects,
        GridPosition playerPosition,
        GridBounds bounds,
        NpcRoster? npcs = null)
    {
        return new PrototypeGameState(
            new LocalMapState(
                new LocalMap(bounds, new TileSurfaceMap(bounds, PrototypeSurfaces.Grass)),
                new TileItemMap(),
                worldObjects,
                npcs
            ),
            playerPosition
        );
    }

    private static WorldObjectCatalog CreateWorldObjectCatalog()
    {
        var catalog = new WorldObjectCatalog();
        catalog.Add(new WorldObjectDefinition(
            PrototypeWorldObjects.PlayerVehicle,
            "Your vehicle",
            "",
            "Vehicle",
            blocksMovement: true,
            footprint: new WorldObjectFootprint(2, 2)
        ));
        catalog.Add(new WorldObjectDefinition(PrototypeWorldObjects.Wall, "Wall", "", "Structure", blocksMovement: true));
        return catalog;
    }
}
