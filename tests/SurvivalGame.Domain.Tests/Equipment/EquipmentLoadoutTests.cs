using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class EquipmentLoadoutTests
{
    [Fact]
    public void DefaultPlayerEquipmentContainsExpectedSlots()
    {
        var player = new PlayerState();
        var slotIds = player.Equipment.Slots.Select(slot => slot.Id).ToHashSet();

        Assert.Equal(7, slotIds.Count);
        Assert.Contains(EquipmentSlotId.MainHand, slotIds);
        Assert.Contains(EquipmentSlotId.OffHand, slotIds);
        Assert.Contains(EquipmentSlotId.Head, slotIds);
        Assert.Contains(EquipmentSlotId.Body, slotIds);
        Assert.Contains(EquipmentSlotId.Legs, slotIds);
        Assert.Contains(EquipmentSlotId.Feet, slotIds);
        Assert.Contains(EquipmentSlotId.Back, slotIds);
    }

    [Fact]
    public void EmptySlotsReturnEmpty()
    {
        var loadout = EquipmentLoadout.CreateDefault();

        Assert.True(loadout.IsEmpty(EquipmentSlotId.MainHand));
        Assert.False(loadout.TryGetEquippedItem(EquipmentSlotId.MainHand, out _));
    }

    [Fact]
    public void SlotCanBeOccupied()
    {
        var loadout = EquipmentLoadout.CreateDefault();
        var item = new EquippedItemRef(PrototypeItems.Ak47, new ItemTypePath("Weapon", "Gun", "Rifle"));

        loadout.OccupySlot(EquipmentSlotId.MainHand, item);

        Assert.False(loadout.IsEmpty(EquipmentSlotId.MainHand));
    }

    [Fact]
    public void OccupiedSlotReportsEquippedItem()
    {
        var loadout = EquipmentLoadout.CreateDefault();
        var item = new EquippedItemRef(PrototypeItems.Ak47, new ItemTypePath("Weapon", "Gun", "Rifle"));

        loadout.OccupySlot(EquipmentSlotId.MainHand, item);

        Assert.True(loadout.TryGetEquippedItem(EquipmentSlotId.MainHand, out var equippedItem));
        Assert.Equal(item, equippedItem);
    }

    [Fact]
    public void SlotDefinitionsCanCheckAcceptedItemTypePaths()
    {
        var catalog = EquipmentSlotCatalog.CreateDefault();
        var mainHand = catalog.Get(EquipmentSlotId.MainHand);
        var head = catalog.Get(EquipmentSlotId.Head);

        Assert.True(mainHand.Accepts(new ItemTypePath("Weapon", "Gun", "Rifle")));
        Assert.True(head.Accepts(new ItemTypePath("Clothing", "Head", "Helmet")));
        Assert.True(head.Accepts(new ItemTypePath("Armor", "Head", "Helmet")));
        Assert.False(head.Accepts(new ItemTypePath("Clothing", "Feet", "Boots")));
    }

    [Fact]
    public void SlotDefinitionsAcceptPrototypeEquipmentItemTypes()
    {
        var catalog = EquipmentSlotCatalog.CreateDefault();

        Assert.True(catalog.Get(EquipmentSlotId.MainHand).Accepts(new ItemTypePath("Weapon", "Melee", "Blade", "Knife")));
        Assert.True(catalog.Get(EquipmentSlotId.MainHand).Accepts(new ItemTypePath("Weapon", "Gun", "Rifle", "HuntingRifle")));
        Assert.True(catalog.Get(EquipmentSlotId.OffHand).Accepts(new ItemTypePath("Tool", "Light", "Flashlight")));
        Assert.True(catalog.Get(EquipmentSlotId.OffHand).Accepts(new ItemTypePath("Armor", "Shield", "Improvised", "PotLid")));
        Assert.True(catalog.Get(EquipmentSlotId.Head).Accepts(new ItemTypePath("Clothing", "Head", "Cap", "BaseballCap")));
        Assert.True(catalog.Get(EquipmentSlotId.Body).Accepts(new ItemTypePath("Armor", "Body", "Jacket", "LeatherJacket")));
        Assert.True(catalog.Get(EquipmentSlotId.Legs).Accepts(new ItemTypePath("Clothing", "Legs", "Pants", "CargoPants")));
        Assert.True(catalog.Get(EquipmentSlotId.Feet).Accepts(new ItemTypePath("Clothing", "Feet", "Boots", "WorkBoots")));
        Assert.True(catalog.Get(EquipmentSlotId.Back).Accepts(new ItemTypePath("Container", "Back", "Backpack", "HikingPack")));
    }

    [Fact]
    public void InvalidItemTypePathsAreRejectedByValidation()
    {
        var loadout = EquipmentLoadout.CreateDefault();
        var food = new EquippedItemRef(new ItemId("canned_beans"), new ItemTypePath("Food"));

        Assert.Throws<InvalidOperationException>(() => loadout.OccupySlot(EquipmentSlotId.MainHand, food));
    }
}
