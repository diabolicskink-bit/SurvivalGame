using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class GameActionPipelineTests
{
    [Fact]
    public void WaitIsAlwaysAvailable()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();

        var actions = pipeline.GetAvailableActions(state);

        Assert.Contains(actions, action => action.Kind == GameActionKind.Wait);
    }

    [Fact]
    public void PickupIsAvailableOnlyWhenPlayerIsOnItems()
    {
        var pipeline = CreatePipeline();
        var groundItems = new TileItemMap();
        groundItems.Place(new GridPosition(1, 1), PrototypeItems.Stone, 2);
        var state = CreateState(groundItems, new GridPosition(1, 1));

        var actions = pipeline.GetAvailableActions(state);

        Assert.Contains(actions, action => action.Kind == GameActionKind.Pickup);
    }

    [Fact]
    public void PickupIsNotAvailableWhenPlayerIsNotOnItems()
    {
        var pipeline = CreatePipeline();
        var groundItems = new TileItemMap();
        groundItems.Place(new GridPosition(1, 1), PrototypeItems.Stone, 2);
        var state = CreateState(groundItems, new GridPosition(2, 2));

        var actions = pipeline.GetAvailableActions(state);

        Assert.DoesNotContain(actions, action => action.Kind == GameActionKind.Pickup);
    }

    [Fact]
    public void WaitAdvancesTurn()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();

        var result = pipeline.Execute(state, new WaitActionRequest());

        Assert.True(result.Succeeded);
        Assert.True(result.AdvancedTurn);
        Assert.Equal(1, state.TurnCount);
        Assert.Contains("Waited.", result.Messages);
    }

    [Fact]
    public void MoveAdvancesTurnAndUpdatesPlayerPosition()
    {
        var pipeline = CreatePipeline();
        var state = CreateState(startPosition: new GridPosition(2, 2));

        var result = pipeline.Execute(state, new MoveActionRequest(GridOffset.Right));

        Assert.True(result.Succeeded);
        Assert.Equal(new GridPosition(3, 2), state.PlayerPosition);
        Assert.Equal(1, state.TurnCount);
    }

    [Fact]
    public void InvalidMoveDoesNotAdvanceTurn()
    {
        var pipeline = CreatePipeline();
        var state = CreateState(startPosition: new GridPosition(0, 0));

        var result = pipeline.Execute(state, new MoveActionRequest(GridOffset.Left));

        Assert.False(result.Succeeded);
        Assert.Equal(new GridPosition(0, 0), state.PlayerPosition);
        Assert.Equal(0, state.TurnCount);
    }

    [Fact]
    public void PickupMovesGroundItemsIntoPlayerInventoryAndAdvancesTurn()
    {
        var pipeline = CreatePipeline();
        var groundItems = new TileItemMap();
        var position = new GridPosition(1, 1);
        groundItems.Place(position, PrototypeItems.Stone, 2);
        groundItems.Place(position, PrototypeItems.Branch);
        var state = CreateState(groundItems, position);

        var result = pipeline.Execute(state, new PickupActionRequest());

        Assert.True(result.Succeeded);
        Assert.Equal(1, state.TurnCount);
        Assert.Equal(2, state.Player.Inventory.CountOf(PrototypeItems.Stone));
        Assert.Equal(1, state.Player.Inventory.CountOf(PrototypeItems.Branch));
        Assert.Empty(state.GroundItems.ItemsAt(position));
        Assert.Contains(result.Messages, message => message.Contains("Picked up Stone x2."));
    }

    [Fact]
    public void PickupFailsWithoutAdvancingTurnWhenNoItemsArePresent()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();

        var result = pipeline.Execute(state, new PickupActionRequest());

        Assert.False(result.Succeeded);
        Assert.Equal(0, state.TurnCount);
    }

    private static GameActionPipeline CreatePipeline()
    {
        var catalog = new ItemCatalog();
        catalog.Add(new ItemDefinition(PrototypeItems.Stone, "Stone", "", "Material"));
        catalog.Add(new ItemDefinition(PrototypeItems.Branch, "Branch", "", "Material"));

        return new GameActionPipeline(catalog);
    }

    private static PrototypeGameState CreateState(
        TileItemMap? groundItems = null,
        GridPosition? startPosition = null
    )
    {
        return new PrototypeGameState(
            new GridBounds(5, 5),
            groundItems ?? new TileItemMap(),
            startPosition ?? new GridPosition(2, 2)
        );
    }
}
