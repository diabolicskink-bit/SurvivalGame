using System.Text.Json;

namespace SurvivalGame.Domain;

public sealed class StructureDefinitionLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public StructureCatalog LoadDirectory(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("Structure definition directory path cannot be empty.", nameof(directoryPath));
        }

        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Structure definition directory does not exist: {directoryPath}");
        }

        var catalog = new StructureCatalog();
        foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*.json").OrderBy(path => path))
        {
            foreach (var structure in LoadFile(filePath))
            {
                catalog.Add(structure);
            }
        }

        return catalog;
    }

    public IReadOnlyList<StructureDefinition> LoadFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Structure definition file path cannot be empty.", nameof(filePath));
        }

        var json = File.ReadAllText(filePath);
        var rows = JsonSerializer.Deserialize<List<StructureDefinitionDto>>(json, JsonOptions)
            ?? throw new InvalidDataException($"Structure definition file is empty or invalid: {filePath}");

        return rows.Select(row => row.ToDefinition(filePath)).ToArray();
    }

    private sealed class StructureDefinitionDto
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Category { get; set; }

        public string? StyleId { get; set; }

        public string? PieceKind { get; set; }

        public string[]? Tags { get; set; }

        public bool BlocksMovement { get; set; }

        public bool BlocksSight { get; set; }

        public bool ConnectsAsWall { get; set; } = true;

        public string? MapColor { get; set; }

        public StructureDefinition ToDefinition(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new InvalidDataException($"Structure definition in '{sourcePath}' is missing an id.");
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new InvalidDataException($"Structure '{Id}' in '{sourcePath}' is missing a name.");
            }

            if (string.IsNullOrWhiteSpace(Category))
            {
                throw new InvalidDataException($"Structure '{Id}' in '{sourcePath}' is missing a category.");
            }

            if (string.IsNullOrWhiteSpace(StyleId))
            {
                throw new InvalidDataException($"Structure '{Id}' in '{sourcePath}' is missing a styleId.");
            }

            if (string.IsNullOrWhiteSpace(PieceKind))
            {
                throw new InvalidDataException($"Structure '{Id}' in '{sourcePath}' is missing a pieceKind.");
            }

            return new StructureDefinition(
                new StructureId(Id),
                Name,
                Description ?? string.Empty,
                Category,
                StyleId,
                PieceKind,
                Tags,
                BlocksMovement,
                BlocksSight,
                ConnectsAsWall,
                MapColor
            );
        }
    }
}
