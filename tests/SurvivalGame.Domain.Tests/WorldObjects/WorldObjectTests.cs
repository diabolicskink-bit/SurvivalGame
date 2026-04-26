using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class WorldObjectTests
{
    private static readonly string[] EdgeOnlyStructureIds =
    [
        "farmhouse_wall",
        "shed_wall",
        "open_wooden_door",
        "interior_doorway",
        "screen_door",
        "broken_window",
        "boarded_window",
        "wire_fence",
        "timber_fence",
        "broken_fence_gap",
        "open_farm_gate",
    ];

    [Fact]
    public void WorldObjectIdTrimsValueAndFormatsAsRawId()
    {
        var id = new WorldObjectId("  fuel_pump  ");

        Assert.Equal("fuel_pump", id.Value);
        Assert.Equal("fuel_pump", id.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void WorldObjectIdRejectsEmptyValues(string value)
    {
        Assert.Throws<ArgumentException>(() => new WorldObjectId(value));
    }

    [Fact]
    public void WorldObjectDataLoadsPrototypeObjects()
    {
        var catalog = LoadWorldObjectCatalog();

        Assert.Equal("Wall", catalog.Get(PrototypeWorldObjects.Wall).Name);
        Assert.Null(catalog.Get(PrototypeWorldObjects.Wall).SpriteRender);
        Assert.Equal("Fridge", catalog.Get(PrototypeWorldObjects.Fridge).Name);
        Assert.Equal("Tree", catalog.Get(PrototypeWorldObjects.Tree).Name);
        Assert.Equal("Single bed", catalog.Get(PrototypeWorldObjects.SingleBed).Name);
        Assert.Equal("Fuel pump", catalog.Get(PrototypeWorldObjects.FuelPump).Name);
        Assert.Equal("Glass door", catalog.Get(PrototypeWorldObjects.GlassDoor).Name);
        Assert.Equal("Verandah post", catalog.Get(new WorldObjectId("verandah_post")).Name);
        Assert.Equal("Water tank", catalog.Get(new WorldObjectId("water_tank")).Name);
        Assert.Equal("Tractor wreck", catalog.Get(new WorldObjectId("tractor_wreck")).Name);
        Assert.Equal("Open farm gate", LoadStructureCatalog().Get(new StructureId("open_farm_gate")).Name);
        Assert.Equal("world_object_fridge", catalog.Get(PrototypeWorldObjects.Fridge).SpriteId);
        Assert.NotNull(catalog.Get(PrototypeWorldObjects.Fridge).Container);
        Assert.Equal("fridge_basic", catalog.Get(PrototypeWorldObjects.Fridge).Container!.ProfileId);
        Assert.NotNull(catalog.Get(PrototypeWorldObjects.StorageCrate).Container);
        Assert.NotNull(catalog.Get(PrototypeWorldObjects.StoreShelf).Container);
        Assert.NotNull(catalog.Get(PrototypeWorldObjects.TrashBin).Container);
        Assert.Equal("world_object_single_bed", catalog.Get(PrototypeWorldObjects.SingleBed).SpriteId);
        Assert.Equal("world_object_tractor_wreck", catalog.Get(new WorldObjectId("tractor_wreck")).SpriteId);
        Assert.Equal("world_object_farm_trailer", catalog.Get(new WorldObjectId("farm_trailer")).SpriteId);
        Assert.Equal(WorldObjectFootprint.SingleTile, catalog.Get(PrototypeWorldObjects.Wall).Footprint);
        Assert.Equal(new WorldObjectFootprint(1, 2), catalog.Get(PrototypeWorldObjects.SingleBed).Footprint);
        Assert.Equal(new WorldObjectFootprint(2, 4), catalog.Get(PrototypeWorldObjects.AbandonedVehicle).Footprint);
        Assert.Equal(new WorldObjectFootprint(3, 3), catalog.Get(new WorldObjectId("water_tank")).Footprint);
        Assert.Equal(new WorldObjectFootprint(2, 3), catalog.Get(new WorldObjectId("tractor_wreck")).Footprint);
        Assert.Equal(new WorldObjectFootprint(2, 3), catalog.Get(new WorldObjectId("farm_trailer")).Footprint);
        Assert.Equal(new WorldObjectFootprint(3, 1), catalog.Get(new WorldObjectId("trough")).Footprint);
        Assert.NotNull(catalog.Get(PrototypeWorldObjects.Tree).SpriteRender);
        Assert.Equal(1.75f, catalog.Get(PrototypeWorldObjects.Tree).SpriteRender!.WidthTiles, precision: 3);
        Assert.Equal(-0.35f, catalog.Get(PrototypeWorldObjects.Tree).SpriteRender!.OffsetYTiles, precision: 3);

        foreach (var structureId in EdgeOnlyStructureIds)
        {
            Assert.False(catalog.TryGet(new WorldObjectId(structureId), out _));
        }
    }

    [Fact]
    public void WorldObjectsCanDescribeMovementBlocking()
    {
        var catalog = LoadWorldObjectCatalog();

        Assert.True(catalog.Get(PrototypeWorldObjects.Wall).BlocksMovement);
        Assert.True(catalog.Get(PrototypeWorldObjects.Fridge).BlocksMovement);
        Assert.True(catalog.Get(PrototypeWorldObjects.FuelPump).BlocksMovement);
        Assert.True(catalog.Get(PrototypeWorldObjects.CheckoutCounter).BlocksMovement);
        Assert.True(catalog.Get(PrototypeWorldObjects.StoreShelf).BlocksMovement);
        Assert.True(catalog.Get(PrototypeWorldObjects.AbandonedVehicle).BlocksMovement);
        Assert.True(catalog.Get(new WorldObjectId("water_tank")).BlocksMovement);
        Assert.True(catalog.Get(new WorldObjectId("tractor_wreck")).BlocksMovement);
        Assert.False(catalog.Get(PrototypeWorldObjects.GlassDoor).BlocksMovement);
        Assert.False(catalog.Get(PrototypeWorldObjects.Chair).BlocksMovement);

        var structures = LoadStructureCatalog();
        Assert.True(structures.Get(new StructureId("wire_fence")).BlocksMovement);
        Assert.False(structures.Get(new StructureId("open_wooden_door")).BlocksMovement);
        Assert.False(structures.Get(new StructureId("interior_doorway")).BlocksMovement);
        Assert.False(structures.Get(new StructureId("open_farm_gate")).BlocksMovement);
        Assert.False(structures.Get(new StructureId("broken_fence_gap")).BlocksMovement);
    }

    [Fact]
    public void TileObjectMapTracksOneObjectPerTile()
    {
        var objectMap = new TileObjectMap();
        var position = new GridPosition(2, 3);

        objectMap.Place(position, PrototypeWorldObjects.Tree);

        Assert.True(objectMap.TryGetObjectAt(position, out var objectId));
        Assert.Equal(PrototypeWorldObjects.Tree, objectId);
        Assert.Throws<InvalidOperationException>(() => objectMap.Place(position, PrototypeWorldObjects.Boulder));
    }

    [Fact]
    public void TileObjectMapIndexesEveryTileInFootprint()
    {
        var objectMap = new TileObjectMap();
        var anchor = new GridPosition(2, 3);

        objectMap.Place(
            anchor,
            PrototypeWorldObjects.AbandonedVehicle,
            WorldObjectFacing.East,
            new WorldObjectFootprint(2, 4),
            new GridBounds(12, 12)
        );

        var placement = Assert.Single(objectMap.AllObjects);
        Assert.Equal(anchor, placement.Position);
        Assert.Equal(new WorldObjectInstanceId("abandoned_vehicle@2,3"), placement.InstanceId);
        Assert.Equal(WorldObjectFacing.East, placement.Facing);
        Assert.Equal(new WorldObjectFootprint(2, 4), placement.Footprint);
        Assert.Equal(new WorldObjectFootprint(4, 2), placement.EffectiveFootprint);

        Assert.True(objectMap.TryGetObjectAt(new GridPosition(2, 3), out var anchorObject));
        Assert.Equal(PrototypeWorldObjects.AbandonedVehicle, anchorObject);
        Assert.True(objectMap.TryGetObjectAt(new GridPosition(5, 4), out var farCornerObject));
        Assert.Equal(PrototypeWorldObjects.AbandonedVehicle, farCornerObject);
        Assert.False(objectMap.TryGetObjectAt(new GridPosition(6, 4), out _));
        Assert.False(objectMap.TryGetObjectAt(new GridPosition(5, 5), out _));
    }

    [Fact]
    public void TileObjectMapCanTrackExplicitObjectInstanceIds()
    {
        var objectMap = new TileObjectMap();
        var instanceId = new WorldObjectInstanceId("crate_01");

        objectMap.Place(
            new GridPosition(2, 3),
            PrototypeWorldObjects.StorageCrate,
            WorldObjectFacing.North,
            WorldObjectFootprint.SingleTile,
            instanceId: instanceId
        );

        Assert.True(objectMap.TryGetPlacement(instanceId, out var placement));
        Assert.Equal(instanceId, placement.InstanceId);
        Assert.Equal(PrototypeWorldObjects.StorageCrate, placement.ObjectId);
        Assert.Throws<InvalidOperationException>(() => objectMap.Place(
            new GridPosition(3, 3),
            PrototypeWorldObjects.StorageCrate,
            WorldObjectFacing.North,
            WorldObjectFootprint.SingleTile,
            instanceId: instanceId
        ));
    }

    [Fact]
    public void TileObjectMapRejectsOverlappingFootprints()
    {
        var objectMap = new TileObjectMap();
        objectMap.Place(
            new GridPosition(2, 3),
            PrototypeWorldObjects.AbandonedVehicle,
            WorldObjectFacing.East,
            new WorldObjectFootprint(2, 4),
            new GridBounds(12, 12)
        );

        var ex = Assert.Throws<InvalidOperationException>(() => objectMap.Place(
            new GridPosition(5, 4),
            PrototypeWorldObjects.Boulder,
            WorldObjectFacing.North,
            WorldObjectFootprint.SingleTile,
            new GridBounds(12, 12)
        ));

        Assert.Contains("already has a world object", ex.Message);
    }

    [Fact]
    public void TileObjectMapRejectsOutOfBoundsFootprints()
    {
        var objectMap = new TileObjectMap();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => objectMap.Place(
            new GridPosition(7, 7),
            PrototypeWorldObjects.AbandonedVehicle,
            WorldObjectFacing.North,
            new WorldObjectFootprint(2, 4),
            new GridBounds(10, 10)
        ));

        Assert.Contains("must stay inside map bounds", ex.Message);
    }

    [Fact]
    public void WorldObjectDefinitionSpriteRenderRejectsNonPositiveSize()
    {
        var directoryPath = CreateTemporaryDirectory();
        var filePath = Path.Combine(directoryPath, "objects.json");
        File.WriteAllText(filePath, """
        [
          {
            "id": "bad_sprite",
            "name": "Bad Sprite",
            "category": "Fixture",
            "spriteRender": {
              "widthTiles": 1,
              "heightTiles": 0
            }
          }
        ]
        """);

        Assert.Throws<ArgumentOutOfRangeException>(() => new WorldObjectDefinitionLoader().LoadDirectory(directoryPath));
    }

    [Fact]
    public void WorldObjectDefinitionFootprintRejectsNonPositiveSize()
    {
        var directoryPath = CreateTemporaryDirectory();
        var filePath = Path.Combine(directoryPath, "objects.json");
        File.WriteAllText(filePath, """
        [
          {
            "id": "bad_footprint",
            "name": "Bad Footprint",
            "category": "Fixture",
            "footprint": {
              "width": 0,
              "height": 1
            }
          }
        ]
        """);

        Assert.Throws<ArgumentOutOfRangeException>(() => new WorldObjectDefinitionLoader().LoadDirectory(directoryPath));
    }

    [Fact]
    public void SpriteRenderProfileDefaultsOffsetsToZero()
    {
        var render = new SpriteRenderProfile(1.25f, 1.5f);

        Assert.Equal(1.25f, render.WidthTiles, precision: 3);
        Assert.Equal(1.5f, render.HeightTiles, precision: 3);
        Assert.Equal(0f, render.OffsetXTiles, precision: 3);
        Assert.Equal(0f, render.OffsetYTiles, precision: 3);
        Assert.Equal(0f, render.SortOffsetYTiles, precision: 3);
    }

    private static WorldObjectCatalog LoadWorldObjectCatalog()
    {
        return new WorldObjectDefinitionLoader().LoadDirectory(GetWorldObjectDataPath());
    }

    private static StructureCatalog LoadStructureCatalog()
    {
        return new StructureDefinitionLoader().LoadDirectory(GetDataPath("structures"));
    }

    private static string CreateTemporaryDirectory()
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), "SurvivalGameWorldObjectTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);
        return directoryPath;
    }

    private static string GetWorldObjectDataPath()
    {
        return GetDataPath("world_objects");
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
