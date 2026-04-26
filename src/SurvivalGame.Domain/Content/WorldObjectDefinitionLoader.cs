using System.Text.Json;

namespace SurvivalGame.Domain;

public sealed class WorldObjectDefinitionLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public WorldObjectCatalog LoadDirectory(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("World object definition directory path cannot be empty.", nameof(directoryPath));
        }

        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"World object definition directory does not exist: {directoryPath}");
        }

        var catalog = new WorldObjectCatalog();
        foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*.json").OrderBy(path => path))
        {
            foreach (var worldObject in LoadFile(filePath))
            {
                catalog.Add(worldObject);
            }
        }

        return catalog;
    }

    public IReadOnlyList<WorldObjectDefinition> LoadFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("World object definition file path cannot be empty.", nameof(filePath));
        }

        var json = File.ReadAllText(filePath);
        var rows = JsonSerializer.Deserialize<List<WorldObjectDefinitionDto>>(json, JsonOptions)
            ?? throw new InvalidDataException($"World object definition file is empty or invalid: {filePath}");

        return rows.Select(row => row.ToDefinition(filePath)).ToArray();
    }

    private sealed class WorldObjectDefinitionDto
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Category { get; set; }

        public string[]? Tags { get; set; }

        public bool BlocksMovement { get; set; }

        public bool BlocksSight { get; set; }

        public string? MapColor { get; set; }

        public string? SpriteId { get; set; }

        public SpriteRenderProfileDto? SpriteRender { get; set; }

        public WorldObjectDefinition ToDefinition(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new InvalidDataException($"World object definition in '{sourcePath}' is missing an id.");
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new InvalidDataException($"World object '{Id}' in '{sourcePath}' is missing a name.");
            }

            if (string.IsNullOrWhiteSpace(Category))
            {
                throw new InvalidDataException($"World object '{Id}' in '{sourcePath}' is missing a category.");
            }

            return new WorldObjectDefinition(
                new WorldObjectId(Id),
                Name,
                Description ?? string.Empty,
                Category,
                Tags,
                BlocksMovement,
                BlocksSight,
                MapColor,
                SpriteId,
                SpriteRender?.ToProfile()
            );
        }
    }

    private sealed class SpriteRenderProfileDto
    {
        public float WidthTiles { get; set; } = 1f;

        public float HeightTiles { get; set; } = 1f;

        public float OffsetXTiles { get; set; }

        public float OffsetYTiles { get; set; }

        public float SortOffsetYTiles { get; set; }

        public SpriteRenderProfile ToProfile()
        {
            return new SpriteRenderProfile(
                WidthTiles,
                HeightTiles,
                OffsetXTiles,
                OffsetYTiles,
                SortOffsetYTiles
            );
        }
    }
}
