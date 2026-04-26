using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class StructureEdgeMapTests
{
    [Fact]
    public void StructureEdgesCanBeResolvedFromEitherSideOfCrossing()
    {
        var structures = new StructureEdgeMap(new GridBounds(4, 4));
        var wall = new StructureId("wall");

        structures.Place(new GridPosition(1, 1), StructureEdgeDirection.North, wall);

        Assert.True(structures.TryGetEdgeBetween(new GridPosition(1, 1), new GridPosition(1, 0), out var northCrossing));
        Assert.Equal(wall, northCrossing.StructureId);

        Assert.True(structures.TryGetEdgeBetween(new GridPosition(1, 0), new GridPosition(1, 1), out var southCrossing));
        Assert.Equal(northCrossing.Key, southCrossing.Key);
    }

    [Fact]
    public void StructureEdgesRejectDuplicateOppositeTileEdges()
    {
        var structures = new StructureEdgeMap(new GridBounds(4, 4));

        structures.Place(new GridPosition(1, 1), StructureEdgeDirection.North, new StructureId("wall"));

        var ex = Assert.Throws<InvalidOperationException>(() =>
            structures.Place(new GridPosition(1, 0), StructureEdgeDirection.South, new StructureId("window")));
        Assert.Contains("already has a structure", ex.Message);
    }

    [Fact]
    public void StructureEdgesAllowOuterPerimeterEdges()
    {
        var structures = new StructureEdgeMap(new GridBounds(4, 4));

        structures.Place(new GridPosition(3, 3), StructureEdgeDirection.East, new StructureId("wall"));
        structures.Place(new GridPosition(3, 3), StructureEdgeDirection.South, new StructureId("wall"));

        Assert.True(structures.TryGetEdgeAt(new GridPosition(3, 3), StructureEdgeDirection.East, out _));
        Assert.True(structures.TryGetEdgeAt(new GridPosition(3, 3), StructureEdgeDirection.South, out _));
    }

    [Fact]
    public void StructureRenderResolverChoosesDirectionalRunVariants()
    {
        var catalog = CreateCatalog();
        var structures = new StructureEdgeMap(new GridBounds(5, 5));
        structures.Place(new GridPosition(1, 2), StructureEdgeDirection.North, new StructureId("wall"));
        structures.Place(new GridPosition(2, 2), StructureEdgeDirection.North, new StructureId("wall"));
        structures.Place(new GridPosition(3, 2), StructureEdgeDirection.North, new StructureId("wall"));

        var resolver = new StructureRenderResolver();

        Assert.True(structures.TryGetEdgeAt(new GridPosition(1, 2), StructureEdgeDirection.North, out var start));
        Assert.True(structures.TryGetEdgeAt(new GridPosition(2, 2), StructureEdgeDirection.North, out var mid));
        Assert.True(structures.TryGetEdgeAt(new GridPosition(3, 2), StructureEdgeDirection.North, out var end));

        Assert.Equal(StructureVisualOrientation.Front, resolver.Resolve(start, structures, catalog).Orientation);
        Assert.Equal("start", resolver.Resolve(start, structures, catalog).Variant);
        Assert.Equal("mid", resolver.Resolve(mid, structures, catalog).Variant);
        Assert.Equal("end", resolver.Resolve(end, structures, catalog).Variant);
    }

    private static StructureCatalog CreateCatalog()
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
        return catalog;
    }
}
