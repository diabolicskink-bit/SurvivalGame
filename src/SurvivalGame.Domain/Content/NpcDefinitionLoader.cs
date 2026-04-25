using System.Text.Json;

namespace SurvivalGame.Domain;

public sealed class NpcDefinitionLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public NpcCatalog LoadDirectory(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("NPC definition directory path cannot be empty.", nameof(directoryPath));
        }

        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"NPC definition directory does not exist: {directoryPath}");
        }

        var catalog = new NpcCatalog();
        foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*.json").OrderBy(path => path))
        {
            foreach (var npc in LoadFile(filePath))
            {
                catalog.Add(npc);
            }
        }

        return catalog;
    }

    public IReadOnlyList<NpcDefinition> LoadFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("NPC definition file path cannot be empty.", nameof(filePath));
        }

        var json = File.ReadAllText(filePath);
        var rows = JsonSerializer.Deserialize<List<NpcDefinitionDto>>(json, JsonOptions)
            ?? throw new InvalidDataException($"NPC definition file is empty or invalid: {filePath}");

        return rows.Select(row => row.ToDefinition(filePath)).ToArray();
    }

    private sealed class NpcDefinitionDto
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public string? Description { get; set; }

        public string? Species { get; set; }

        public string[]? Tags { get; set; }

        public int MaximumHealth { get; set; }

        public bool BlocksMovement { get; set; } = true;

        public string? MapColor { get; set; }

        public NpcBehaviorProfileDto? Behavior { get; set; }

        public NpcDefinition ToDefinition(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new InvalidDataException($"NPC definition in '{sourcePath}' is missing an id.");
            }

            var displayName = DisplayName ?? Name;
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new InvalidDataException($"NPC definition '{Id}' in '{sourcePath}' is missing a name.");
            }

            if (string.IsNullOrWhiteSpace(Species))
            {
                throw new InvalidDataException($"NPC definition '{Id}' in '{sourcePath}' is missing a species.");
            }

            return new NpcDefinition(
                new NpcDefinitionId(Id),
                displayName,
                Description ?? string.Empty,
                Species,
                MaximumHealth,
                Tags,
                BlocksMovement,
                MapColor,
                Behavior?.ToProfile(sourcePath, Id) ?? NpcBehaviorProfile.Inert
            );
        }
    }

    private sealed class NpcBehaviorProfileDto
    {
        public string? Kind { get; set; }

        public int PerceptionRange { get; set; }

        public string[]? Tags { get; set; }

        public NpcBehaviorProfile ToProfile(string sourcePath, string npcId)
        {
            var kindText = string.IsNullOrWhiteSpace(Kind) ? nameof(NpcBehaviorKind.Inert) : Kind.Trim();
            if (!Enum.TryParse<NpcBehaviorKind>(kindText, ignoreCase: true, out var kind))
            {
                throw new InvalidDataException(
                    $"NPC definition '{npcId}' in '{sourcePath}' has unknown behavior kind '{Kind}'."
                );
            }

            return new NpcBehaviorProfile(kind, PerceptionRange, Tags);
        }
    }
}
