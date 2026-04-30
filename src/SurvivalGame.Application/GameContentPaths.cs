namespace SurvivalGame.Application;

public sealed class GameContentPaths
{
    private GameContentPaths(
        string dataRoot,
        string items,
        string firearms,
        string surfaces,
        string worldObjects,
        string structures,
        string npcs,
        string localMaps)
    {
        DataRoot = dataRoot;
        Items = items;
        Firearms = firearms;
        Surfaces = surfaces;
        WorldObjects = worldObjects;
        Structures = structures;
        Npcs = npcs;
        LocalMaps = localMaps;
    }

    public string DataRoot { get; }

    public string Items { get; }

    public string Firearms { get; }

    public string Surfaces { get; }

    public string WorldObjects { get; }

    public string Structures { get; }

    public string Npcs { get; }

    public string LocalMaps { get; }

    public static GameContentPaths FromDataRoot(string dataRoot)
    {
        if (string.IsNullOrWhiteSpace(dataRoot))
        {
            throw new ArgumentException("Data root path cannot be empty.", nameof(dataRoot));
        }

        return new GameContentPaths(
            dataRoot,
            Path.Combine(dataRoot, "items"),
            Path.Combine(dataRoot, "firearms"),
            Path.Combine(dataRoot, "surfaces"),
            Path.Combine(dataRoot, "world_objects"),
            Path.Combine(dataRoot, "structures"),
            Path.Combine(dataRoot, "npcs"),
            Path.Combine(dataRoot, "local_maps")
        );
    }
}
