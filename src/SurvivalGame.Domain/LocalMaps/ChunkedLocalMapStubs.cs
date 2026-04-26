namespace SurvivalGame.Domain;

public readonly record struct LocalMapChunkCoordinate(int X, int Y);

public sealed record LocalMapChunkDefinition(
    LocalMapChunkCoordinate Coordinate,
    int Width,
    int Height
);

public sealed record LocalMapGeneratorDefinition(
    string Id,
    int Seed,
    IReadOnlyDictionary<string, string>? Parameters = null
);

public sealed class ChunkedLocalMapSource
{
    public PrototypeLocalSite Build()
    {
        throw new NotSupportedException("Chunked procedural map generation is not implemented yet.");
    }
}
