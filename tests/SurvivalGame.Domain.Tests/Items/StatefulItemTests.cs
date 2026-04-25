using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class StatefulItemTests
{
    [Fact]
    public void SameItemTypeCanHaveDifferentLoadedState()
    {
        var state = CreateState();
        var firearms = LoadFirearmCatalog();
        var loadedMagazine = state.StatefulItems.Create(
            PrototypeFirearms.Magazine9mmStandard,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearms
        );
        var emptyMagazine = state.StatefulItems.Create(
            PrototypeFirearms.Magazine9mmStandard,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearms
        );

        loadedMagazine.FeedDevice!.Load(firearms.GetAmmunition(PrototypeFirearms.Ammo9mmStandard), 15);

        Assert.NotEqual(loadedMagazine.Id, emptyMagazine.Id);
        Assert.Equal(15, loadedMagazine.FeedDevice.LoadedCount);
        Assert.True(emptyMagazine.FeedDevice!.IsEmpty);
    }

    [Fact]
    public void RemovedMagazineKeepsLoadedState()
    {
        var state = CreateState();
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var pistol = state.StatefulItems.Create(PrototypeFirearms.Pistol9mm, 1, StatefulItemLocation.PlayerInventory(), firearms);
        var magazine = state.StatefulItems.Create(PrototypeFirearms.Magazine9mmStandard, 1, StatefulItemLocation.PlayerInventory(), firearms);
        magazine.FeedDevice!.Load(firearms.GetAmmunition(PrototypeFirearms.Ammo9mmStandard), 12);

        var insertResult = pipeline.Execute(state, new InsertStatefulFeedDeviceActionRequest(pistol.Id, magazine.Id));
        var removeResult = pipeline.Execute(state, new RemoveStatefulFeedDeviceActionRequest(pistol.Id));

        Assert.True(insertResult.Succeeded);
        Assert.True(removeResult.Succeeded);
        Assert.Equal(StatefulItemLocationKind.PlayerInventory, magazine.Location.Kind);
        Assert.Equal(12, magazine.FeedDevice.LoadedCount);
        Assert.False(pistol.Weapon!.HasInsertedFeedDevice);
    }

    [Fact]
    public void DroppedLoadedMagazineStaysLoadedWhenPickedBackUp()
    {
        var state = CreateState();
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var magazine = state.StatefulItems.Create(PrototypeFirearms.Magazine9mmStandard, 1, StatefulItemLocation.PlayerInventory(), firearms);
        magazine.FeedDevice!.Load(firearms.GetAmmunition(PrototypeFirearms.Ammo9mmHollowPoint), 8);

        var dropResult = pipeline.Execute(state, new DropStatefulItemActionRequest(magazine.Id));
        var pickupResult = pipeline.Execute(state, new PickupStatefulItemActionRequest(magazine.Id));

        Assert.True(dropResult.Succeeded);
        Assert.Equal(0, dropResult.ElapsedTicks);
        Assert.True(pickupResult.Succeeded);
        Assert.Equal(GameActionPipeline.PickupTickCost, pickupResult.ElapsedTicks);
        Assert.Equal(50, state.Time.ElapsedTicks);
        Assert.Equal(StatefulItemLocationKind.PlayerInventory, magazine.Location.Kind);
        Assert.Equal(8, magazine.FeedDevice.LoadedCount);
        Assert.Equal("hollow point", magazine.FeedDevice.LoadedAmmunitionVariant);
    }

    [Fact]
    public void StatefulEquipAndUnequipPreservesWeaponState()
    {
        var state = CreateState();
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var rifle = state.StatefulItems.Create(PrototypeItems.HuntingRifle, 1, StatefulItemLocation.PlayerInventory(), firearms);
        state.Player.Inventory.Add(PrototypeFirearms.Ammo308Standard, 10);

        pipeline.Execute(state, new LoadStatefulWeaponActionRequest(rifle.Id, PrototypeFirearms.Ammo308Standard));
        var equipResult = pipeline.Execute(state, new EquipStatefulItemActionRequest(rifle.Id, EquipmentSlotId.MainHand));
        var unequipResult = pipeline.Execute(state, new UnequipStatefulItemActionRequest(rifle.Id));

        Assert.True(equipResult.Succeeded);
        Assert.True(unequipResult.Succeeded);
        Assert.Equal(StatefulItemLocationKind.PlayerInventory, rifle.Location.Kind);
        Assert.Equal(4, rifle.Weapon!.BuiltInFeed!.LoadedCount);
    }

    [Fact]
    public void ContainerContentsPersistWhenContainerMoves()
    {
        var state = CreateState();
        var backpack = state.StatefulItems.Create(new ItemId("school_backpack"), 1, StatefulItemLocation.PlayerInventory());
        var beans = state.StatefulItems.Create(new ItemId("canned_beans"), 1, StatefulItemLocation.PlayerInventory());

        state.StatefulItems.MoveToContained(beans.Id, backpack.Id);
        state.StatefulItems.MoveToGround(backpack.Id, new GridPosition(2, 2));
        state.StatefulItems.MoveToInventory(backpack.Id);

        Assert.Contains(beans.Id, backpack.Contents);
        Assert.Equal(StatefulItemLocationKind.Contained, beans.Location.Kind);
        Assert.Equal(backpack.Id, beans.Location.ParentItemId);
    }

    [Fact]
    public void DroppingInsertedMagazineFailsWithoutStateMutation()
    {
        var state = CreateState();
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var pistol = state.StatefulItems.Create(PrototypeFirearms.Pistol9mm, 1, StatefulItemLocation.PlayerInventory(), firearms);
        var magazine = state.StatefulItems.Create(PrototypeFirearms.Magazine9mmStandard, 1, StatefulItemLocation.PlayerInventory(), firearms);

        pipeline.Execute(state, new InsertStatefulFeedDeviceActionRequest(pistol.Id, magazine.Id));

        var result = pipeline.Execute(state, new DropStatefulItemActionRequest(magazine.Id));

        Assert.False(result.Succeeded);
        Assert.Equal(StatefulItemLocationKind.Inserted, magazine.Location.Kind);
        Assert.Equal(pistol.Id, magazine.Location.ParentItemId);
        Assert.True(pistol.Weapon!.HasInsertedFeedDevice);
    }

    [Fact]
    public void WrongAmmunitionForStatefulMagazineFailsWithoutStateMutation()
    {
        var state = CreateState();
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var magazine = state.StatefulItems.Create(PrototypeFirearms.Magazine9mmStandard, 1, StatefulItemLocation.PlayerInventory(), firearms);
        state.Player.Inventory.Add(PrototypeFirearms.Ammo12GaugeBuckshot, 5);

        var result = pipeline.Execute(
            state,
            new LoadStatefulFeedDeviceActionRequest(magazine.Id, PrototypeFirearms.Ammo12GaugeBuckshot)
        );

        Assert.False(result.Succeeded);
        Assert.True(magazine.FeedDevice!.IsEmpty);
        Assert.Equal(5, state.Player.Inventory.CountOf(PrototypeFirearms.Ammo12GaugeBuckshot));
    }

    [Fact]
    public void DroppedLoadedMagazineCannotBeUnloadedUntilPickedUp()
    {
        var state = CreateState();
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var magazine = state.StatefulItems.Create(PrototypeFirearms.Magazine9mmStandard, 1, StatefulItemLocation.Ground(new GridPosition(1, 1)), firearms);
        magazine.FeedDevice!.Load(firearms.GetAmmunition(PrototypeFirearms.Ammo9mmStandard), 9);

        var result = pipeline.Execute(state, new UnloadStatefulFeedDeviceActionRequest(magazine.Id));
        var actions = pipeline.GetAvailableActions(state);

        Assert.False(result.Succeeded);
        Assert.Equal(9, magazine.FeedDevice.LoadedCount);
        Assert.Equal(0, state.Player.Inventory.CountOf(PrototypeFirearms.Ammo9mmStandard));
        Assert.DoesNotContain(actions, action => action.Kind == GameActionKind.UnloadStatefulFeedDevice);
    }

    [Fact]
    public void InspectShowsStatefulPracticalDetails()
    {
        var state = CreateState();
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var magazine = state.StatefulItems.Create(PrototypeFirearms.Magazine9mmStandard, 1, StatefulItemLocation.PlayerInventory(), firearms);
        magazine.FeedDevice!.Load(firearms.GetAmmunition(PrototypeFirearms.Ammo9mmStandard), 9);

        var result = pipeline.Execute(state, new InspectStatefulItemActionRequest(magazine.Id));

        Assert.True(result.Succeeded);
        Assert.Contains(result.Messages, message => message.Contains("9/15 standard"));
        Assert.Contains(result.Messages, message => message.Contains("Location: inventory"));
    }

    private static GameActionPipeline CreatePipeline()
    {
        return new GameActionPipeline(LoadItemCatalog(), firearmCatalog: LoadFirearmCatalog());
    }

    private static PrototypeGameState CreateState()
    {
        return new PrototypeGameState(
            new GridBounds(5, 5),
            new TileItemMap(),
            new TileSurfaceMap(new GridBounds(5, 5), PrototypeSurfaces.Concrete),
            new TileObjectMap(),
            new GridPosition(2, 2)
        );
    }

    private static ItemCatalog LoadItemCatalog()
    {
        return new ItemDefinitionLoader().LoadDirectory(GetDataPath("items"));
    }

    private static FirearmCatalog LoadFirearmCatalog()
    {
        return new FirearmDefinitionLoader().LoadDirectory(GetDataPath("firearms"));
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
