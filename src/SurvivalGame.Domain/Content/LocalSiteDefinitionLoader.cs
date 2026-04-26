using System.Text.Json;

namespace SurvivalGame.Domain;

public sealed class LocalSiteDefinitionLoader
{
    private const char DefaultEmptySymbol = '.';

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public IReadOnlyList<PrototypeLocalSite> LoadDirectory(
        string directoryPath,
        TileSurfaceCatalog surfaceCatalog,
        WorldObjectCatalog worldObjectCatalog,
        ItemCatalog itemCatalog,
        NpcCatalog npcCatalog)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("Local map definition directory path cannot be empty.", nameof(directoryPath));
        }

        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Local map definition directory does not exist: {directoryPath}");
        }

        return Directory.EnumerateFiles(directoryPath, "*.json")
            .OrderBy(path => path)
            .Select(filePath => LoadFile(filePath, surfaceCatalog, worldObjectCatalog, itemCatalog, npcCatalog))
            .ToArray();
    }

    public PrototypeLocalSite LoadFile(
        string filePath,
        TileSurfaceCatalog surfaceCatalog,
        WorldObjectCatalog worldObjectCatalog,
        ItemCatalog itemCatalog,
        NpcCatalog npcCatalog)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Local map definition file path cannot be empty.", nameof(filePath));
        }

        var json = File.ReadAllText(filePath);
        var row = JsonSerializer.Deserialize<LocalSiteDefinitionDto>(json, JsonOptions)
            ?? throw new InvalidDataException($"Local map definition file is empty or invalid: {filePath}");

        return row.ToSite(filePath, surfaceCatalog, worldObjectCatalog, itemCatalog, npcCatalog);
    }

    private sealed class LocalSiteDefinitionDto
    {
        public string? Id { get; set; }

        public string? DisplayName { get; set; }

        public string? SourceKind { get; set; }

        public GridSizeDto? Size { get; set; }

        public GridPositionDto? StartPosition { get; set; }

        public string? DefaultSurface { get; set; }

        public MapLayerDto? SurfaceLayer { get; set; }

        public MapLayerDto? ObjectLayer { get; set; }

        public ObjectPlacementDto[]? ObjectPlacements { get; set; }

        public GroundItemPlacementDto[]? Items { get; set; }

        public NpcPlacementDto[]? Npcs { get; set; }

        public PrototypeLocalSite ToSite(
            string sourcePath,
            TileSurfaceCatalog surfaceCatalog,
            WorldObjectCatalog worldObjectCatalog,
            ItemCatalog itemCatalog,
            NpcCatalog npcCatalog)
        {
            var sourceKind = ParseSourceKind(SourceKind, sourcePath);
            return sourceKind switch
            {
                LocalMapSourceKind.Authored => ToAuthoredSite(
                    sourcePath,
                    surfaceCatalog,
                    worldObjectCatalog,
                    itemCatalog,
                    npcCatalog
                ),
                LocalMapSourceKind.Recipe => new LocalMapRecipeSource().Build(),
                LocalMapSourceKind.ChunkedProcedural => new ChunkedLocalMapSource().Build(),
                _ => throw new InvalidDataException($"Unsupported local map source kind '{SourceKind}' in '{sourcePath}'.")
            };
        }

        private PrototypeLocalSite ToAuthoredSite(
            string sourcePath,
            TileSurfaceCatalog surfaceCatalog,
            WorldObjectCatalog worldObjectCatalog,
            ItemCatalog itemCatalog,
            NpcCatalog npcCatalog)
        {
            try
            {
                var size = Required(Size, nameof(Size), sourcePath);
                var start = Required(StartPosition, nameof(StartPosition), sourcePath);
                var bounds = new GridBounds(size.Width, size.Height);
                var builder = new LocalMapBuilder(
                    RequiredString(Id, nameof(Id), sourcePath),
                    RequiredString(DisplayName, nameof(DisplayName), sourcePath),
                    bounds,
                    start.ToGridPosition(),
                    new SurfaceId(RequiredString(DefaultSurface, nameof(DefaultSurface), sourcePath)),
                    surfaceCatalog,
                    worldObjectCatalog,
                    itemCatalog,
                    npcCatalog
                );

                ApplySurfaceLayer(builder, SurfaceLayer, sourcePath);
                ApplyObjectLayer(builder, ObjectLayer, sourcePath);
                ApplyObjectPlacements(builder, ObjectPlacements);
                ApplyGroundItems(builder, Items);
                ApplyNpcs(builder, Npcs);

                return builder.Build();
            }
            catch (Exception ex) when (ex is ArgumentException
                or ArgumentOutOfRangeException
                or InvalidOperationException
                or KeyNotFoundException)
            {
                throw new InvalidDataException($"Local map definition '{sourcePath}' is invalid: {ex.Message}", ex);
            }
        }
    }

    private sealed class GridSizeDto
    {
        public int Width { get; set; }

        public int Height { get; set; }
    }

    private sealed class GridPositionDto
    {
        public int X { get; set; }

        public int Y { get; set; }

        public GridPosition ToGridPosition()
        {
            return new GridPosition(X, Y);
        }
    }

    private sealed class MapLayerDto
    {
        public Dictionary<string, string>? Legend { get; set; }

        public string[]? Rows { get; set; }

        public string? EmptySymbol { get; set; }
    }

    private sealed class ObjectPlacementDto
    {
        public string? ObjectId { get; set; }

        public int X { get; set; }

        public int Y { get; set; }
    }

    private sealed class GroundItemPlacementDto
    {
        public string? ItemId { get; set; }

        public int Quantity { get; set; } = 1;

        public int X { get; set; }

        public int Y { get; set; }
    }

    private sealed class NpcPlacementDto
    {
        public string? InstanceId { get; set; }

        public string? DefinitionId { get; set; }

        public int X { get; set; }

        public int Y { get; set; }
    }

    private static void ApplySurfaceLayer(LocalMapBuilder builder, MapLayerDto? layer, string sourcePath)
    {
        if (layer is null)
        {
            return;
        }

        ApplyLayer(
            builder.Bounds,
            layer,
            sourcePath,
            "surfaceLayer",
            (position, id) => builder.SetSurface(position, new SurfaceId(id))
        );
    }

    private static void ApplyObjectLayer(LocalMapBuilder builder, MapLayerDto? layer, string sourcePath)
    {
        if (layer is null)
        {
            return;
        }

        ApplyLayer(
            builder.Bounds,
            layer,
            sourcePath,
            "objectLayer",
            (position, id) => builder.PlaceWorldObject(position, new WorldObjectId(id))
        );
    }

    private static void ApplyLayer(
        GridBounds bounds,
        MapLayerDto layer,
        string sourcePath,
        string layerName,
        Action<GridPosition, string> applyCell)
    {
        var rows = Required(layer.Rows, $"{layerName}.rows", sourcePath);
        var legend = CreateLegend(layer.Legend, layerName, sourcePath);
        var emptySymbol = ParseEmptySymbol(layer.EmptySymbol, layerName, sourcePath);

        if (rows.Length != bounds.Height)
        {
            throw new InvalidDataException($"{layerName} in '{sourcePath}' must have {bounds.Height} rows but has {rows.Length}.");
        }

        for (var y = 0; y < rows.Length; y++)
        {
            var row = rows[y];
            if (row.Length != bounds.Width)
            {
                throw new InvalidDataException($"{layerName} row {y} in '{sourcePath}' must be {bounds.Width} characters wide but is {row.Length}.");
            }

            for (var x = 0; x < row.Length; x++)
            {
                var symbol = row[x];
                if (symbol == emptySymbol)
                {
                    continue;
                }

                if (!legend.TryGetValue(symbol, out var id))
                {
                    throw new InvalidDataException($"{layerName} in '{sourcePath}' uses undefined symbol '{symbol}' at {x}, {y}.");
                }

                applyCell(new GridPosition(x, y), id);
            }
        }
    }

    private static Dictionary<char, string> CreateLegend(Dictionary<string, string>? rawLegend, string layerName, string sourcePath)
    {
        if (rawLegend is null || rawLegend.Count == 0)
        {
            throw new InvalidDataException($"{layerName}.legend is required in '{sourcePath}'.");
        }

        var legend = new Dictionary<char, string>();
        foreach (var entry in rawLegend)
        {
            if (entry.Key.Length != 1)
            {
                throw new InvalidDataException($"{layerName}.legend key '{entry.Key}' in '{sourcePath}' must be exactly one character.");
            }

            if (string.IsNullOrWhiteSpace(entry.Value))
            {
                throw new InvalidDataException($"{layerName}.legend key '{entry.Key}' in '{sourcePath}' must map to a non-empty id.");
            }

            legend.Add(entry.Key[0], entry.Value.Trim());
        }

        return legend;
    }

    private static char ParseEmptySymbol(string? emptySymbol, string layerName, string sourcePath)
    {
        if (string.IsNullOrEmpty(emptySymbol))
        {
            return DefaultEmptySymbol;
        }

        if (emptySymbol.Length != 1)
        {
            throw new InvalidDataException($"{layerName}.emptySymbol in '{sourcePath}' must be exactly one character.");
        }

        return emptySymbol[0];
    }

    private static void ApplyObjectPlacements(LocalMapBuilder builder, ObjectPlacementDto[]? placements)
    {
        foreach (var placement in placements ?? Array.Empty<ObjectPlacementDto>())
        {
            builder.PlaceWorldObject(
                new GridPosition(placement.X, placement.Y),
                new WorldObjectId(RequiredString(placement.ObjectId, nameof(placement.ObjectId), "object placement"))
            );
        }
    }

    private static void ApplyGroundItems(LocalMapBuilder builder, GroundItemPlacementDto[]? placements)
    {
        foreach (var placement in placements ?? Array.Empty<GroundItemPlacementDto>())
        {
            builder.PlaceGroundItem(
                new GridPosition(placement.X, placement.Y),
                new ItemId(RequiredString(placement.ItemId, nameof(placement.ItemId), "item placement")),
                placement.Quantity
            );
        }
    }

    private static void ApplyNpcs(LocalMapBuilder builder, NpcPlacementDto[]? placements)
    {
        foreach (var placement in placements ?? Array.Empty<NpcPlacementDto>())
        {
            builder.PlaceNpc(
                new GridPosition(placement.X, placement.Y),
                new NpcId(RequiredString(placement.InstanceId, nameof(placement.InstanceId), "NPC placement")),
                new NpcDefinitionId(RequiredString(placement.DefinitionId, nameof(placement.DefinitionId), "NPC placement"))
            );
        }
    }

    private static LocalMapSourceKind ParseSourceKind(string? rawSourceKind, string sourcePath)
    {
        var normalized = (string.IsNullOrWhiteSpace(rawSourceKind) ? "authored" : rawSourceKind.Trim())
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();

        return normalized switch
        {
            "authored" => LocalMapSourceKind.Authored,
            "recipe" => LocalMapSourceKind.Recipe,
            "chunkedprocedural" => LocalMapSourceKind.ChunkedProcedural,
            _ => throw new InvalidDataException($"Unsupported local map source kind '{rawSourceKind}' in '{sourcePath}'.")
        };
    }

    private static T Required<T>(T? value, string name, string sourcePath)
        where T : class
    {
        return value ?? throw new InvalidDataException($"Local map definition in '{sourcePath}' is missing '{name}'.");
    }

    private static string RequiredString(string? value, string name, string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidDataException($"Local map definition in '{sourcePath}' is missing '{name}'.");
        }

        return value.Trim();
    }
}
