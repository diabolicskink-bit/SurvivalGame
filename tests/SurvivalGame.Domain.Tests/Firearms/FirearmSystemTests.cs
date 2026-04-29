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
        AssertWeaponRange(catalog.GetWeapon(PrototypeFirearms.Carbine556), effective: 22, maximum: 55);
    }

    [Fact]
    public void ExistingWeaponsDefaultToSingleShotMode()
    {
        var catalog = LoadFirearmCatalog();
        var pistol = catalog.GetWeapon(PrototypeFirearms.Pistol9mm);

        Assert.Equal(new[] { WeaponFireMode.SingleShot }, pistol.SupportedFireModes);
        Assert.False(pistol.HasMultipleFireModes);
        Assert.True(pistol.SupportsFireMode(WeaponFireMode.SingleShot));
        Assert.False(pistol.SupportsFireMode(WeaponFireMode.Burst));
    }

    [Fact]
    public void BurstCarbineLoadsAmmoMagazineModesAndModCompatibility()
    {
        var catalog = LoadFirearmCatalog();
        var carbine = catalog.GetWeapon(PrototypeFirearms.Carbine556);
        var ammunition = catalog.GetAmmunition(PrototypeFirearms.Ammo556Standard);
        var magazine = catalog.GetFeedDevice(PrototypeFirearms.Magazine55630Round);

        Assert.Equal("5.56 burst carbine", carbine.Name);
        Assert.Equal("carbine_556", carbine.WeaponFamily);
        Assert.True(carbine.AcceptsAmmunition(ammunition));
        Assert.True(carbine.CanUseFeedDevice(magazine));
        Assert.Equal(PrototypeFirearms.FiveFiveSix, ammunition.Size);
        Assert.Equal(40, ammunition.Damage);
        Assert.Equal(30, magazine.Capacity);
        Assert.Equal(new[] { WeaponFireMode.SingleShot, WeaponFireMode.Burst }, carbine.SupportedFireModes);
        Assert.Equal(3, carbine.BurstRoundCount);
        Assert.Equal(2, carbine.BurstDamageMultiplier);
        Assert.True(catalog.GetWeaponMod(PrototypeFirearms.RedDotSight).IsCompatibleWith(carbine));
        Assert.True(catalog.GetWeaponMod(PrototypeFirearms.HuntingScope).IsCompatibleWith(carbine));
        Assert.True(catalog.GetWeaponMod(PrototypeFirearms.MatchBarrel).IsCompatibleWith(carbine));
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
    public void StackBackedBurstWeaponCanToggleFireModeWithoutAdvancingTime()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeFirearms.Carbine556);

        var actions = pipeline.GetAvailableActions(state);
        Assert.Contains(actions, action => action.Request is ToggleFireModeActionRequest toggle
            && toggle.WeaponItemId == PrototypeFirearms.Carbine556);

        var toggleToBurst = pipeline.Execute(new ToggleFireModeActionRequest(PrototypeFirearms.Carbine556), state);

        Assert.True(toggleToBurst.Succeeded);
        Assert.Equal(0, toggleToBurst.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.True(state.Player.Firearms.TryGetWeapon(PrototypeFirearms.Carbine556, out var weaponState));
        Assert.Equal(WeaponFireMode.Burst, weaponState.CurrentFireMode);
        Assert.Contains("Set 5.56 burst carbine to burst.", toggleToBurst.Messages);

        var toggleToSingle = pipeline.Execute(new ToggleFireModeActionRequest(PrototypeFirearms.Carbine556), state);

        Assert.True(toggleToSingle.Succeeded);
        Assert.Equal(WeaponFireMode.SingleShot, weaponState.CurrentFireMode);
        Assert.Contains("Set 5.56 burst carbine to single shot.", toggleToSingle.Messages);
    }

    [Fact]
    public void SingleShotOnlyWeaponDoesNotExposeFireModeToggle()
    {
        var pipeline = CreatePipeline();
        var state = CreateState();
        state.Player.Inventory.Add(PrototypeFirearms.Pistol9mm);

        var actions = pipeline.GetAvailableActions(state);
        var result = pipeline.Execute(new ToggleFireModeActionRequest(PrototypeFirearms.Pistol9mm), state);

        Assert.DoesNotContain(actions, action => action.Request is ToggleFireModeActionRequest);
        Assert.False(result.Succeeded);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Contains("9mm pistol has only one fire mode.", result.Messages);
    }

    [Fact]
    public void StatefulBurstWeaponCanToggleFireModeWithoutAdvancingTime()
    {
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var state = CreateState();
        var carbine = state.StatefulItems.Create(
            PrototypeFirearms.Carbine556,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearms
        );

        var actions = pipeline.GetAvailableActions(state);
        Assert.Contains(actions, action => action.Request is ToggleStatefulFireModeActionRequest toggle
            && toggle.WeaponItemId == carbine.Id);

        var result = pipeline.Execute(new ToggleStatefulFireModeActionRequest(carbine.Id), state);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(WeaponFireMode.Burst, carbine.Weapon!.CurrentFireMode);
        Assert.Contains("Set 5.56 burst carbine to burst.", result.Messages);
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
        Assert.Contains("Fired single shot at Test Dummy with 9mm pistol using 9mm standard rounds for 25 damage.", result.Messages);
        Assert.Contains("Test Dummy health: 175/200.", result.Messages);
    }

    [Fact]
    public void BurstShootingConsumesThreeRoundsDealsBurstDamageDisablesNpcAndAdvancesBurstTime()
    {
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var npcs = new NpcRoster();
        var target = new NpcState(PrototypeNpcs.TestDummy, "Test Dummy", new GridPosition(3, 2), 80, 80);
        npcs.Add(target);
        var state = CreateState(npcs: npcs);
        var carbine = state.StatefulItems.Create(
            PrototypeFirearms.Carbine556,
            1,
            StatefulItemLocation.Equipment(EquipmentSlotId.MainHand),
            firearms
        );
        var magazine = state.StatefulItems.Create(
            PrototypeFirearms.Magazine55630Round,
            1,
            StatefulItemLocation.Inserted(carbine.Id),
            firearms
        );
        magazine.FeedDevice!.Load(firearms.GetAmmunition(PrototypeFirearms.Ammo556Standard), 5);
        carbine.Weapon!.InsertFeedDevice(magazine.Id);
        pipeline.Execute(new ToggleStatefulFireModeActionRequest(carbine.Id), state);

        var result = pipeline.Execute(new ShootNpcActionRequest(PrototypeNpcs.TestDummy), state);

        Assert.True(result.Succeeded);
        Assert.Equal(GameActionPipeline.BurstShootTickCost, result.ElapsedTicks);
        Assert.Equal(GameActionPipeline.BurstShootTickCost, state.Time.ElapsedTicks);
        Assert.Equal(2, magazine.FeedDevice.LoadedCount);
        Assert.Equal(0, target.Health.Current);
        Assert.True(target.IsDisabled);
        Assert.Contains("Fired 3-round burst at Test Dummy with 5.56 burst carbine using 5.56x45mm standard rounds for 80 damage.", result.Messages);
        Assert.Contains("Test Dummy is disabled.", result.Messages);
    }

    [Fact]
    public void BurstShootingRequiresThreeLoadedRoundsWithoutMutation()
    {
        var pipeline = CreatePipeline();
        var firearms = LoadFirearmCatalog();
        var npcs = new NpcRoster();
        var target = new NpcState(PrototypeNpcs.TestDummy, "Test Dummy", new GridPosition(3, 2), 200, 200);
        npcs.Add(target);
        var state = CreateState(npcs: npcs);
        var carbine = state.StatefulItems.Create(
            PrototypeFirearms.Carbine556,
            1,
            StatefulItemLocation.Equipment(EquipmentSlotId.MainHand),
            firearms
        );
        var magazine = state.StatefulItems.Create(
            PrototypeFirearms.Magazine55630Round,
            1,
            StatefulItemLocation.Inserted(carbine.Id),
            firearms
        );
        magazine.FeedDevice!.Load(firearms.GetAmmunition(PrototypeFirearms.Ammo556Standard), 2);
        carbine.Weapon!.InsertFeedDevice(magazine.Id);
        pipeline.Execute(new ToggleStatefulFireModeActionRequest(carbine.Id), state);

        var result = pipeline.Execute(new ShootNpcActionRequest(PrototypeNpcs.TestDummy), state);

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(2, magazine.FeedDevice.LoadedCount);
        Assert.Equal(200, target.Health.Current);
        Assert.Contains("5.56 burst carbine needs 3 loaded rounds for burst.", result.Messages);
    }

    [Fact]
    public void ShootingNpcFailsWithoutMutationWhenStructureBlocksLineOfFire()
    {
        var firearms = LoadFirearmCatalog();
        var structures = new StructureEdgeMap(new GridBounds(7, 3));
        var wallId = new StructureId("test_wall");
        structures.Place(new GridPosition(2, 1), StructureEdgeDirection.East, wallId);
        var structureCatalog = CreateStructureCatalog(new StructureDefinition(
            wallId,
            "Test wall",
            "",
            "Structure",
            "test",
            "wall",
            blocksSight: true
        ));
        var pipeline = CreatePipeline(structureCatalog: structureCatalog);
        var npcs = CreateTargetRoster(new GridPosition(5, 1), out var target);
        var state = CreateState(new GridBounds(7, 3), npcs, new GridPosition(1, 1), structures: structures);
        var magazine = CreateEquippedLoadedPistol(state, firearms);

        var result = pipeline.Execute(new ShootNpcActionRequest(PrototypeNpcs.TestDummy), state);

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(5, magazine.FeedDevice!.LoadedCount);
        Assert.Equal(200, target.Health.Current);
        Assert.Contains("Line of fire blocked by Test wall.", result.Messages);
    }

    [Fact]
    public void ShootingNpcAllowsNonSightBlockingStructureInLineOfFire()
    {
        var firearms = LoadFirearmCatalog();
        var structures = new StructureEdgeMap(new GridBounds(7, 3));
        var windowId = new StructureId("test_window");
        structures.Place(new GridPosition(2, 1), StructureEdgeDirection.East, windowId);
        var structureCatalog = CreateStructureCatalog(new StructureDefinition(
            windowId,
            "Test window",
            "",
            "Window",
            "test",
            "window",
            blocksMovement: true,
            blocksSight: false
        ));
        var pipeline = CreatePipeline(structureCatalog: structureCatalog);
        var npcs = CreateTargetRoster(new GridPosition(5, 1), out var target);
        var state = CreateState(new GridBounds(7, 3), npcs, new GridPosition(1, 1), structures: structures);
        var magazine = CreateEquippedLoadedPistol(state, firearms);

        var result = pipeline.Execute(new ShootNpcActionRequest(PrototypeNpcs.TestDummy), state);

        Assert.True(result.Succeeded);
        Assert.Equal(GameActionPipeline.ShootTickCost, result.ElapsedTicks);
        Assert.Equal(4, magazine.FeedDevice!.LoadedCount);
        Assert.Equal(175, target.Health.Current);
    }

    [Fact]
    public void ShootingNpcFailsWithoutMutationWhenWorldObjectBlocksLineOfFire()
    {
        var firearms = LoadFirearmCatalog();
        var blockerId = new WorldObjectId("test_blocker");
        var worldObjects = new TileObjectMap();
        worldObjects.Place(new GridPosition(3, 1), blockerId);
        var worldObjectCatalog = CreateWorldObjectCatalog(new WorldObjectDefinition(
            blockerId,
            "Test blocker",
            "",
            "Obstacle",
            blocksSight: true
        ));
        var pipeline = CreatePipeline(worldObjectCatalog: worldObjectCatalog);
        var npcs = CreateTargetRoster(new GridPosition(5, 1), out var target);
        var state = CreateState(new GridBounds(7, 3), npcs, new GridPosition(1, 1), worldObjects, structures: null);
        var magazine = CreateEquippedLoadedPistol(state, firearms);

        var result = pipeline.Execute(new ShootNpcActionRequest(PrototypeNpcs.TestDummy), state);

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(5, magazine.FeedDevice!.LoadedCount);
        Assert.Equal(200, target.Health.Current);
        Assert.Contains("Line of fire blocked by Test blocker.", result.Messages);
    }

    [Fact]
    public void ShootingNpcAllowsNonSightBlockingWorldObjectInLineOfFire()
    {
        var firearms = LoadFirearmCatalog();
        var tableId = new WorldObjectId("test_table");
        var worldObjects = new TileObjectMap();
        worldObjects.Place(new GridPosition(3, 1), tableId);
        var worldObjectCatalog = CreateWorldObjectCatalog(new WorldObjectDefinition(
            tableId,
            "Test table",
            "",
            "Furniture",
            blocksMovement: true,
            blocksSight: false
        ));
        var pipeline = CreatePipeline(worldObjectCatalog: worldObjectCatalog);
        var npcs = CreateTargetRoster(new GridPosition(5, 1), out var target);
        var state = CreateState(new GridBounds(7, 3), npcs, new GridPosition(1, 1), worldObjects, structures: null);
        var magazine = CreateEquippedLoadedPistol(state, firearms);

        var result = pipeline.Execute(new ShootNpcActionRequest(PrototypeNpcs.TestDummy), state);

        Assert.True(result.Succeeded);
        Assert.Equal(GameActionPipeline.ShootTickCost, result.ElapsedTicks);
        Assert.Equal(4, magazine.FeedDevice!.LoadedCount);
        Assert.Equal(175, target.Health.Current);
    }

    [Fact]
    public void ShootingNpcFailsWhenMultiTileObjectFootprintBlocksLineOfFire()
    {
        var firearms = LoadFirearmCatalog();
        var vehicleId = new WorldObjectId("test_vehicle");
        var worldObjects = new TileObjectMap();
        worldObjects.Place(
            new GridPosition(3, 0),
            vehicleId,
            WorldObjectFacing.North,
            new WorldObjectFootprint(2, 2),
            new GridBounds(8, 3)
        );
        var worldObjectCatalog = CreateWorldObjectCatalog(new WorldObjectDefinition(
            vehicleId,
            "Test vehicle",
            "",
            "Vehicle",
            blocksSight: true,
            footprint: new WorldObjectFootprint(2, 2)
        ));
        var pipeline = CreatePipeline(worldObjectCatalog: worldObjectCatalog);
        var npcs = CreateTargetRoster(new GridPosition(6, 1), out var target);
        var state = CreateState(new GridBounds(8, 3), npcs, new GridPosition(1, 1), worldObjects, structures: null);
        var magazine = CreateEquippedLoadedPistol(state, firearms);

        var result = pipeline.Execute(new ShootNpcActionRequest(PrototypeNpcs.TestDummy), state);

        Assert.False(result.Succeeded);
        Assert.Equal(0, result.ElapsedTicks);
        Assert.Equal(0, state.Time.ElapsedTicks);
        Assert.Equal(5, magazine.FeedDevice!.LoadedCount);
        Assert.Equal(200, target.Health.Current);
        Assert.Contains("Line of fire blocked by Test vehicle.", result.Messages);
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
        Assert.Contains("Fired single shot at Test Dummy with .308 hunting rifle using .308 standard rounds for 70 damage.", shootResult.Messages);
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
        Assert.Contains("Fired single shot at Test Dummy with 9mm pistol using 9mm standard rounds for 30 damage.", shootResult.Messages);
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

    private static GameActionPipeline CreatePipeline(
        WorldObjectCatalog? worldObjectCatalog = null,
        StructureCatalog? structureCatalog = null)
    {
        return new GameActionPipeline(
            new ItemCatalog(),
            worldObjectCatalog: worldObjectCatalog,
            firearmCatalog: LoadFirearmCatalog(),
            structureCatalog: structureCatalog
        );
    }

    private static GameActionPipeline CreatePipelineWithItemCatalog()
    {
        return new GameActionPipeline(LoadItemCatalog(), firearmCatalog: LoadFirearmCatalog());
    }

    private static PrototypeGameState CreateState(
        GridBounds? bounds = null,
        NpcRoster? npcs = null,
        GridPosition? startPosition = null,
        TileObjectMap? worldObjects = null,
        StructureEdgeMap? structures = null
    )
    {
        var mapBounds = bounds ?? new GridBounds(5, 5);
        var surfaces = new TileSurfaceMap(mapBounds, PrototypeSurfaces.Concrete);
        return new PrototypeGameState(
            new LocalMapState(
                new LocalMap(mapBounds, surfaces),
                new TileItemMap(),
                worldObjects ?? new TileObjectMap(),
                npcs,
                structures: structures
            ),
            startPosition ?? new GridPosition(2, 2)
        );
    }

    private static NpcRoster CreateTargetRoster(GridPosition targetPosition, out NpcState target)
    {
        var npcs = new NpcRoster();
        target = new NpcState(PrototypeNpcs.TestDummy, "Test Dummy", targetPosition, 200, 200);
        npcs.Add(target);
        return npcs;
    }

    private static StatefulItem CreateEquippedLoadedPistol(
        PrototypeGameState state,
        FirearmCatalog firearms,
        int loadedRounds = 5)
    {
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
        magazine.FeedDevice!.Load(firearms.GetAmmunition(PrototypeFirearms.Ammo9mmStandard), loadedRounds);
        pistol.Weapon!.InsertFeedDevice(magazine.Id);
        return magazine;
    }

    private static WorldObjectCatalog CreateWorldObjectCatalog(params WorldObjectDefinition[] definitions)
    {
        var catalog = new WorldObjectCatalog();
        foreach (var definition in definitions)
        {
            catalog.Add(definition);
        }

        return catalog;
    }

    private static StructureCatalog CreateStructureCatalog(params StructureDefinition[] definitions)
    {
        var catalog = new StructureCatalog();
        foreach (var definition in definitions)
        {
            catalog.Add(definition);
        }

        return catalog;
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
