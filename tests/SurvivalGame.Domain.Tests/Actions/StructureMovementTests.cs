using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class StructureMovementTests
{
    [Fact]
    public void MovementBlocksWhenCrossingBlockingStructureEdge()
    {
        var structures = new StructureEdgeMap(new GridBounds(3, 3));
        structures.Place(new GridPosition(1, 1), StructureEdgeDirection.East, new StructureId("wall"));
        var state = CreateState(structures, new GridPosition(1, 1));
        var pipeline = new GameActionPipeline(new ItemCatalog(), structureCatalog: CreateStructureCatalog());

        var result = pipeline.Execute(new MoveActionRequest(GridOffset.Right), state);

        Assert.False(result.Succeeded);
        Assert.Equal(new GridPosition(1, 1), state.Player.Position);
        Assert.Contains(result.Messages, message => message.Contains("Wall", StringComparison.Ordinal));
    }

    [Fact]
    public void MovementAllowsCrossingOpenStructureEdge()
    {
        var structures = new StructureEdgeMap(new GridBounds(3, 3));
        structures.Place(new GridPosition(1, 1), StructureEdgeDirection.East, new StructureId("open_doorway"));
        var state = CreateState(structures, new GridPosition(1, 1));
        var pipeline = new GameActionPipeline(new ItemCatalog(), structureCatalog: CreateStructureCatalog());

        var result = pipeline.Execute(new MoveActionRequest(GridOffset.Right), state);

        Assert.True(result.Succeeded);
        Assert.Equal(new GridPosition(2, 1), state.Player.Position);
    }

    private static PrototypeGameState CreateState(StructureEdgeMap structures, GridPosition startPosition)
    {
        var bounds = structures.Bounds;
        var surfaces = new TileSurfaceMap(bounds, PrototypeSurfaces.Grass);
        return new PrototypeGameState(
            new LocalMapState(
                new LocalMap(bounds, surfaces),
                new TileItemMap(),
                new TileObjectMap(),
                structures: structures
            ),
            startPosition
        );
    }

    private static StructureCatalog CreateStructureCatalog()
    {
        var catalog = new StructureCatalog();
        catalog.Add(new StructureDefinition(
            new StructureId("wall"),
            "Wall",
            "",
            "Structure",
            "generic",
            "wall",
            blocksMovement: true,
            blocksSight: true));
        catalog.Add(new StructureDefinition(
            new StructureId("open_doorway"),
            "Open doorway",
            "",
            "Doorway",
            "generic",
            "doorway",
            blocksMovement: false,
            connectsAsWall: false));
        return catalog;
    }
}
