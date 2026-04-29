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
        Assert.Equal(new InventoryItemSize(5, 2), ak47.InventorySize);
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
        Assert.Equal(new InventoryItemSize(1, 1), bandage.InventorySize);
        Assert.Equal("bandage_clean", bandage.IconId);
        Assert.Equal("item_bandage_clean", bandage.SpriteId);
        Assert.True(bandage.AllowsAction("apply_to_wound"));
    }

    [Fact]
    public void ItemDataLoadsFuelCanDefinition()
    {
        var catalog = LoadItemCatalog();

        var fuelCan = catalog.Get(PrototypeItems.FuelCan);

        Assert.Equal("Fuel can", fuelCan.Name);
        Assert.Equal("Tool", fuelCan.Category);
        Assert.True(fuelCan.HasTag("fuel_can"));
        Assert.Equal(new InventoryItemSize(2, 3), fuelCan.InventorySize);
        Assert.NotNull(fuelCan.FuelContainer);
        Assert.Equal(5.0, fuelCan.FuelContainer!.Capacity);
    }

    [Theory]
    [InlineData("kitchen_knife", "Kitchen knife", "Weapon", "Weapon|Melee|Blade|Knife")]
    [InlineData("hunting_rifle", "Hunting rifle", "Weapon", "Weapon|Gun|Rifle|HuntingRifle")]
    [InlineData("flashlight", "Flashlight", "Tool", "Tool|Light|Flashlight")]
    [InlineData("pot_lid", "Pot lid", "Armor", "Armor|Shield|Improvised|PotLid")]
    [InlineData("baseball_cap", "Baseball cap", "Clothing", "Clothing|Head|Cap|BaseballCap")]
    [InlineData("motorcycle_helmet", "Motorcycle helmet", "Armor", "Armor|Head|Helmet|MotorcycleHelmet")]
    [InlineData("hoodie", "Hoodie", "Clothing", "Clothing|Body|Jacket|Hoodie")]
    [InlineData("leather_jacket", "Leather jacket", "Armor", "Armor|Body|Jacket|LeatherJacket")]
    [InlineData("work_jeans", "Work jeans", "Clothing", "Clothing|Legs|Pants|WorkJeans")]
    [InlineData("cargo_pants", "Cargo pants", "Clothing", "Clothing|Legs|Pants|CargoPants")]
    [InlineData("running_shoes", "Running shoes", "Clothing", "Clothing|Feet|Shoes|RunningShoes")]
    [InlineData("work_boots", "Work boots", "Clothing", "Clothing|Feet|Boots|WorkBoots")]
    [InlineData("school_backpack", "School backpack", "Container", "Container|Back|Backpack|SchoolBackpack")]
    [InlineData("hiking_pack", "Hiking pack", "Container", "Container|Back|Backpack|HikingPack")]
    public void ItemDataLoadsPrototypeEquipmentDefinitions(
        string id,
        string expectedName,
        string expectedCategory,
        string expectedTypePath)
    {
        var catalog = LoadItemCatalog();
        var expectedTypeSegments = expectedTypePath.Split('|');

        var item = catalog.Get(new ItemId(id));

        Assert.Equal(expectedName, item.Name);
        Assert.Equal(expectedCategory, item.Category);
        Assert.True(item.TypePath.IsA(new ItemTypePath(expectedTypeSegments)));
        Assert.True(item.AllowsAction("equip"));
        Assert.Equal(1, item.MaxStackSize);
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

    [Fact]
    public void ItemDefinitionDefaultsToOneByOneInventorySize()
    {
        var item = new ItemDefinition(new ItemId("test_item"), "Test item", "", "Test");

        Assert.Equal(InventoryItemSize.Default, item.InventorySize);
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
