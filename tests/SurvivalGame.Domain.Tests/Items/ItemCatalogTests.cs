using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class ItemCatalogTests
{
    [Fact]
    public void ItemTypePathSupportsNestedSubtypeChecks()
    {
        var ak47 = new ItemTypePath("Weapon", "Gun", "Rifle", "AK47");

        Assert.True(ak47.IsA(new ItemTypePath("Weapon")));
        Assert.True(ak47.IsA(new ItemTypePath("Weapon", "Gun")));
        Assert.True(ak47.IsA(new ItemTypePath("Weapon", "Gun", "Rifle")));
        Assert.True(ak47.IsA(new ItemTypePath("Weapon", "Gun", "Rifle", "AK47")));
        Assert.False(ak47.IsA(new ItemTypePath("Consumable")));
    }

    [Fact]
    public void ItemTypePathComparisonIsCaseInsensitive()
    {
        var rifle = new ItemTypePath("Weapon", "Gun", "Rifle");

        Assert.True(rifle.IsA(new ItemTypePath("weapon", "gun")));
    }

    [Fact]
    public void ItemDataLoadsNestedAk47Definition()
    {
        var catalog = LoadItemCatalog();

        var ak47 = catalog.Get(PrototypeItems.Ak47);

        Assert.Equal("AK-47", ak47.Name);
        Assert.Equal(4.3f, ak47.Weight);
        Assert.Equal(1, ak47.MaxStackSize);
        Assert.True(ak47.TypePath.IsA(PrototypeItems.Weapon));
        Assert.True(ak47.TypePath.IsA(PrototypeItems.Gun));
        Assert.True(ak47.TypePath.IsA(PrototypeItems.Rifle));
        Assert.True(ak47.AllowsAction("equip"));
    }

    [Fact]
    public void ItemDataLoadsStableDefinitionProperties()
    {
        var catalog = LoadItemCatalog();

        var bandage = catalog.Get(new ItemId("clean_bandage"));

        Assert.Equal("Clean bandage", bandage.Name);
        Assert.Equal("Medical", bandage.Category);
        Assert.True(bandage.HasTag("wound_treatment"));
        Assert.Equal(10, bandage.MaxStackSize);
        Assert.Equal("bandage_clean", bandage.IconId);
        Assert.Equal("item_bandage_clean", bandage.SpriteId);
        Assert.True(bandage.AllowsAction("apply_to_wound"));
    }

    [Fact]
    public void CatalogRejectsDuplicateItemIds()
    {
        var catalog = new ItemCatalog();
        var itemId = new ItemId("stone");
        var item = new ItemDefinition(itemId, "Stone", "", "Material");

        catalog.Add(item);

        Assert.Throws<InvalidOperationException>(() => catalog.Add(item));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ItemTypePathRejectsEmptySegments(string segment)
    {
        Assert.Throws<ArgumentException>(() => new ItemTypePath("Weapon", segment));
    }

    private static ItemCatalog LoadItemCatalog()
    {
        return new ItemDefinitionLoader().LoadDirectory(GetItemDataPath());
    }

    private static string GetItemDataPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var itemDataPath = Path.Combine(directory.FullName, "data", "items");
            if (Directory.Exists(itemDataPath))
            {
                return itemDataPath;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate data/items from the test output directory.");
    }
}
