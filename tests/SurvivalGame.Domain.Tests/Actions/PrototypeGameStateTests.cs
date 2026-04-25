using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class PrototypeGameStateTests
{
    [Fact]
    public void RootStateOwnsTimePlayerAndWorldState()
    {
        var bounds = new GridBounds(5, 5);
        var groundItems = new TileItemMap();
        var worldObjects = new TileObjectMap();
        var surfaces = new TileSurfaceMap(bounds, PrototypeSurfaces.Concrete);
        var npcs = new NpcRoster();
        npcs.Add(new NpcState(PrototypeNpcs.TestDummy, "Test Dummy", new GridPosition(4, 3), 200, 200));
        var state = new PrototypeGameState(bounds, groundItems, surfaces, worldObjects, npcs, new GridPosition(2, 3));

        Assert.Same(groundItems, state.World.GroundItems);
        Assert.Same(worldObjects, state.World.WorldObjects);
        Assert.Same(surfaces, state.World.Map.Surfaces);
        Assert.Same(npcs, state.World.Npcs);
        Assert.Equal(bounds, state.World.Map.Bounds);
        Assert.Equal(new GridPosition(2, 3), state.Player.Position);
        Assert.Equal(0, state.Time.ElapsedTicks);
    }

    [Fact]
    public void SetPlayerPositionValidatesAgainstWorldMap()
    {
        var state = new PrototypeGameState(
            new GridBounds(5, 5),
            new TileItemMap(),
            new GridPosition(2, 2)
        );

        Assert.Throws<ArgumentOutOfRangeException>(() => state.SetPlayerPosition(new GridPosition(5, 0)));
    }

    [Fact]
    public void WorldStateRejectsNpcPositionsOutsideMap()
    {
        var bounds = new GridBounds(5, 5);
        var npcs = new NpcRoster();
        npcs.Add(new NpcState(PrototypeNpcs.TestDummy, "Test Dummy", new GridPosition(5, 0), 200, 200));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PrototypeGameState(
                bounds,
                new TileItemMap(),
                new TileSurfaceMap(bounds, PrototypeSurfaces.Concrete),
                new TileObjectMap(),
                npcs,
                new GridPosition(2, 2)
            ));
    }

    [Fact]
    public void WorldTimeAdvancesElapsedTicks()
    {
        var time = new WorldTime();

        time.Advance(100);

        Assert.Equal(100, time.ElapsedTicks);
    }

    [Fact]
    public void WorldTimeRejectsNonPositiveAdvancement()
    {
        var time = new WorldTime();

        Assert.Throws<ArgumentOutOfRangeException>(() => time.Advance(0));
    }
}
