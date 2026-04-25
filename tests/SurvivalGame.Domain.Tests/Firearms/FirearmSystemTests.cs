using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class FirearmSystemTests
{
    [Fact]
    public void WeaponsAcceptCorrectAmmunitionAndRejectWrongAmmunition()
    {
        var catalog = LoadFirearmCatalog();
        var pistol = catalog.GetWeapon(PrototypeFirearms.Pistol9mm);
        var standard9mm = catalog.GetAmmunition(PrototypeFirearms.Ammo9mmStandard);
        var hollowPoint9mm = catalog.GetAmmunition(PrototypeFirearms.Ammo9mmHollowPoint);
        var buckshot = catalog.GetAmmunition(PrototypeFirearms.Ammo12GaugeBuckshot);

        Assert.True(pistol.AcceptsAmmunition(standard9mm));
        Assert.True(pistol.AcceptsAmmunition(hollowPoint9mm));
        Assert.False(pistol.AcceptsAmmunition(buckshot));
    }

    [Fact]
    public void AmmunitionTracksSizeAndVariant()
    {
        var catalog = LoadFirearmCatalog();
        var standard9mm = catalog.GetAmmunition(PrototypeFirearms.Ammo9mmStandard);
        var hollowPoint9mm = catalog.GetAmmunition(PrototypeFirearms.Ammo9mmHollowPoint);
        var buckshot = catalog.GetAmmunition(PrototypeFirearms.Ammo12GaugeBuckshot);
        var slug = catalog.GetAmmunition(PrototypeFirearms.Ammo12GaugeSlug);

        Assert.Equal(standard9mm.Size, hollowPoint9mm.Size);
        Assert.NotEqual(standard9mm.Variant, hollowPoint9mm.Variant);
        Assert.Equal(buckshot.Size, slug.Size);
        Assert.NotEqual(buckshot.Variant, slug.Variant);
    }

    [Fact]
    public void FeedDeviceAcceptsCorrectAmmunitionAndRejectsWrongAmmunition()
    {
        var catalog = LoadFirearmCatalog();
        var magazine = catalog.GetFeedDevice(PrototypeFirearms.Magazine9mmStandard).CreateState();
        var standard9mm = catalog.GetAmmunition(PrototypeFirearms.Ammo9mmStandard);
        var buckshot = catalog.GetAmmunition(PrototypeFirearms.Ammo12GaugeBuckshot);

        Assert.True(magazine.CanAccept(standard9mm));
        Assert.False(magazine.CanAccept(buckshot));
        Assert.Throws<InvalidOperationException>(() => magazine.Load(buckshot, 1));
    }

    [Fact]
    public void FeedDeviceCapacityCannotBeExceededAndCanBeUnloaded()
    {
        var catalog = LoadFirearmCatalog();
        var magazine = catalog.GetFeedDevice(PrototypeFirearms.Magazine9mmStandard).CreateState();
        var standard9mm = catalog.GetAmmunition(PrototypeFirearms.Ammo9mmStandard);

        var loaded = magazine.Load(standard9mm, 50);
        var unloaded = magazine.UnloadAll();

        Assert.Equal(15, loaded);
        Assert.Equal(15, unloaded?.Quantity);
        Assert.True(magazine.IsEmpty);
    }

    [Fact]
    public void FeedDeviceRejectsMixedAmmunitionInFirstVersion()
    {
        var catalog = LoadFirearmCatalog();
        var magazine = catalog.GetFeedDevice(PrototypeFirearms.Magazine9mmStandard).CreateState();
        var standard9mm = catalog.GetAmmunition(PrototypeFirearms.Ammo9mmStandard);
        var hollowPoint9mm = catalog.GetAmmunition(PrototypeFirearms.Ammo9mmHollowPoint);

        magazine.Load(standard9mm, 5);

        Assert.False(magazine.CanAccept(hollowPoint9mm));
        Assert.Throws<InvalidOperationException>(() => magazine.Load(hollowPoint9mm, 1));
    }

    [Fact]
    public void WeaponsAcceptCompatibleMagazineAlternatives()
    {
        var catalog = LoadFirearmCatalog();
        var pistol = catalog.GetWeapon(PrototypeFirearms.Pistol9mm);
        var standardMagazine = catalog.GetFeedDevice(PrototypeFirearms.Magazine9mmStandard);
        var extendedMagazine = catalog.GetFeedDevice(PrototypeFirearms.Magazine9mmExtended);

        Assert.True(pistol.CanUseFeedDevice(standardMagazine));
        Assert.True(pistol.CanUseFeedDevice(extendedMagazine));
    }

    [Fact]
    public void WeaponRejectsWrongCalibreMagazineAndWrongFamilyMagazine()
    {
        var catalog = LoadFirearmCatalog();
        var pistol = catalog.GetWeapon(PrototypeFirearms.Pistol9mm);
        var akMagazine = catalog.GetFeedDevice(PrototypeFirearms.MagazineAk30Round);
        var wrongFamily9mmMagazine = new FeedDeviceDefinition(
            new ItemId("other_9mm_magazine"),
            "Other 9mm magazine",
            FeedDeviceKind.DetachableMagazine,
            PrototypeFirearms.NineMillimeter,
            12,
            new[] { "other_9mm_pistol" }
        );

        Assert.False(pistol.CanUseFeedDevice(akMagazine));
        Assert.False(pistol.CanUseFeedDevice(wrongFamily9mmMagazine));
    }

    [Fact]
    public void LoadingFeedDeviceReducesInventoryAndUnloadingRestoresInventory()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeFirearms.Magazine9mmStandard);
        state.Player.Inventory.Add(PrototypeFirearms.Ammo9mmStandard, 20);

        var loadResult = pipeline.Execute(
            state,
            new LoadFeedDeviceActionRequest(PrototypeFirearms.Magazine9mmStandard, PrototypeFirearms.Ammo9mmStandard)
        );

        Assert.True(loadResult.Succeeded);
        Assert.False(loadResult.AdvancedTurn);
        Assert.Equal(5, state.Player.Inventory.CountOf(PrototypeFirearms.Ammo9mmStandard));

        var unloadResult = pipeline.Execute(
            state,
            new UnloadFeedDeviceActionRequest(PrototypeFirearms.Magazine9mmStandard)
        );

        Assert.True(unloadResult.Succeeded);
        Assert.Equal(20, state.Player.Inventory.CountOf(PrototypeFirearms.Ammo9mmStandard));
    }

    [Fact]
    public void LoadingWrongAmmunitionFailsWithoutMutatingInventoryOrState()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeFirearms.Magazine9mmStandard);
        state.Player.Inventory.Add(PrototypeFirearms.Ammo12GaugeBuckshot, 10);

        var result = pipeline.Execute(
            state,
            new LoadFeedDeviceActionRequest(PrototypeFirearms.Magazine9mmStandard, PrototypeFirearms.Ammo12GaugeBuckshot)
        );

        Assert.False(result.Succeeded);
        Assert.False(result.AdvancedTurn);
        Assert.Equal(10, state.Player.Inventory.CountOf(PrototypeFirearms.Ammo12GaugeBuckshot));
        Assert.False(state.Player.Firearms.TryGetFeedDevice(PrototypeFirearms.Magazine9mmStandard, out _));
        Assert.Contains("Cannot load 12 gauge buckshot shells into 9mm standard pistol magazine.", result.Messages);
    }

    [Fact]
    public void AvailableActionQueryDoesNotCreateRuntimeFirearmState()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeFirearms.Pistol9mm);
        state.Player.Inventory.Add(PrototypeFirearms.Magazine9mmStandard);
        state.Player.Inventory.Add(PrototypeFirearms.Ammo9mmStandard, 20);

        var actions = pipeline.GetAvailableActions(state);

        Assert.Contains(actions, action => action.Kind == GameActionKind.LoadFeedDevice);
        Assert.Empty(state.Player.Firearms.Weapons);
        Assert.Empty(state.Player.Firearms.FeedDevices);
    }

    [Fact]
    public void CompatibleMagazineInsertsIntoWeaponAndCanBeRemoved()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeFirearms.Pistol9mm);
        state.Player.Inventory.Add(PrototypeFirearms.Magazine9mmStandard);

        var insertResult = pipeline.Execute(
            state,
            new InsertFeedDeviceActionRequest(PrototypeFirearms.Pistol9mm, PrototypeFirearms.Magazine9mmStandard)
        );

        Assert.True(insertResult.Succeeded);
        Assert.Equal(0, state.Player.Inventory.CountOf(PrototypeFirearms.Magazine9mmStandard));

        var removeResult = pipeline.Execute(
            state,
            new RemoveFeedDeviceActionRequest(PrototypeFirearms.Pistol9mm)
        );

        Assert.True(removeResult.Succeeded);
        Assert.Equal(1, state.Player.Inventory.CountOf(PrototypeFirearms.Magazine9mmStandard));
    }

    [Fact]
    public void IncompatibleMagazineInsertIsRejectedWithoutInventoryMutation()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeFirearms.Pistol9mm);
        state.Player.Inventory.Add(PrototypeFirearms.MagazineAk30Round);

        var result = pipeline.Execute(
            state,
            new InsertFeedDeviceActionRequest(PrototypeFirearms.Pistol9mm, PrototypeFirearms.MagazineAk30Round)
        );

        Assert.False(result.Succeeded);
        Assert.False(result.AdvancedTurn);
        Assert.Equal(1, state.Player.Inventory.CountOf(PrototypeFirearms.MagazineAk30Round));
        Assert.Contains("This magazine does not fit that weapon.", result.Messages);
    }

    [Fact]
    public void DirectFeedWeaponLoadsAmmunitionAndReportsLoadedState()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeItems.HuntingRifle);
        state.Player.Inventory.Add(PrototypeFirearms.Ammo308Standard, 10);

        var result = pipeline.Execute(
            state,
            new LoadWeaponActionRequest(PrototypeItems.HuntingRifle, PrototypeFirearms.Ammo308Standard)
        );

        Assert.True(result.Succeeded);
        Assert.Equal(6, state.Player.Inventory.CountOf(PrototypeFirearms.Ammo308Standard));
        var weapon = LoadFirearmCatalog().GetWeapon(PrototypeItems.HuntingRifle);
        var service = new FirearmActionService(LoadFirearmCatalog());
        Assert.True(service.IsLoaded(state.Player.Firearms, weapon));
        Assert.Equal(4, service.GetAvailableRounds(state.Player.Firearms, weapon));
    }

    [Fact]
    public void DetachableMagazineWeaponRejectsDirectAmmunitionLoading()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeFirearms.Pistol9mm);
        state.Player.Inventory.Add(PrototypeFirearms.Ammo9mmStandard, 10);

        var result = pipeline.Execute(
            state,
            new LoadWeaponActionRequest(PrototypeFirearms.Pistol9mm, PrototypeFirearms.Ammo9mmStandard)
        );

        Assert.False(result.Succeeded);
        Assert.False(result.AdvancedTurn);
        Assert.Equal(10, state.Player.Inventory.CountOf(PrototypeFirearms.Ammo9mmStandard));
        Assert.Contains("9mm pistol must use a compatible feed device.", result.Messages);
    }

    [Fact]
    public void FailedDirectWeaponLoadDoesNotCreateWeaponState()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeItems.HuntingRifle);

        var result = pipeline.Execute(
            state,
            new LoadWeaponActionRequest(PrototypeItems.HuntingRifle, PrototypeFirearms.Ammo308Standard)
        );

        Assert.False(result.Succeeded);
        Assert.False(result.AdvancedTurn);
        Assert.Empty(state.Player.Firearms.Weapons);
    }

    [Fact]
    public void TestFireConsumesOneLoadedRound()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeFirearms.Pistol9mm);
        state.Player.Inventory.Add(PrototypeFirearms.Magazine9mmStandard);
        state.Player.Inventory.Add(PrototypeFirearms.Ammo9mmStandard, 20);

        pipeline.Execute(state, new LoadFeedDeviceActionRequest(PrototypeFirearms.Magazine9mmStandard, PrototypeFirearms.Ammo9mmStandard));
        pipeline.Execute(state, new InsertFeedDeviceActionRequest(PrototypeFirearms.Pistol9mm, PrototypeFirearms.Magazine9mmStandard));

        var result = pipeline.Execute(state, new TestFireActionRequest(PrototypeFirearms.Pistol9mm));

        Assert.True(result.Succeeded);
        Assert.False(result.AdvancedTurn);
        Assert.True(state.Player.Firearms.TryGetFeedDevice(PrototypeFirearms.Magazine9mmStandard, out var magazine));
        Assert.Equal(14, magazine.LoadedCount);
    }

    [Fact]
    public void TestFireEmptyWeaponGivesClearFeedbackWithoutMutation()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeFirearms.Pistol9mm);

        var result = pipeline.Execute(state, new TestFireActionRequest(PrototypeFirearms.Pistol9mm));

        Assert.False(result.Succeeded);
        Assert.False(result.AdvancedTurn);
        Assert.Contains("Weapon is empty.", result.Messages);
        Assert.Empty(state.Player.Firearms.Weapons);
    }

    private static GameActionPipeline CreatePipeline()
    {
        return new GameActionPipeline(new ItemCatalog(), firearmCatalog: LoadFirearmCatalog());
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

    private static FirearmCatalog LoadFirearmCatalog()
    {
        return new FirearmDefinitionLoader().LoadDirectory(GetFirearmDataPath());
    }

    private static string GetFirearmDataPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var firearmDataPath = Path.Combine(directory.FullName, "data", "firearms");
            if (Directory.Exists(firearmDataPath))
            {
                return firearmDataPath;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate data/firearms from the test output directory.");
    }
}
