using System.Text.Json;

namespace SurvivalGame.Domain;

public sealed class TileSurfaceDefinitionLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public TileSurfaceCatalog LoadDirectory(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("Surface definition directory path cannot be empty.", nameof(directoryPath));
        }

        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Surface definition directory does not exist: {directoryPath}");
        }

        var catalog = new TileSurfaceCatalog();
        foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*.json").OrderBy(path => path))
        {
            foreach (var surface in LoadFile(filePath))
            {
                catalog.Add(surface);
            }
        }

        return catalog;
    }

    public IReadOnlyList<TileSurfaceDefinition> LoadFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Surface definition file path cannot be empty.", nameof(filePath));
        }

        var json = File.ReadAllText(filePath);
        var rows = JsonSerializer.Deserialize<List<TileSurfaceDefinitionDto>>(json, JsonOptions)
            ?? throw new InvalidDataException($"Surface definition file is empty or invalid: {filePath}");

        return rows.Select(row => row.ToDefinition(filePath)).ToArray();
    }

    private sealed class TileSurfaceDefinitionDto
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Category { get; set; }

        public string[]? Tags { get; set; }

        public int MovementCost { get; set; } = 1;

        public string? MapColor { get; set; }

        public TileSurfaceDefinition ToDefinition(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new InvalidDataException($"Surface definition in '{sourcePath}' is missing an id.");
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new InvalidDataException($"Surface '{Id}' in '{sourcePath}' is missing a name.");
            }

            if (string.IsNullOrWhiteSpace(Category))
            {
                throw new InvalidDataException($"Surface '{Id}' in '{sourcePath}' is missing a category.");
            }

            return new TileSurfaceDefinition(
                new SurfaceId(Id),
                Name,
                Description ?? string.Empty,
                Category,
                Tags,
                MovementCost,
                MapColor
            );
        }
    }
}
