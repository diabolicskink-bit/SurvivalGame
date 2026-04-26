namespace SurvivalGame.Domain;

public static class PrototypeWorldMapSites
{
    public static readonly WorldMapDefinition Definition = LoadColoradoDefinition();

    public static double MapWidth => Definition.MapWidth;

    public static double MapHeight => Definition.MapHeight;

    public static double VisibleWidth => Definition.VisibleWidth;

    public static double VisibleHeight => Definition.VisibleHeight;

    public static WorldMapPosition StartPosition => Definition.StartPosition;

    public static IReadOnlyList<WorldMapPointOfInterest> All => Definition.PointsOfInterest;

    private static WorldMapDefinition LoadColoradoDefinition()
    {
        var path = ResolveDataPath("data", "world_map", "colorado.json");
        return new WorldMapDefinitionLoader().LoadFile(path);
    }

    private static string ResolveDataPath(params string[] pathParts)
    {
        var relativePath = Path.Combine(pathParts);
        var candidates = new[]
        {
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory
        };

        foreach (var candidate in candidates)
        {
            var directory = new DirectoryInfo(candidate);
            while (directory is not null)
            {
                var path = Path.Combine(directory.FullName, relativePath);
                if (File.Exists(path))
                {
                    return path;
                }

                directory = directory.Parent;
            }
        }

        throw new FileNotFoundException($"Could not find required data file '{relativePath}'.");
    }
}
