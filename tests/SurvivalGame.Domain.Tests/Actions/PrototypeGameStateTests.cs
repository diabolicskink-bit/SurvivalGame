using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class PrototypeGameStateTests
{
    [Fact]
    public void RootStateOwnsTurnPlayerAndWorldState()
    {
        var bounds = new GridBounds(5, 5);
        var groundItems = new TileItemMap();
        var worldObjects = new TileObjectMap();
        var surfaces = new TileSurfaceMap(bounds, PrototypeSurfaces.Concrete);
        var state = new PrototypeGameState(bounds, groundItems, surfaces, worldObjects, new GridPosition(2, 3));

        Assert.Same(groundItems, state.World.GroundItems);
        Assert.Same(worldObjects, state.World.WorldObjects);
        Assert.Same(surfaces, state.World.Map.Surfaces);
        Assert.Equal(bounds, state.World.Map.Bounds);
        Assert.Equal(new GridPosition(2, 3), state.Player.Position);
        Assert.Equal(0, state.Turn.CurrentTurn);
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
    public void TurnStateAdvancesCurrentTurn()
    {
        var turn = new TurnState();

        turn.Advance();

        Assert.Equal(1, turn.CurrentTurn);
    }
}
