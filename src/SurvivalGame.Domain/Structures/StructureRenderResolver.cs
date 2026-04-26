namespace SurvivalGame.Domain;

public enum StructureVisualOrientation
{
    Front,
    Side
}

public sealed record StructureRenderInfo(
    PlacedStructureEdge Edge,
    StructureDefinition Definition,
    StructureVisualOrientation Orientation,
    string Variant,
    string SpriteId);

public sealed class StructureRenderResolver
{
    public StructureRenderInfo Resolve(
        PlacedStructureEdge edge,
        StructureEdgeMap structures,
        StructureCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(structures);
        ArgumentNullException.ThrowIfNull(catalog);

        var definition = catalog.Get(edge.StructureId);
        var orientation = edge.Key.Axis == StructureEdgeAxis.Horizontal
            ? StructureVisualOrientation.Front
            : StructureVisualOrientation.Side;
        var variant = ResolveVariant(edge, structures, catalog, definition);
        var spriteId = FormatSpriteId(definition, orientation, variant);

        return new StructureRenderInfo(edge, definition, orientation, variant, spriteId);
    }

    private static string ResolveVariant(
        PlacedStructureEdge edge,
        StructureEdgeMap structures,
        StructureCatalog catalog,
        StructureDefinition definition)
    {
        if (!definition.ConnectsAsWall)
        {
            return "opening";
        }

        var connectsBefore = Connects(edge.Key.NeighborBefore(), structures, catalog, definition);
        var connectsAfter = Connects(edge.Key.NeighborAfter(), structures, catalog, definition);

        return (connectsBefore, connectsAfter) switch
        {
            (true, true) => "mid",
            (false, true) => "start",
            (true, false) => "end",
            _ => "single"
        };
    }

    private static bool Connects(
        StructureEdgeKey key,
        StructureEdgeMap structures,
        StructureCatalog catalog,
        StructureDefinition sourceDefinition)
    {
        if (!structures.TryGetEdge(key, out var neighbor))
        {
            return false;
        }

        if (!catalog.TryGet(neighbor.StructureId, out var neighborDefinition))
        {
            return false;
        }

        return neighborDefinition.ConnectsAsWall
            && string.Equals(neighborDefinition.StyleId, sourceDefinition.StyleId, StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatSpriteId(
        StructureDefinition definition,
        StructureVisualOrientation orientation,
        string variant)
    {
        return $"structure_{definition.StyleId}_{definition.PieceKind}_{orientation.ToString().ToLowerInvariant()}_{variant}";
    }
}
