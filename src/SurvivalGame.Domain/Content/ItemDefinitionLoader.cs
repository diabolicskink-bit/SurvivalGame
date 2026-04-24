using System.Text.Json;

namespace SurvivalGame.Domain;

public sealed class ItemDefinitionLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public ItemCatalog LoadDirectory(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("Item definition directory path cannot be empty.", nameof(directoryPath));
        }

        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Item definition directory does not exist: {directoryPath}");
        }

        var catalog = new ItemCatalog();
        foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*.json").OrderBy(path => path))
        {
            foreach (var item in LoadFile(filePath))
            {
                catalog.Add(item);
            }
        }

        return catalog;
    }

    public IReadOnlyList<ItemDefinition> LoadFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Item definition file path cannot be empty.", nameof(filePath));
        }

        var json = File.ReadAllText(filePath);
        var rows = JsonSerializer.Deserialize<List<ItemDefinitionDto>>(json, JsonOptions)
            ?? throw new InvalidDataException($"Item definition file is empty or invalid: {filePath}");

        return rows.Select(row => row.ToDefinition(filePath)).ToArray();
    }

    private sealed class ItemDefinitionDto
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Category { get; set; }

        public string[]? Tags { get; set; }

        public int MaxStackSize { get; set; } = 1;

        public float Weight { get; set; }

        public string? IconId { get; set; }

        public string? SpriteId { get; set; }

        public string[]? Actions { get; set; }

        public ItemDefinition ToDefinition(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new InvalidDataException($"Item definition in '{sourcePath}' is missing an id.");
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new InvalidDataException($"Item '{Id}' in '{sourcePath}' is missing a name.");
            }

            if (string.IsNullOrWhiteSpace(Category))
            {
                throw new InvalidDataException($"Item '{Id}' in '{sourcePath}' is missing a category.");
            }

            return new ItemDefinition(
                new ItemId(Id),
                Name,
                Description ?? string.Empty,
                Category,
                Tags,
                MaxStackSize,
                Weight,
                IconId,
                SpriteId,
                Actions
            );
        }
    }
}
