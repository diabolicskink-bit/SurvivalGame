using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class CampaignStateTests
{
    [Fact]
    public void CampaignOwnsPersistentRunState()
    {
        var fixture = CreateCampaignWithSites();

        Assert.Same(fixture.Time, fixture.Campaign.Time);
        Assert.Same(fixture.Player, fixture.Campaign.Player);
        Assert.Same(fixture.StatefulItems, fixture.Campaign.StatefulItems);
        Assert.Same(fixture.WorldMap, fixture.Campaign.WorldMap);
        Assert.Same(fixture.VehicleFuel, fixture.Campaign.VehicleFuel);
        Assert.Same(fixture.TravelCargo, fixture.Campaign.TravelCargo);
        Assert.Equal(CampaignMode.WorldMap, fixture.Campaign.Mode);
    }

    [Fact]
    public void LocalSitesMustShareCampaignPersistentState()
    {
        var fixture = CreateCampaignWithSites();
        var wrongState = new PrototypeGameState(
            CreateLocalMapState(new GridBounds(5, 5)),
            new GridPosition(1, 1),
            PrototypeLocalSites.DefaultSiteId
        );

        Assert.Throws<ArgumentException>(() =>
            fixture.Campaign.AddLocalSite(new LocalSiteState(
                wrongState,
                "Wrong Site",
                new GridPosition(1, 1)
            )));
    }

    [Fact]
    public void EnteringAndLeavingLocalSitePreservesWorldMapPosition()
    {
        var fixture = CreateCampaignWithSites();
        fixture.WorldMap.SetDestination(new WorldMapPosition(40, 10));
        fixture.WorldMap.Advance(0.5, fixture.Time, PrototypeTravelMethods.Walking);
        var worldMapPosition = fixture.WorldMap.Position;

        var localSite = fixture.Campaign.EnterLocalSite(PrototypeLocalSites.DefaultSiteId);
        fixture.Campaign.ReturnToWorldMap();

        Assert.Equal(CampaignMode.WorldMap, fixture.Campaign.Mode);
        Assert.Null(fixture.Campaign.ActiveLocalSiteId);
        Assert.Equal(worldMapPosition, fixture.WorldMap.Position);
        Assert.Same(localSite, fixture.Campaign.GetLocalSite(PrototypeLocalSites.DefaultSiteId));
    }

    [Fact]
    public void EnteringLocalSiteSelectsActiveSiteAndClearsWorldMapDestination()
    {
        var fixture = CreateCampaignWithSites();
        fixture.WorldMap.SetDestination(new WorldMapPosition(40, 10));

        var localSite = fixture.Campaign.EnterLocalSite(PrototypeLocalSites.GasStationSiteId);

        Assert.Equal(CampaignMode.LocalSite, fixture.Campaign.Mode);
        Assert.Equal(PrototypeLocalSites.GasStationSiteId, fixture.Campaign.ActiveLocalSiteId);
        Assert.Null(fixture.WorldMap.Destination);
        Assert.Equal(localSite.EntryPosition, fixture.Player.Position);
    }

    [Fact]
    public void ReEnteringSameLocalSitePreservesItsState()
    {
        var fixture = CreateCampaignWithSites();
        var firstEntry = fixture.Campaign.EnterLocalSite(PrototypeLocalSites.DefaultSiteId);
        firstEntry.GameState.GroundItems.Place(new GridPosition(2, 2), PrototypeItems.Stone, 2);
        firstEntry.GameState.SetPlayerPosition(new GridPosition(3, 3));

        fixture.Campaign.ReturnToWorldMap();
        var secondEntry = fixture.Campaign.EnterLocalSite(PrototypeLocalSites.DefaultSiteId);

        Assert.Same(firstEntry, secondEntry);
        Assert.Same(firstEntry.GameState, secondEntry.GameState);
        Assert.Equal(new GridPosition(3, 3), secondEntry.GameState.PlayerPosition);
        Assert.Contains(
            secondEntry.GameState.GroundItems.ItemsAt(new GridPosition(2, 2)),
            stack => stack.ItemId == PrototypeItems.Stone && stack.Quantity == 2
        );
    }

    [Fact]
    public void SharedPlayerInventoryEquipmentAndStatefulItemsPersistAcrossModes()
    {
        var fixture = CreateCampaignWithSites();
        fixture.Player.Inventory.Add(PrototypeItems.Stone, 3, InventoryItemSize.Default);
        fixture.Player.Equipment.OccupySlot(
            EquipmentSlotId.MainHand,
            new EquippedItemRef(PrototypeItems.Ak47, new ItemTypePath("Weapon", "Gun"))
        );
        var statefulItem = fixture.StatefulItems.Create(
            PrototypeFirearms.Magazine9mmStandard,
            1,
            StatefulItemLocation.PlayerInventory()
        );

        fixture.Campaign.EnterLocalSite(PrototypeLocalSites.DefaultSiteId);
        fixture.Campaign.ReturnToWorldMap();
        fixture.Campaign.EnterLocalSite(PrototypeLocalSites.GasStationSiteId);

        Assert.Equal(3, fixture.Player.Inventory.CountOf(PrototypeItems.Stone));
        Assert.True(fixture.Player.Equipment.TryGetEquippedItem(EquipmentSlotId.MainHand, out var equippedItem));
        Assert.Equal(PrototypeItems.Ak47, equippedItem.ItemId);
        Assert.Same(statefulItem, fixture.StatefulItems.Get(statefulItem.Id));
    }

    [Fact]
    public void TravelCargoPersistsAcrossLocalSites()
    {
        var fixture = CreateCampaignWithSites();
        var cargoItem = fixture.StatefulItems.Create(
            PrototypeItems.FuelCan,
            1,
            StatefulItemLocation.TravelCargo(),
            itemCatalog: CreateItemCatalog()
        );
        fixture.TravelCargo.StowStack(PrototypeItems.Stone, 4);
        fixture.TravelCargo.StowStatefulItem(cargoItem.Id);

        fixture.Campaign.EnterLocalSite(PrototypeLocalSites.DefaultSiteId);
        fixture.Campaign.ReturnToWorldMap();
        fixture.Campaign.EnterLocalSite(PrototypeLocalSites.GasStationSiteId);

        Assert.Equal(4, fixture.TravelCargo.CountOf(PrototypeItems.Stone));
        Assert.True(fixture.TravelCargo.ContainsStatefulItem(cargoItem.Id));
        Assert.Same(cargoItem, fixture.StatefulItems.Get(cargoItem.Id));
    }

    [Fact]
    public void TravelAnchorsArePlacedOnceForAnchoredTravelMethods()
    {
        var fixture = CreateCampaignWithSites();
        var site = fixture.Campaign.GetLocalSite(PrototypeLocalSites.DefaultSiteId);
        var catalog = CreateWorldObjectCatalog();

        var firstVehicleAnchor = TravelAnchorService.EnsureAnchor(site, TravelMethodId.Vehicle, catalog);
        var secondVehicleAnchor = TravelAnchorService.EnsureAnchor(site, TravelMethodId.Vehicle, catalog);
        var pushbikeAnchor = TravelAnchorService.EnsureAnchor(site, TravelMethodId.Pushbike, catalog);

        Assert.NotNull(firstVehicleAnchor);
        Assert.NotNull(secondVehicleAnchor);
        Assert.NotNull(pushbikeAnchor);
        Assert.Equal(firstVehicleAnchor.Value.InstanceId, secondVehicleAnchor.Value.InstanceId);
        Assert.Single(site.GameState.WorldObjects.AllObjects, placed => placed.ObjectId == PrototypeWorldObjects.PlayerVehicle);
        Assert.Single(site.GameState.WorldObjects.AllObjects, placed => placed.ObjectId == PrototypeWorldObjects.PlayerPushbike);
    }

    [Fact]
    public void WalkingTravelDoesNotPlaceAnArrivalAnchor()
    {
        var fixture = CreateCampaignWithSites();
        var site = fixture.Campaign.GetLocalSite(PrototypeLocalSites.DefaultSiteId);

        var anchor = TravelAnchorService.EnsureAnchor(site, TravelMethodId.Walking, CreateWorldObjectCatalog());

        Assert.Null(anchor);
        Assert.Empty(site.GameState.WorldObjects.AllObjects);
    }

    private static CampaignFixture CreateCampaignWithSites()
    {
        var time = new WorldTime();
        var player = new PlayerState();
        var statefulItems = new StatefulItemStore();
        var vehicleFuel = new VehicleFuelState(PrototypeTravelMethods.VehicleFuelCapacity, 8);
        var worldMap = new WorldMapTravelState(
            100,
            100,
            new WorldMapPosition(10, 10),
            TravelMethodId.Walking,
            vehicleFuel
        );
        var campaign = new CampaignState(time, player, statefulItems, worldMap, vehicleFuel);

        campaign.AddLocalSite(CreateLocalSite(
            PrototypeLocalSites.DefaultSiteId,
            "Default Site",
            new GridBounds(5, 5),
            new GridPosition(1, 1),
            player,
            time,
            statefulItems
        ));
        campaign.AddLocalSite(CreateLocalSite(
            PrototypeLocalSites.GasStationSiteId,
            "Gas Station",
            new GridBounds(8, 8),
            new GridPosition(2, 2),
            player,
            time,
            statefulItems
        ));

        return new CampaignFixture(campaign, time, player, statefulItems, worldMap, vehicleFuel, campaign.TravelCargo);
    }

    private static LocalSiteState CreateLocalSite(
        SiteId siteId,
        string displayName,
        GridBounds bounds,
        GridPosition entryPosition,
        PlayerState player,
        WorldTime time,
        StatefulItemStore statefulItems)
    {
        return new LocalSiteState(
            new PrototypeGameState(
                CreateLocalMapState(bounds),
                entryPosition,
                player,
                time,
                statefulItems,
                siteId
            ),
            displayName,
            entryPosition,
            new Dictionary<TravelMethodId, TravelAnchorPlacement>
            {
                [TravelMethodId.Vehicle] = new(TravelMethodId.Vehicle, new GridPosition(0, 1), WorldObjectFacing.North),
                [TravelMethodId.Pushbike] = new(TravelMethodId.Pushbike, new GridPosition(3, 1), WorldObjectFacing.North)
            }
        );
    }

    private static LocalMapState CreateLocalMapState(GridBounds bounds)
    {
        return new LocalMapState(
            new LocalMap(bounds, new TileSurfaceMap(bounds, PrototypeSurfaces.Concrete)),
            new TileItemMap(),
            new TileObjectMap()
        );
    }

    private static ItemCatalog CreateItemCatalog()
    {
        var catalog = new ItemCatalog();
        catalog.Add(new ItemDefinition(
            PrototypeItems.FuelCan,
            "Fuel can",
            "",
            "Tool",
            fuelContainer: new FuelContainerDefinition(5.0)
        ));
        return catalog;
    }

    private static WorldObjectCatalog CreateWorldObjectCatalog()
    {
        var catalog = new WorldObjectCatalog();
        catalog.Add(new WorldObjectDefinition(
            PrototypeWorldObjects.PlayerVehicle,
            "Your vehicle",
            "",
            "Vehicle",
            new[] { "travel_anchor", "cargo_anchor", "fuel_receiver" },
            blocksMovement: true,
            footprint: new WorldObjectFootprint(2, 4)
        ));
        catalog.Add(new WorldObjectDefinition(
            PrototypeWorldObjects.PlayerPushbike,
            "Your pushbike",
            "",
            "Vehicle",
            new[] { "travel_anchor", "cargo_anchor" },
            footprint: new WorldObjectFootprint(1, 2)
        ));
        return catalog;
    }

    private sealed record CampaignFixture(
        CampaignState Campaign,
        WorldTime Time,
        PlayerState Player,
        StatefulItemStore StatefulItems,
        WorldMapTravelState WorldMap,
        VehicleFuelState VehicleFuel,
        TravelCargoStore TravelCargo
    );
}
