using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class WorldObjectTests
{
    [Fact]
    public void WorldObjectDataLoadsTenPrototypeObjects()
    {
        var catalog = LoadWorldObjectCatalog();

        Assert.Equal(10, catalog.Objects.Count);
        Assert.Equal("Wall", catalog.Get(PrototypeWorldObjects.Wall).Name);
        Assert.Equal("Fridge", catalog.Get(PrototypeWorldObjects.Fridge).Name);
        Assert.Equal("Tree", catalog.Get(PrototypeWorldObjects.Tree).Name);
        Assert.Equal("world_object_fridge", catalog.Get(PrototypeWorldObjects.Fridge).SpriteId);
    }

    [Fact]
    public void WorldObjectsCanDescribeMovementBlocking()
    {
        var catalog = LoadWorldObjectCatalog();

        Assert.True(catalog.Get(PrototypeWorldObjects.Wall).BlocksMovement);
        Assert.True(catalog.Get(PrototypeWorldObjects.Fridge).BlocksMovement);
        Assert.False(catalog.Get(PrototypeWorldObjects.Chair).BlocksMovement);
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

    private static WorldObjectCatalog LoadWorldObjectCatalog()
    {
        return new WorldObjectDefinitionLoader().LoadDirectory(GetWorldObjectDataPath());
    }

    private static string GetWorldObjectDataPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var objectDataPath = Path.Combine(directory.FullName, "data", "world_objects");
            if (Directory.Exists(objectDataPath))
            {
                return objectDataPath;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate data/world_objects from the test output directory.");
    }
}
