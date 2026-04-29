using System.Text.Json;

namespace SurvivalGame.Domain;

public sealed class WorldMapDefinitionLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public WorldMapDefinition LoadFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("World map definition path is required.", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"World map definition file '{path}' was not found.", path);
        }

        var json = File.ReadAllText(path);
        var dto = JsonSerializer.Deserialize<WorldMapDefinitionDto>(json, JsonOptions)
            ?? throw new InvalidOperationException($"World map definition '{path}' is empty or invalid.");

        return ToDefinition(dto, path);
    }

    private static WorldMapDefinition ToDefinition(WorldMapDefinitionDto dto, string sourcePath)
    {
        var boundsDto = dto.Projection?.Bounds
            ?? throw new InvalidOperationException($"World map definition '{sourcePath}' is missing projection bounds.");
        var bounds = new WorldMapBounds(
            boundsDto.MinLongitude,
            boundsDto.MaxLongitude,
            boundsDto.MinLatitude,
            boundsDto.MaxLatitude
        );

        var mapWidth = RequirePositive(dto.MapWidth, nameof(dto.MapWidth), sourcePath);
        var mapHeight = RequirePositive(dto.MapHeight, nameof(dto.MapHeight), sourcePath);
        var visibleWidth = RequirePositive(dto.VisibleWidth, nameof(dto.VisibleWidth), sourcePath);
        var visibleHeight = RequirePositive(dto.VisibleHeight, nameof(dto.VisibleHeight), sourcePath);
        var projector = new CoordinateProjector(bounds, mapWidth, mapHeight);

        var start = dto.Start is null
            ? throw new InvalidOperationException($"World map definition '{sourcePath}' is missing a start coordinate.")
            : projector.Project(dto.Start.Longitude, dto.Start.Latitude);

        var terrainRegions = (dto.TerrainRegions ?? Array.Empty<TerrainRegionDto>())
            .Select(region => ToTerrainRegion(region, projector, sourcePath))
            .ToArray();
        var terrainGrid = LoadTerrainGrid(dto, sourcePath);

        var roads = LoadRoadDtos(dto, sourcePath)
            .Select(road => ToRoad(road, projector, sourcePath))
            .ToArray();

        var points = (dto.PointsOfInterest ?? Array.Empty<PointOfInterestDto>())
            .Select(point => ToPoint(point, projector, sourcePath))
            .ToArray();

        return new WorldMapDefinition(
            Required(dto.Id, "map id", sourcePath),
            Required(dto.DisplayName, "map display name", sourcePath),
            mapWidth,
            mapHeight,
            visibleWidth,
            visibleHeight,
            bounds,
            start,
            points,
            roads,
            terrainRegions,
            terrainGrid,
            dto.BackgroundTexture
        );
    }

    private static WorldMapPointOfInterest ToPoint(
        PointOfInterestDto dto,
        CoordinateProjector projector,
        string sourcePath)
    {
        return new WorldMapPointOfInterest(
            Required(dto.Id, "point id", sourcePath),
            Required(dto.DisplayName, "point display name", sourcePath),
            projector.Project(dto.Longitude, dto.Latitude),
            dto.EnterRadius <= 0 ? 42.0 : dto.EnterRadius,
            ParseEnum(dto.Category, WorldMapPointCategory.Landmark),
            dto.LabelPriority,
            dto.LocalSiteId
        );
    }

    private static WorldMapRoad ToRoad(RoadDto dto, CoordinateProjector projector, string sourcePath)
    {
        var segments = new List<WorldMapRoadSegment>();
        if (dto.Segments is { Length: > 0 })
        {
            foreach (var segment in dto.Segments)
            {
                var coordinates = segment ?? Array.Empty<CoordinateDto>();
                segments.Add(new WorldMapRoadSegment(
                    coordinates.Select(point => projector.Project(point.Longitude, point.Latitude)).ToArray()
                ));
            }
        }
        else if (dto.Points is { Length: > 0 } points)
        {
            segments.Add(new WorldMapRoadSegment(
                points.Select(point => projector.Project(point.Longitude, point.Latitude)).ToArray()
            ));
        }

        var kind = ParseRoadKind(dto.Kind);
        var laneCount = dto.LaneCount > 0 ? dto.LaneCount : DefaultLaneCount(kind);

        return new WorldMapRoad(
            Required(dto.Id, "road id", sourcePath),
            Required(dto.DisplayName, "road display name", sourcePath),
            kind,
            segments,
            dto.Priority > 0 ? dto.Priority : DefaultRoadPriority(kind),
            laneCount,
            dto.MapLanesPerDirection > 0
                ? dto.MapLanesPerDirection
                : MapLanesPerDirectionFromLaneCount(laneCount),
            dto.SurfaceWidthFeet > 0 ? dto.SurfaceWidthFeet : laneCount * 12.0,
            dto.TravelInfluenceRadius <= 0 ? DefaultRoadTravelRadius(kind, laneCount) : dto.TravelInfluenceRadius
        );
    }

    private static IReadOnlyList<RoadDto> LoadRoadDtos(WorldMapDefinitionDto dto, string sourcePath)
    {
        var roads = new List<RoadDto>();
        if (dto.Roads is { Length: > 0 })
        {
            roads.AddRange(dto.Roads);
        }

        foreach (var roadFile in dto.RoadFiles ?? Array.Empty<string>())
        {
            var filePath = ResolveSiblingPath(sourcePath, roadFile);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"World map road file '{filePath}' was not found.", filePath);
            }

            var json = File.ReadAllText(filePath);
            var roadFileDto = JsonSerializer.Deserialize<WorldMapRoadFileDto>(json, JsonOptions)
                ?? throw new InvalidOperationException($"World map road file '{filePath}' is empty or invalid.");
            if (roadFileDto.Roads is { Length: > 0 })
            {
                roads.AddRange(roadFileDto.Roads);
            }
        }

        return roads;
    }

    private static WorldMapTerrainGrid? LoadTerrainGrid(WorldMapDefinitionDto dto, string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(dto.TerrainGridFile))
        {
            return null;
        }

        var filePath = ResolveSiblingPath(sourcePath, dto.TerrainGridFile);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"World map terrain grid file '{filePath}' was not found.", filePath);
        }

        var json = File.ReadAllText(filePath);
        var gridDto = JsonSerializer.Deserialize<WorldMapTerrainGridDto>(json, JsonOptions)
            ?? throw new InvalidOperationException($"World map terrain grid file '{filePath}' is empty or invalid.");
        var legend = gridDto.Legend ?? throw new InvalidOperationException($"World map terrain grid file '{filePath}' is missing legend.");
        var profiles = new Dictionary<char, WorldMapTerrainProfile>();
        foreach (var (rawCode, profile) in legend)
        {
            if (rawCode.Length != 1)
            {
                throw new InvalidOperationException($"World map terrain grid file '{filePath}' has invalid terrain code '{rawCode}'.");
            }

            profiles[rawCode[0]] = new WorldMapTerrainProfile(
                ParseEnum(profile.Kind, WorldMapTerrainKind.Plains),
                profile.SpeedMultiplier <= 0 ? 1.0 : profile.SpeedMultiplier,
                profile.FuelUseMultiplier < 0 ? 1.0 : profile.FuelUseMultiplier,
                string.IsNullOrWhiteSpace(profile.DisplayName)
                    ? rawCode
                    : profile.DisplayName.Trim()
            );
        }

        return new WorldMapTerrainGrid(
            gridDto.Width,
            gridDto.Height,
            gridDto.Rows ?? Array.Empty<string>(),
            profiles
        );
    }

    private static string ResolveSiblingPath(string sourcePath, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException($"World map definition '{sourcePath}' contains an empty road file path.");
        }

        return Path.IsPathRooted(path)
            ? path
            : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourcePath) ?? string.Empty, path));
    }

    private static WorldMapRoadKind ParseRoadKind(string? raw)
    {
        return raw?.Trim().Replace("-", "_", StringComparison.Ordinal).ToLowerInvariant() switch
        {
            "interstate" => WorldMapRoadKind.Interstate,
            "us_highway" => WorldMapRoadKind.UsHighway,
            "state_highway" => WorldMapRoadKind.StateHighway,
            "major_road" => WorldMapRoadKind.MajorRoad,
            _ => WorldMapRoadKind.MajorRoad
        };
    }

    private static int DefaultRoadPriority(WorldMapRoadKind kind)
    {
        return kind switch
        {
            WorldMapRoadKind.Interstate => 1,
            WorldMapRoadKind.UsHighway => 2,
            WorldMapRoadKind.StateHighway => 3,
            _ => 4
        };
    }

    private static int DefaultLaneCount(WorldMapRoadKind kind)
    {
        return kind switch
        {
            WorldMapRoadKind.Interstate => 4,
            _ => 2
        };
    }

    private static double DefaultRoadTravelRadius(WorldMapRoadKind kind, int laneCount)
    {
        var baseRadius = kind switch
        {
            WorldMapRoadKind.Interstate => 136.0,
            WorldMapRoadKind.UsHighway => 112.0,
            WorldMapRoadKind.StateHighway => 88.0,
            _ => 72.0
        };

        return baseRadius + (Math.Max(0, laneCount - 2) * 6.0);
    }

    private static int MapLanesPerDirectionFromLaneCount(int laneCount)
    {
        if (laneCount <= 2)
        {
            return 1;
        }

        if (laneCount <= 4)
        {
            return 2;
        }

        return 3;
    }

    private static WorldMapTerrainRegion ToTerrainRegion(
        TerrainRegionDto dto,
        CoordinateProjector projector,
        string sourcePath)
    {
        var coordinates = dto.Points ?? Array.Empty<CoordinateDto>();
        return new WorldMapTerrainRegion(
            Required(dto.Id, "terrain region id", sourcePath),
            Required(dto.DisplayName, "terrain region display name", sourcePath),
            ParseEnum(dto.Kind, WorldMapTerrainKind.Plains),
            coordinates.Select(point => projector.Project(point.Longitude, point.Latitude)).ToArray(),
            dto.SpeedMultiplier <= 0 ? 1.0 : dto.SpeedMultiplier,
            dto.FuelUseMultiplier < 0 ? 1.0 : dto.FuelUseMultiplier,
            dto.MapColor ?? "#31401f"
        );
    }

    private static TEnum ParseEnum<TEnum>(string? raw, TEnum fallback)
        where TEnum : struct
    {
        return Enum.TryParse<TEnum>(raw, ignoreCase: true, out var value) ? value : fallback;
    }

    private static double RequirePositive(double value, string fieldName, string sourcePath)
    {
        if (value <= 0)
        {
            throw new InvalidOperationException($"World map definition '{sourcePath}' has invalid {fieldName}.");
        }

        return value;
    }

    private static string Required(string? value, string fieldName, string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"World map definition '{sourcePath}' is missing {fieldName}.");
        }

        return value.Trim();
    }

    private sealed record CoordinateProjector(WorldMapBounds Bounds, double MapWidth, double MapHeight)
    {
        public WorldMapPosition Project(double longitude, double latitude)
        {
            var x = (longitude - Bounds.MinLongitude) / (Bounds.MaxLongitude - Bounds.MinLongitude) * MapWidth;
            var y = (Bounds.MaxLatitude - latitude) / (Bounds.MaxLatitude - Bounds.MinLatitude) * MapHeight;
            return new WorldMapPosition(Math.Clamp(x, 0, MapWidth), Math.Clamp(y, 0, MapHeight));
        }
    }

    private sealed class WorldMapDefinitionDto
    {
        public string? Id { get; set; }
        public string? DisplayName { get; set; }
        public double MapWidth { get; set; }
        public double MapHeight { get; set; }
        public double VisibleWidth { get; set; }
        public double VisibleHeight { get; set; }
        public ProjectionDto? Projection { get; set; }
        public CoordinateDto? Start { get; set; }
        public PointOfInterestDto[]? PointsOfInterest { get; set; }
        public RoadDto[]? Roads { get; set; }
        public string[]? RoadFiles { get; set; }
        public string? TerrainGridFile { get; set; }
        public string? BackgroundTexture { get; set; }
        public TerrainRegionDto[]? TerrainRegions { get; set; }
    }

    private sealed class WorldMapRoadFileDto
    {
        public RoadDto[]? Roads { get; set; }
    }

    private sealed class WorldMapTerrainGridDto
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Dictionary<string, TerrainProfileDto>? Legend { get; set; }
        public string[]? Rows { get; set; }
    }

    private sealed class TerrainProfileDto
    {
        public string? Kind { get; set; }
        public string? DisplayName { get; set; }
        public double SpeedMultiplier { get; set; } = 1.0;
        public double FuelUseMultiplier { get; set; } = 1.0;
    }

    private sealed class ProjectionDto
    {
        public WorldMapBoundsDto? Bounds { get; set; }
    }

    private sealed class WorldMapBoundsDto
    {
        public double MinLongitude { get; set; }
        public double MaxLongitude { get; set; }
        public double MinLatitude { get; set; }
        public double MaxLatitude { get; set; }
    }

    private class CoordinateDto
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }

    private sealed class PointOfInterestDto : CoordinateDto
    {
        public string? Id { get; set; }
        public string? DisplayName { get; set; }
        public string? Category { get; set; }
        public int LabelPriority { get; set; } = 3;
        public double EnterRadius { get; set; } = 42.0;
        public string? LocalSiteId { get; set; }
    }

    private sealed class RoadDto
    {
        public string? Id { get; set; }
        public string? DisplayName { get; set; }
        public string? Kind { get; set; }
        public int Priority { get; set; } = 2;
        public int LaneCount { get; set; }
        public int MapLanesPerDirection { get; set; }
        public double SurfaceWidthFeet { get; set; }
        public double TravelInfluenceRadius { get; set; }
        public CoordinateDto[]? Points { get; set; }
        public CoordinateDto[][]? Segments { get; set; }
    }

    private sealed class TerrainRegionDto
    {
        public string? Id { get; set; }
        public string? DisplayName { get; set; }
        public string? Kind { get; set; }
        public double SpeedMultiplier { get; set; } = 1.0;
        public double FuelUseMultiplier { get; set; } = 1.0;
        public string? MapColor { get; set; }
        public CoordinateDto[]? Points { get; set; }
    }
}
