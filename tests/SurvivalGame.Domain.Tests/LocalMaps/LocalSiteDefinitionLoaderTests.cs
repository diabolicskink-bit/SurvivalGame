using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class LocalSiteDefinitionLoaderTests
{
    [Fact]
    public void LoadsCurrentAuthoredLocalMapsFromData()
    {
        var sites = LoadAllSites();

        var defaultSite = Assert.Single(sites, site => site.Id == PrototypeLocalSites.DefaultSiteId);
        Assert.Equal(PrototypeLocalSites.DefaultBounds, defaultSite.Bounds);
        Assert.Equal(PrototypeLocalSites.DefaultBounds.Center, defaultSite.StartPosition);
        Assert.Equal(PrototypeSurfaces.Concrete, defaultSite.Surfaces.GetSurfaceId(new GridPosition(2, 2)));
        Assert.Equal(PrototypeSurfaces.Carpet, defaultSite.Surfaces.GetSurfaceId(new GridPosition(3, 3)));
        Assert.Equal(PrototypeSurfaces.Tile, defaultSite.Surfaces.GetSurfaceId(new GridPosition(11, 2)));
        Assert.Equal(PrototypeSurfaces.Ice, defaultSite.Surfaces.GetSurfaceId(new GridPosition(12, 8)));
        Assert.True(defaultSite.WorldObjects.TryGetObjectAt(new GridPosition(6, 6), out var door));
        Assert.Equal(PrototypeWorldObjects.WoodenDoor, door);
        Assert.True(defaultSite.WorldObjects.TryGetPlacement(new WorldObjectInstanceId("prototype_fridge_01"), out var fridgePlacement));
        Assert.Equal(new GridPosition(8, 3), fridgePlacement.Position);
        Assert.Contains(
            fridgePlacement.ContainerLoot!.FixedStacks,
            stack => stack.ItemId == new ItemId("canned_beans") && stack.Quantity == 2);
        Assert.True(defaultSite.WorldObjects.TryGetPlacement(new WorldObjectInstanceId("prototype_storage_crate_01"), out var cratePlacement));
        Assert.Equal(new GridPosition(15, 5), cratePlacement.Position);
        Assert.Contains(
            defaultSite.GroundItems.ItemsAt(new GridPosition(4, 4)),
            stack => stack.ItemId == PrototypeItems.Stone && stack.Quantity == 2);
        Assert.True(defaultSite.Npcs.TryGetAt(new GridPosition(14, 8), out var testDummy));
        Assert.Equal(PrototypeNpcs.TestDummy, testDummy.Id);
        AssertArrivalAnchor(defaultSite, TravelMethodId.Vehicle, new GridPosition(10, 10), WorldObjectFacing.East);
        AssertArrivalAnchor(defaultSite, TravelMethodId.Pushbike, new GridPosition(9, 7), WorldObjectFacing.North);

        var gasStation = Assert.Single(sites, site => site.Id == PrototypeLocalSites.GasStationSiteId);
        Assert.Equal(PrototypeLocalSites.GasStationBounds, gasStation.Bounds);
        Assert.Equal(new GridPosition(21, 14), gasStation.StartPosition);
        Assert.True(gasStation.WorldObjects.TryGetObjectAt(new GridPosition(25, 9), out var pump));
        Assert.Equal(PrototypeWorldObjects.FuelPump, pump);
        Assert.True(gasStation.WorldObjects.TryGetPlacementAt(new GridPosition(7, 6), out var shelfPlacement));
        Assert.Equal(new WorldObjectInstanceId("store_shelf@7,6"), shelfPlacement.InstanceId);
        Assert.True(gasStation.Npcs.TryGetAt(new GridPosition(30, 12), out var turret));
        Assert.Equal(PrototypeNpcs.GasStationTurret, turret.Id);
        AssertArrivalAnchor(gasStation, TravelMethodId.Vehicle, new GridPosition(17, 14), WorldObjectFacing.East);
        AssertArrivalAnchor(gasStation, TravelMethodId.Pushbike, new GridPosition(21, 16), WorldObjectFacing.North);

        var farmstead = Assert.Single(sites, site => site.Id == PrototypeLocalSites.FarmsteadSiteId);
        Assert.Equal(PrototypeLocalSites.FarmsteadBounds, farmstead.Bounds);
        Assert.Equal(new GridPosition(4, 41), farmstead.StartPosition);
        Assert.Equal(PrototypeSurfaces.Dirt, farmstead.Surfaces.GetSurfaceId(farmstead.StartPosition));
        Assert.Equal(PrototypeSurfaces.WeatheredWood, farmstead.Surfaces.GetSurfaceId(new GridPosition(26, 30)));
        Assert.True(farmstead.WorldObjects.TryGetObjectAt(new GridPosition(17, 13), out var farmhouseWall));
        Assert.Equal(new WorldObjectId("wall"), farmhouseWall);
        Assert.True(farmstead.WorldObjects.TryGetObjectAt(new GridPosition(32, 13), out var farmhouseWindow));
        Assert.Equal(new WorldObjectId("window"), farmhouseWindow);
        Assert.False(farmstead.Structures.TryGetEdgeAt(new GridPosition(26, 28), StructureEdgeDirection.South, out _));
        Assert.True(farmstead.Structures.TryGetEdgeAt(new GridPosition(44, 34), StructureEdgeDirection.South, out var paddockGate));
        Assert.Equal(new StructureId("open_farm_gate"), paddockGate.StructureId);
        Assert.True(farmstead.WorldObjects.TryGetObjectAt(new GridPosition(42, 7), out var tank));
        Assert.Equal(new WorldObjectId("water_tank"), tank);
        Assert.Contains(
            farmstead.GroundItems.ItemsAt(new GridPosition(49, 17)),
            stack => stack.ItemId == new ItemId("hammer") && stack.Quantity == 1);
        AssertArrivalAnchor(farmstead, TravelMethodId.Vehicle, new GridPosition(1, 39), WorldObjectFacing.East);
        AssertArrivalAnchor(farmstead, TravelMethodId.Pushbike, new GridPosition(5, 40), WorldObjectFacing.North);
    }

    [Fact]
    public void AuthoredMapRejectsWrongRowWidth()
    {
        var ex = Assert.Throws<InvalidDataException>(() => LoadSingleFromJson(
            """
            {
              "id": "bad_width",
              "displayName": "Bad Width",
              "sourceKind": "authored",
              "size": { "width": 2, "height": 2 },
              "startPosition": { "x": 0, "y": 0 },
              "defaultSurface": "grass",
              "surfaceLayer": {
                "legend": { "g": "grass" },
                "rows": [ "gg", "g" ]
              }
            }
            """));

        Assert.Contains("row 1", ex.Message);
    }

    [Fact]
    public void AuthoredMapRejectsUnknownIds()
    {
        var ex = Assert.Throws<InvalidDataException>(() => LoadSingleFromJson(
            """
            {
              "id": "bad_surface",
              "displayName": "Bad Surface",
              "sourceKind": "authored",
              "size": { "width": 1, "height": 1 },
              "startPosition": { "x": 0, "y": 0 },
              "defaultSurface": "missing_surface"
            }
            """));

        Assert.Contains("missing_surface", ex.Message);
    }

    [Fact]
    public void AuthoredMapRejectsOutOfBoundsPlacements()
    {
        var ex = Assert.Throws<InvalidDataException>(() => LoadSingleFromJson(
            """
            {
              "id": "bad_item_position",
              "displayName": "Bad Item Position",
              "sourceKind": "authored",
              "size": { "width": 1, "height": 1 },
              "startPosition": { "x": 0, "y": 0 },
              "defaultSurface": "grass",
              "items": [
                { "itemId": "stone", "quantity": 1, "x": 2, "y": 0 }
              ]
            }
            """));

        Assert.Contains("inside map bounds", ex.Message);
    }

    [Fact]
    public void AuthoredMapRejectsDuplicateObjectPlacements()
    {
        var ex = Assert.Throws<InvalidDataException>(() => LoadSingleFromJson(
            """
            {
              "id": "duplicate_object",
              "displayName": "Duplicate Object",
              "sourceKind": "authored",
              "size": { "width": 1, "height": 1 },
              "startPosition": { "x": 0, "y": 0 },
              "defaultSurface": "grass",
              "objectLayer": {
                "legend": { "#": "wall" },
                "rows": [ "#" ]
              },
              "objectPlacements": [
                { "objectId": "tree", "x": 0, "y": 0 }
              ]
            }
            """));

        Assert.Contains("already has a world object", ex.Message);
    }

    [Fact]
    public void AuthoredMapLoadsObjectPlacementFacing()
    {
        var site = LoadSingleFromJson(
            """
            {
              "id": "facing_object",
              "displayName": "Facing Object",
              "sourceKind": "authored",
              "size": { "width": 12, "height": 12 },
              "startPosition": { "x": 0, "y": 0 },
              "defaultSurface": "grass",
              "objectPlacements": [
                { "objectId": "abandoned_vehicle", "x": 2, "y": 3, "facing": "east" }
              ]
            }
            """);

        Assert.True(site.WorldObjects.TryGetPlacementAt(new GridPosition(5, 4), out var placement));
        Assert.Equal(new GridPosition(2, 3), placement.Position);
        Assert.Equal(PrototypeWorldObjects.AbandonedVehicle, placement.ObjectId);
        Assert.Equal(WorldObjectFacing.East, placement.Facing);
        Assert.Equal(new WorldObjectFootprint(2, 4), placement.Footprint);
        Assert.Equal(new WorldObjectFootprint(4, 2), placement.EffectiveFootprint);
    }

    [Fact]
    public void AuthoredMapLoadsStructureEdges()
    {
        var site = LoadSingleFromJson(
            """
            {
              "id": "structure_edges",
              "displayName": "Structure Edges",
              "sourceKind": "authored",
              "size": { "width": 3, "height": 3 },
              "startPosition": { "x": 1, "y": 1 },
              "defaultSurface": "grass",
              "structureEdges": [
                { "structureId": "wall", "x": 1, "y": 1, "edge": "north" },
                { "structureId": "open_doorway", "x": 1, "y": 1, "edge": "east" }
              ]
            }
            """,
            CreateStructureCatalog());

        Assert.True(site.Structures.TryGetEdgeAt(new GridPosition(1, 1), StructureEdgeDirection.North, out var wall));
        Assert.Equal(new StructureId("wall"), wall.StructureId);
        Assert.True(site.Structures.TryGetEdgeAt(new GridPosition(1, 1), StructureEdgeDirection.East, out var doorway));
        Assert.Equal(new StructureId("open_doorway"), doorway.StructureId);
    }

    [Fact]
    public void AuthoredMapRejectsDuplicateStructureEdges()
    {
        var ex = Assert.Throws<InvalidDataException>(() => LoadSingleFromJson(
            """
            {
              "id": "duplicate_structure_edge",
              "displayName": "Duplicate Structure Edge",
              "sourceKind": "authored",
              "size": { "width": 3, "height": 3 },
              "startPosition": { "x": 1, "y": 1 },
              "defaultSurface": "grass",
              "structureEdges": [
                { "structureId": "wall", "x": 1, "y": 1, "edge": "north" },
                { "structureId": "wall", "x": 1, "y": 0, "edge": "south" }
              ]
            }
            """,
            CreateStructureCatalog()));

        Assert.Contains("already has a structure", ex.Message);
    }

    [Fact]
    public void AuthoredMapRejectsInvalidObjectPlacementFacing()
    {
        var ex = Assert.Throws<InvalidDataException>(() => LoadSingleFromJson(
            """
            {
              "id": "bad_facing",
              "displayName": "Bad Facing",
              "sourceKind": "authored",
              "size": { "width": 12, "height": 12 },
              "startPosition": { "x": 0, "y": 0 },
              "defaultSurface": "grass",
              "objectPlacements": [
                { "objectId": "abandoned_vehicle", "x": 2, "y": 3, "facing": "diagonal" }
              ]
            }
            """));

        Assert.Contains("unsupported facing", ex.Message);
    }

    [Fact]
    public void AuthoredMapRejectsOutOfBoundsObjectFootprints()
    {
        var ex = Assert.Throws<InvalidDataException>(() => LoadSingleFromJson(
            """
            {
              "id": "bad_object_footprint",
              "displayName": "Bad Object Footprint",
              "sourceKind": "authored",
              "size": { "width": 5, "height": 5 },
              "startPosition": { "x": 0, "y": 0 },
              "defaultSurface": "grass",
              "objectPlacements": [
                { "objectId": "abandoned_vehicle", "x": 2, "y": 2 }
              ]
            }
            """));

        Assert.Contains("must stay inside map bounds", ex.Message);
    }

    [Fact]
    public void AuthoredMapLoadsArrivalAnchors()
    {
        var site = LoadSingleFromJson(
            """
            {
              "id": "arrival_anchor_site",
              "displayName": "Arrival Anchor Site",
              "sourceKind": "authored",
              "size": { "width": 12, "height": 12 },
              "startPosition": { "x": 0, "y": 0 },
              "arrivalAnchors": {
                "vehicle": { "x": 2, "y": 3, "facing": "east" },
                "pushbike": { "x": 8, "y": 8 }
              },
              "defaultSurface": "grass"
            }
            """);

        AssertArrivalAnchor(site, TravelMethodId.Vehicle, new GridPosition(2, 3), WorldObjectFacing.East);
        AssertArrivalAnchor(site, TravelMethodId.Pushbike, new GridPosition(8, 8), WorldObjectFacing.North);
    }

    [Fact]
    public void AuthoredMapRejectsOutOfBoundsArrivalAnchorFootprints()
    {
        var ex = Assert.Throws<InvalidDataException>(() => LoadSingleFromJson(
            """
            {
              "id": "bad_arrival_anchor",
              "displayName": "Bad Arrival Anchor",
              "sourceKind": "authored",
              "size": { "width": 5, "height": 5 },
              "startPosition": { "x": 0, "y": 0 },
              "arrivalAnchors": {
                "vehicle": { "x": 3, "y": 3 }
              },
              "defaultSurface": "grass"
            }
            """));

        Assert.Contains("inside map bounds", ex.Message);
    }

    [Fact]
    public void AuthoredMapRejectsOverlappingArrivalAnchors()
    {
        var ex = Assert.Throws<InvalidDataException>(() => LoadSingleFromJson(
            """
            {
              "id": "overlapping_arrival_anchors",
              "displayName": "Overlapping Arrival Anchors",
              "sourceKind": "authored",
              "size": { "width": 12, "height": 12 },
              "startPosition": { "x": 0, "y": 0 },
              "arrivalAnchors": {
                "vehicle": { "x": 2, "y": 3, "facing": "east" },
                "pushbike": { "x": 5, "y": 4 }
              },
              "defaultSurface": "grass"
            }
            """));

        Assert.Contains("overlaps", ex.Message);
    }

    [Theory]
    [InlineData("recipe", "Recipe map generation is not implemented yet.")]
    [InlineData("chunkedProcedural", "Chunked procedural map generation is not implemented yet.")]
    public void NonAuthoredSourceKindsFailClearly(string sourceKind, string expectedMessage)
    {
        var ex = Assert.Throws<NotSupportedException>(() => LoadSingleFromJson(
            $$"""
            {
              "sourceKind": "{{sourceKind}}"
            }
            """));

        Assert.Equal(expectedMessage, ex.Message);
    }

    private static IReadOnlyList<PrototypeLocalSite> LoadAllSites()
    {
        return new LocalSiteDefinitionLoader().LoadDirectory(
            GetDataPath("local_maps"),
            LoadSurfaceCatalog(),
            LoadWorldObjectCatalog(),
            LoadStructureCatalog(),
            LoadItemCatalog(),
            LoadNpcCatalog()
        );
    }

    private static void AssertArrivalAnchor(
        PrototypeLocalSite site,
        TravelMethodId method,
        GridPosition position,
        WorldObjectFacing facing)
    {
        Assert.NotNull(site.ArrivalAnchors);
        Assert.True(site.ArrivalAnchors!.TryGetValue(method, out var anchor));
        Assert.Equal(method, anchor.TravelMethod);
        Assert.Equal(position, anchor.Position);
        Assert.Equal(facing, anchor.Facing);
        Assert.True(site.Bounds.Contains(anchor.Position));
    }

    private static PrototypeLocalSite LoadSingleFromJson(string json, StructureCatalog? structureCatalog = null)
    {
        var directory = Directory.CreateTempSubdirectory("survival-map-loader-tests-");
        try
        {
            var filePath = Path.Combine(directory.FullName, "map.json");
            File.WriteAllText(filePath, json);
            return new LocalSiteDefinitionLoader().LoadFile(
                filePath,
                LoadSurfaceCatalog(),
                LoadWorldObjectCatalog(),
                structureCatalog ?? new StructureCatalog(),
                LoadItemCatalog(),
                LoadNpcCatalog()
            );
        }
        finally
        {
            Directory.Delete(directory.FullName, recursive: true);
        }
    }

    private static TileSurfaceCatalog LoadSurfaceCatalog()
    {
        return new TileSurfaceDefinitionLoader().LoadDirectory(GetDataPath("surfaces"));
    }

    private static WorldObjectCatalog LoadWorldObjectCatalog()
    {
        return new WorldObjectDefinitionLoader().LoadDirectory(GetDataPath("world_objects"));
    }

    private static StructureCatalog LoadStructureCatalog()
    {
        return new StructureDefinitionLoader().LoadDirectory(GetDataPath("structures"));
    }

    private static StructureCatalog CreateStructureCatalog()
    {
        var catalog = new StructureCatalog();
        catalog.Add(new StructureDefinition(
            new StructureId("wall"),
            "Wall",
            "",
            "Structure",
            "generic",
            "wall",
            blocksMovement: true,
            blocksSight: true));
        catalog.Add(new StructureDefinition(
            new StructureId("open_doorway"),
            "Open doorway",
            "",
            "Doorway",
            "generic",
            "doorway",
            blocksMovement: false,
            connectsAsWall: false));
        return catalog;
    }

    private static ItemCatalog LoadItemCatalog()
    {
        return new ItemDefinitionLoader().LoadDirectory(GetDataPath("items"));
    }

    private static NpcCatalog LoadNpcCatalog()
    {
        return new NpcDefinitionLoader().LoadDirectory(GetDataPath("npcs"));
    }

    private static string GetDataPath(string childDirectory)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var dataPath = Path.Combine(directory.FullName, "data", childDirectory);
            if (Directory.Exists(dataPath))
            {
                return dataPath;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException($"Could not locate data/{childDirectory} from the test output directory.");
    }
}
