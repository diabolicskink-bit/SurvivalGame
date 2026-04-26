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
    public void WeaponsLoadPrototypeTileRanges()
    {
        var catalog = LoadFirearmCatalog();

        AssertWeaponRange(catalog.GetWeapon(PrototypeFirearms.Pistol9mm), effective: 8, maximum: 20);
        AssertWeaponRange(catalog.GetWeapon(PrototypeItems.Ak47), effective: 24, maximum: 60);
        AssertWeaponRange(catalog.GetWeapon(PrototypeItems.HuntingRifle), effective: 32, maximum: 80);
        AssertWeaponRange(catalog.GetWeapon(PrototypeFirearms.Shotgun12Gauge), effective: 6, maximum: 18);
        AssertWeaponRange(catalog.GetWeapon(PrototypeFirearms.Rifle22), effective: 18, maximum: 45);
    }

    [Fact]
    public void WeaponModsLoadPrototypeSlotsCompatibilityAndEffects()
    {
        var catalog = LoadFirearmCatalog();
        var pistol = catalog.GetWeapon(PrototypeFirearms.Pistol9mm);
        var huntingRifle = catalog.GetWeapon(PrototypeItems.HuntingRifle);
        var redDot = catalog.GetWeaponMod(PrototypeFirearms.RedDotSight);
        var huntingScope = catalog.GetWeaponMod(PrototypeFirearms.HuntingScope);
        var matchBarrel = catalog.GetWeaponMod(PrototypeFirearms.MatchBarrel);

        Assert.Equal(PrototypeFirearms.OpticSlot, redDot.Slot);
        Assert.True(redDot.IsCompatibleWith(pistol));
        Assert.Equal(3, redDot.EffectiveRangeBonus);
        Assert.Equal(PrototypeFirearms.OpticSlot, huntingScope.Slot);
        Assert.True(huntingScope.IsCompatibleWith(huntingRifle));
        Assert.False(huntingScope.IsCompatibleWith(pistol));
        Assert.Equal(16, huntingScope.MaximumRangeBonus);
        Assert.Equal(PrototypeFirearms.BarrelSlot, matchBarrel.Slot);
        Assert.Equal(5, matchBarrel.DamageBonus);
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
        Assert.Equal(25, standard9mm.Damage);
        Assert.Equal(35, hollowPoint9mm.Damage);
        Assert.Equal(55, buckshot.Damage);
        Assert.Equal(80, slug.Damage);
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

        var loadResult = pipeline.Execute(new LoadFeedDeviceActionRequest(PrototypeFirearms.Magazine9mmStandard, PrototypeFirearms.Ammo9mmStandard), state
        );

        Assert.True(loadResult.Succeeded);
        Assert.Equal(15 * FirearmActionService.LoadRoundTickCost, loadResult.ElapsedTicks);
        Assert.Equal(15 * FirearmActionService.LoadRoundTickCost, state.Time.ElapsedTicks);
        Assert.Equal(5, state.Player.Inventory.CountOf(PrototypeFirearms.Ammo9mmStandard));

        var unloadResult = pipeline.Execute(new UnloadFeedDeviceActionRequest(PrototypeFirearms.Magazine9mmStandard), state
        );

        Assert.True(unloadResult.Succeeded);
        Assert.Equal(20, state.Player.Inventory.CountOf(PrototypeFirearms.Ammo9mmStandard));
    }

    [Fact]
    public void UnloadingFeedDeviceRestoresLooseAmmoWhenInventoryGridIsFull()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeFirearms.Magazine9mmStandard);
        for (var index = 0; index < 199; index++)
        {
            state.Player.Inventory.Add(new ItemId($"filler_{index}"));
        }

        state.Player.Inventory.Add(PrototypeFirearms.Ammo9mmStandard, 20, usesGrid: false);

        var loadResult = pipeline.Execute(new LoadFeedDeviceActionRequest(PrototypeFirearms.Magazine9mmStandard, PrototypeFirearms.Ammo9mmStandard), state
        );
        var unloadResult = pipeline.Execute(new UnloadFeedDeviceActionRequest(PrototypeFirearms.Magazine9mmStandard), state
        );

        Assert.True(loadResult.Succeeded);
        Assert.True(unloadResult.Succeeded);
        Assert.Equal(20, state.Player.Inventory.CountOf(PrototypeFirearms.Ammo9mmStandard));
        Assert.False(state.Player.Inventory.Container.Contains(ContainerItemRef.Stack(PrototypeFirearms.Ammo9mmStandard)));
    }

    [Fact]
    public void LoadingWrongAmmunitionFailsWithoutMutatingInventoryOrState()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeFirearms.Magazine9mmStandard);
        state.Player.Inventory.Add(PrototypeFirearms.Ammo12GaugeBuckshot, 10);

        var result = pipeline.Execute(new LoadFeedDeviceActionRequest(PrototypeFirearms.Magazine9mmStandard, PrototypeFirearms.Ammo12GaugeBuckshot), state
        );

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
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

        var insertResult = pipeline.Execute(new InsertFeedDeviceActionRequest(PrototypeFirearms.Pistol9mm, PrototypeFirearms.Magazine9mmStandard), state
        );

        Assert.True(insertResult.Succeeded);
        Assert.Equal(0, state.Player.Inventory.CountOf(PrototypeFirearms.Magazine9mmStandard));

        var removeResult = pipeline.Execute(new RemoveFeedDeviceActionRequest(PrototypeFirearms.Pistol9mm), state
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

        var result = pipeline.Execute(new InsertFeedDeviceActionRequest(PrototypeFirearms.Pistol9mm, PrototypeFirearms.MagazineAk30Round), state
        );

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
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

        var result = pipeline.Execute(new LoadWeaponActionRequest(PrototypeItems.HuntingRifle, PrototypeFirearms.Ammo308Standard), state
        );

        Assert.True(result.Succeeded);
        Assert.Equal(4 * FirearmActionService.LoadRoundTickCost, result.ElapsedTicks);
        Assert.Equal(4 * FirearmActionService.LoadRoundTickCost, state.Time.ElapsedTicks);
        Assert.Equal(6, state.Player.Inventory.CountOf(PrototypeFirearms.Ammo308Standard));
        Assert.True(state.Player.Firearms.TryGetWeapon(PrototypeItems.HuntingRifle, out var weaponState));
        var activeFeed = state.Player.Firearms.GetActiveFeedForWeapon(weaponState);
        Assert.NotNull(activeFeed);
        Assert.Equal(4, activeFeed.LoadedCount);
    }

    [Fact]
    public void DetachableMagazineWeaponRejectsDirectAmmunitionLoading()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeFirearms.Pistol9mm);
        state.Player.Inventory.Add(PrototypeFirearms.Ammo9mmStandard, 10);

        var result = pipeline.Execute(new LoadWeaponActionRequest(PrototypeFirearms.Pistol9mm, PrototypeFirearms.Ammo9mmStandard), state
        );

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(10, state.Player.Inventory.CountOf(PrototypeFirearms.Ammo9mmStandard));
        Assert.Contains("9mm pistol must use a compatible feed device.", result.Messages);
    }

    [Fact]
    public void FailedDirectWeaponLoadDoesNotCreateWeaponState()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeItems.HuntingRifle);

        var result = pipeline.Execute(new LoadWeaponActionRequest(PrototypeItems.HuntingRifle, PrototypeFirearms.Ammo308Standard), state
        );

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
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

        pipeline.Execute(new LoadFeedDeviceActionRequest(PrototypeFirearms.Magazine9mmStandard, PrototypeFirearms.Ammo9mmStandard), state);
        pipeline.Execute(new InsertFeedDeviceActionRequest(PrototypeFirearms.Pistol9mm, PrototypeFirearms.Magazine9mmStandard), state);

        var result = pipeline.Execute(new TestFireActionRequest(PrototypeFirearms.Pistol9mm), state);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.True(state.Player.Firearms.TryGetFeedDevice(PrototypeFirearms.Magazine9mmStandard, out var magazine));
        Assert.Equal(14, magazine.LoadedCount);
    }

    [Fact]
    public void ReloadStatefulWeaponIsAvailableWhenInsertedMagazineCanAcceptHeldAmmo()
    {
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var state = CreateState();
        var pistol = state.StatefulItems.Create(
            PrototypeFirearms.Pistol9mm,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearms
        );
        var magazine = state.StatefulItems.Create(
            PrototypeFirearms.Magazine9mmStandard,
            1,
            StatefulItemLocation.Inserted(pistol.Id),
            firearms
        );
        magazine.FeedDevice!.Load(firearms.GetAmmunition(PrototypeFirearms.Ammo9mmStandard), 5);
        pistol.Weapon!.InsertFeedDevice(magazine.Id);
        state.Player.Inventory.Add(PrototypeFirearms.Ammo9mmStandard, 20);

        var actions = pipeline.GetAvailableActions(state);

        Assert.Contains(actions, action =>
            action.Kind == GameActionKind.ReloadStatefulWeapon
            && action.Request is ReloadStatefulWeaponActionRequest request
            && request.WeaponItemId == pistol.Id
            && request.AmmunitionItemId == PrototypeFirearms.Ammo9mmStandard
        );
    }

    [Fact]
    public void ReloadStatefulWeaponTopsOffInsertedMagazineAndAdvancesCompositeTime()
    {
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var state = CreateState();
        var pistol = state.StatefulItems.Create(
            PrototypeFirearms.Pistol9mm,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearms
        );
        var magazine = state.StatefulItems.Create(
            PrototypeFirearms.Magazine9mmStandard,
            1,
            StatefulItemLocation.Inserted(pistol.Id),
            firearms
        );
        magazine.FeedDevice!.Load(firearms.GetAmmunition(PrototypeFirearms.Ammo9mmStandard), 5);
        pistol.Weapon!.InsertFeedDevice(magazine.Id);
        state.Player.Inventory.Add(PrototypeFirearms.Ammo9mmStandard, 20);

        var result = pipeline.Execute(new ReloadStatefulWeaponActionRequest(pistol.Id, PrototypeFirearms.Ammo9mmStandard), state
        );

        var expectedTicks = FirearmActionService.RemoveFeedDeviceTickCost
            + (10 * FirearmActionService.LoadRoundTickCost)
            + FirearmActionService.InsertFeedDeviceTickCost;

        Assert.True(result.Succeeded);
        Assert.Equal(expectedTicks, result.ElapsedTicks);
        Assert.Equal(expectedTicks, state.Time.ElapsedTicks);
        Assert.Equal(10, state.Player.Inventory.CountOf(PrototypeFirearms.Ammo9mmStandard));
        Assert.Equal(15, magazine.FeedDevice.LoadedCount);
        var insertedLoc1 = Assert.IsType<InsertedLocation>(magazine.Location);
        Assert.Equal(StatefulItemLocationKind.Inserted, magazine.Location.Kind);
        Assert.Equal(pistol.Id, insertedLoc1.ParentItemId);
        Assert.Equal(magazine.Id, pistol.Weapon.InsertedFeedDeviceItemId);
        Assert.Contains(
            "Reloaded 10 9mm standard rounds into 9mm standard pistol magazine (remove 25 ticks, load 100 ticks, insert 25 ticks).",
            result.Messages
        );
        Assert.Contains("Time +150.", result.Messages);
    }

    [Fact]
    public void ReloadStatefulWeaponFailsSafelyWhenInsertedMagazineIsFull()
    {
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var state = CreateState();
        var pistol = state.StatefulItems.Create(
            PrototypeFirearms.Pistol9mm,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearms
        );
        var magazine = state.StatefulItems.Create(
            PrototypeFirearms.Magazine9mmStandard,
            1,
            StatefulItemLocation.Inserted(pistol.Id),
            firearms
        );
        magazine.FeedDevice!.Load(firearms.GetAmmunition(PrototypeFirearms.Ammo9mmStandard), 15);
        pistol.Weapon!.InsertFeedDevice(magazine.Id);
        state.Player.Inventory.Add(PrototypeFirearms.Ammo9mmStandard, 20);

        var result = pipeline.Execute(new ReloadStatefulWeaponActionRequest(pistol.Id, PrototypeFirearms.Ammo9mmStandard), state
        );

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(20, state.Player.Inventory.CountOf(PrototypeFirearms.Ammo9mmStandard));
        Assert.Equal(15, magazine.FeedDevice.LoadedCount);
        var insertedLoc2 = Assert.IsType<InsertedLocation>(magazine.Location);
        Assert.Equal(StatefulItemLocationKind.Inserted, magazine.Location.Kind);
        Assert.Equal(pistol.Id, insertedLoc2.ParentItemId);
    }

    [Fact]
    public void TestFireEmptyWeaponGivesClearFeedbackWithoutMutation()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeFirearms.Pistol9mm);

        var result = pipeline.Execute(new TestFireActionRequest(PrototypeFirearms.Pistol9mm), state);

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Contains("Weapon is empty.", result.Messages);
        Assert.Empty(state.Player.Firearms.Weapons);
    }

    [Fact]
    public void ShootingNpcWithEquippedLoadedStatefulWeaponConsumesAmmoDamagesNpcAndAdvancesTime()
    {
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var npcs = new NpcRoster();
        var target = new NpcState(PrototypeNpcs.TestDummy, "Test Dummy", new GridPosition(3, 2), 200, 200);
        npcs.Add(target);
        var state = CreateState(npcs: npcs);
        var pistol = state.StatefulItems.Create(
            PrototypeFirearms.Pistol9mm,
            1,
            StatefulItemLocation.Equipment(EquipmentSlotId.MainHand),
            firearms
        );
        var magazine = state.StatefulItems.Create(
            PrototypeFirearms.Magazine9mmStandard,
            1,
            StatefulItemLocation.Inserted(pistol.Id),
            firearms
        );
        magazine.FeedDevice!.Load(firearms.GetAmmunition(PrototypeFirearms.Ammo9mmStandard), 5);
        pistol.Weapon!.InsertFeedDevice(magazine.Id);

        var result = pipeline.Execute(new ShootNpcActionRequest(PrototypeNpcs.TestDummy), state);

        Assert.True(result.Succeeded);
        Assert.Equal(GameActionPipeline.ShootTickCost, result.ElapsedTicks);
        Assert.Equal(GameActionPipeline.ShootTickCost, state.Time.ElapsedTicks);
        Assert.Equal(4, magazine.FeedDevice.LoadedCount);
        Assert.Equal(175, target.Health.Current);
        Assert.Contains("Shot Test Dummy with 9mm pistol using 9mm standard rounds for 25 damage.", result.Messages);
        Assert.Contains("Test Dummy health: 175/200.", result.Messages);
    }

    [Fact]
    public void InstallingAndRemovingStatefulWeaponModMovesItemAndAdvancesTime()
    {
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var state = CreateState();
        var pistol = state.StatefulItems.Create(
            PrototypeFirearms.Pistol9mm,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearms
        );
        var redDot = state.StatefulItems.Create(
            PrototypeFirearms.RedDotSight,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearms
        );

        var installResult = pipeline.Execute(new InstallStatefulWeaponModActionRequest(pistol.Id, redDot.Id), state);

        Assert.True(installResult.Succeeded);
        Assert.Equal(FirearmActionService.InstallWeaponModTickCost, installResult.ElapsedTicks);
        Assert.Equal(FirearmActionService.InstallWeaponModTickCost, state.Time.ElapsedTicks);
        Assert.True(pistol.Weapon!.TryGetInstalledMod(PrototypeFirearms.OpticSlot, out var installedModId));
        Assert.Equal(redDot.Id, installedModId);
        var insertedLocation = Assert.IsType<InsertedLocation>(redDot.Location);
        Assert.Equal(pistol.Id, insertedLocation.ParentItemId);

        var removeResult = pipeline.Execute(new RemoveStatefulWeaponModActionRequest(pistol.Id, PrototypeFirearms.OpticSlot), state);

        Assert.True(removeResult.Succeeded);
        Assert.Equal(FirearmActionService.RemoveWeaponModTickCost, removeResult.ElapsedTicks);
        Assert.Equal(
            FirearmActionService.InstallWeaponModTickCost + FirearmActionService.RemoveWeaponModTickCost,
            state.Time.ElapsedTicks
        );
        Assert.False(pistol.Weapon.HasInstalledMod(PrototypeFirearms.OpticSlot));
        Assert.Equal(StatefulItemLocationKind.PlayerInventory, redDot.Location.Kind);
    }

    [Fact]
    public void InstallingSecondWeaponModInSameSlotFailsWithoutMutation()
    {
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var state = CreateState();
        var rifle = state.StatefulItems.Create(
            PrototypeItems.Ak47,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearms
        );
        var redDot = state.StatefulItems.Create(
            PrototypeFirearms.RedDotSight,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearms
        );
        var huntingScope = state.StatefulItems.Create(
            PrototypeFirearms.HuntingScope,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearms
        );

        var installRedDot = pipeline.Execute(new InstallStatefulWeaponModActionRequest(rifle.Id, redDot.Id), state);
        var installScope = pipeline.Execute(new InstallStatefulWeaponModActionRequest(rifle.Id, huntingScope.Id), state);

        Assert.True(installRedDot.Succeeded);
        Assert.False(installScope.Succeeded);
        Assert.Equal(StatefulItemLocationKind.PlayerInventory, huntingScope.Location.Kind);
        Assert.True(rifle.Weapon!.TryGetInstalledMod(PrototypeFirearms.OpticSlot, out var installedModId));
        Assert.Equal(redDot.Id, installedModId);
        Assert.Contains("AK-style rifle already has a mod installed in the optic slot.", installScope.Messages);
    }

    [Fact]
    public void InstallingIncompatibleWeaponModFailsWithoutMutation()
    {
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var state = CreateState();
        var pistol = state.StatefulItems.Create(
            PrototypeFirearms.Pistol9mm,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearms
        );
        var huntingScope = state.StatefulItems.Create(
            PrototypeFirearms.HuntingScope,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearms
        );

        var result = pipeline.Execute(new InstallStatefulWeaponModActionRequest(pistol.Id, huntingScope.Id), state);

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(StatefulItemLocationKind.PlayerInventory, huntingScope.Location.Kind);
        Assert.Empty(pistol.Weapon!.InstalledMods);
        Assert.Contains("hunting scope does not fit 9mm pistol.", result.Messages);
    }

    [Fact]
    public void InstalledWeaponModExtendsShootingMaximumRange()
    {
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var npcs = new NpcRoster();
        var target = new NpcState(PrototypeNpcs.TestDummy, "Test Dummy", new GridPosition(87, 2), 200, 200);
        npcs.Add(target);
        var state = CreateState(new GridBounds(100, 5), npcs, startPosition: new GridPosition(2, 2));
        var rifle = state.StatefulItems.Create(
            PrototypeItems.HuntingRifle,
            1,
            StatefulItemLocation.Equipment(EquipmentSlotId.MainHand),
            firearms
        );
        rifle.Weapon!.BuiltInFeed!.Load(firearms.GetAmmunition(PrototypeFirearms.Ammo308Standard), 4);
        var scope = state.StatefulItems.Create(
            PrototypeFirearms.HuntingScope,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearms
        );

        var installResult = pipeline.Execute(new InstallStatefulWeaponModActionRequest(rifle.Id, scope.Id), state);
        var shootResult = pipeline.Execute(new ShootNpcActionRequest(PrototypeNpcs.TestDummy), state);

        Assert.True(installResult.Succeeded);
        Assert.True(shootResult.Succeeded);
        Assert.Equal(130, target.Health.Current);
        Assert.Contains("Shot Test Dummy with .308 hunting rifle using .308 standard rounds for 70 damage.", shootResult.Messages);
    }

    [Fact]
    public void InstalledWeaponModIncreasesShootingDamage()
    {
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var npcs = new NpcRoster();
        var target = new NpcState(PrototypeNpcs.TestDummy, "Test Dummy", new GridPosition(3, 2), 200, 200);
        npcs.Add(target);
        var state = CreateState(npcs: npcs);
        var pistol = state.StatefulItems.Create(
            PrototypeFirearms.Pistol9mm,
            1,
            StatefulItemLocation.Equipment(EquipmentSlotId.MainHand),
            firearms
        );
        var magazine = state.StatefulItems.Create(
            PrototypeFirearms.Magazine9mmStandard,
            1,
            StatefulItemLocation.Inserted(pistol.Id),
            firearms
        );
        magazine.FeedDevice!.Load(firearms.GetAmmunition(PrototypeFirearms.Ammo9mmStandard), 5);
        pistol.Weapon!.InsertFeedDevice(magazine.Id);
        var matchBarrel = state.StatefulItems.Create(
            PrototypeFirearms.MatchBarrel,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearms
        );

        var installResult = pipeline.Execute(new InstallStatefulWeaponModActionRequest(pistol.Id, matchBarrel.Id), state);
        var shootResult = pipeline.Execute(new ShootNpcActionRequest(PrototypeNpcs.TestDummy), state);

        Assert.True(installResult.Succeeded);
        Assert.True(shootResult.Succeeded);
        Assert.Equal(170, target.Health.Current);
        Assert.Contains("Shot Test Dummy with 9mm pistol using 9mm standard rounds for 30 damage.", shootResult.Messages);
    }

    [Fact]
    public void InspectStatefulWeaponShowsInstalledModsAndModifiedStats()
    {
        var pipeline = CreatePipelineWithItemCatalog();
        var firearms = LoadFirearmCatalog();
        var state = CreateState();
        var pistol = state.StatefulItems.Create(
            PrototypeFirearms.Pistol9mm,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearms
        );
        var matchBarrel = state.StatefulItems.Create(
            PrototypeFirearms.MatchBarrel,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearms
        );
        pipeline.Execute(new InstallStatefulWeaponModActionRequest(pistol.Id, matchBarrel.Id), state);

        var result = pipeline.Execute(new InspectStatefulItemActionRequest(pistol.Id), state);

        Assert.True(result.Succeeded);
        Assert.Contains(result.Messages, message => message.Contains("Modified range: 10 effective / 24 max tiles"));
        Assert.Contains(result.Messages, message => message.Contains("Damage bonus: +5"));
        Assert.Contains(result.Messages, message => message.Contains("Mods: barrel: Match barrel"));
    }

    [Fact]
    public void ShootingNpcRequiresEquippedFirearm()
    {
        var pipeline = CreatePipeline();
        var npcs = new NpcRoster();
        var target = new NpcState(PrototypeNpcs.TestDummy, "Test Dummy", new GridPosition(3, 2), 200, 200);
        npcs.Add(target);
        var state = CreateState(npcs: npcs);

        var result = pipeline.Execute(new ShootNpcActionRequest(PrototypeNpcs.TestDummy), state);

        Assert.False(result.Succeeded);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(200, target.Health.Current);
        Assert.Contains("No equipped firearm.", result.Messages);
    }

    [Fact]
    public void ShootingNpcRequiresLoadedFirearm()
    {
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var npcs = new NpcRoster();
        var target = new NpcState(PrototypeNpcs.TestDummy, "Test Dummy", new GridPosition(3, 2), 200, 200);
        npcs.Add(target);
        var state = CreateState(npcs: npcs);
        state.StatefulItems.Create(
            PrototypeFirearms.Pistol9mm,
            1,
            StatefulItemLocation.Equipment(EquipmentSlotId.MainHand),
            firearms
        );

        var result = pipeline.Execute(new ShootNpcActionRequest(PrototypeNpcs.TestDummy), state);

        Assert.False(result.Succeeded);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(200, target.Health.Current);
        Assert.Contains("9mm pistol is empty.", result.Messages);
    }

    [Fact]
    public void ShootingNpcRejectsTargetsOutsideWeaponMaximumRangeWithoutConsumingAmmo()
    {
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var npcs = new NpcRoster();
        var target = new NpcState(PrototypeNpcs.TestDummy, "Test Dummy", new GridPosition(25, 2), 200, 200);
        npcs.Add(target);
        var state = CreateState(new GridBounds(30, 5), npcs, startPosition: new GridPosition(2, 2));
        var pistol = state.StatefulItems.Create(
            PrototypeFirearms.Pistol9mm,
            1,
            StatefulItemLocation.Equipment(EquipmentSlotId.MainHand),
            firearms
        );
        var magazine = state.StatefulItems.Create(
            PrototypeFirearms.Magazine9mmStandard,
            1,
            StatefulItemLocation.Inserted(pistol.Id),
            firearms
        );
        magazine.FeedDevice!.Load(firearms.GetAmmunition(PrototypeFirearms.Ammo9mmStandard), 5);
        pistol.Weapon!.InsertFeedDevice(magazine.Id);

        var result = pipeline.Execute(new ShootNpcActionRequest(PrototypeNpcs.TestDummy), state);

        Assert.False(result.Succeeded);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(5, magazine.FeedDevice.LoadedCount);
        Assert.Equal(200, target.Health.Current);
        Assert.Contains("Test Dummy is out of range for 9mm pistol (23/20 tiles).", result.Messages);
    }

    private static GameActionPipeline CreatePipeline()
    {
        return new GameActionPipeline(new ItemCatalog(), firearmCatalog: LoadFirearmCatalog());
    }

    private static GameActionPipeline CreatePipelineWithItemCatalog()
    {
        return new GameActionPipeline(LoadItemCatalog(), firearmCatalog: LoadFirearmCatalog());
    }

    private static PrototypeGameState CreateState(
        GridBounds? bounds = null,
        NpcRoster? npcs = null,
        GridPosition? startPosition = null
    )
    {
        var mapBounds = bounds ?? new GridBounds(5, 5);
        return new PrototypeGameState(
            mapBounds,
            new TileItemMap(),
            new TileSurfaceMap(mapBounds, PrototypeSurfaces.Concrete),
            new TileObjectMap(),
            npcs ?? new NpcRoster(),
            startPosition ?? new GridPosition(2, 2)
        );
    }

    private static FirearmCatalog LoadFirearmCatalog()
    {
        return new FirearmDefinitionLoader().LoadDirectory(GetFirearmDataPath());
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

    private static void AssertWeaponRange(WeaponDefinition weapon, int effective, int maximum)
    {
        Assert.Equal(effective, weapon.EffectiveRangeTiles);
        Assert.Equal(maximum, weapon.MaximumRangeTiles);
    }
}
