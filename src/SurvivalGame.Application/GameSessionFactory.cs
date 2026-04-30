using SurvivalGame.Domain;

namespace SurvivalGame.Application;

public static class GameSessionFactory
{
    public static LocalSiteSession CreateStandaloneLocalSiteSession(
        GameContentPaths paths,
        VehicleFuelState? vehicleFuelState = null)
    {
        var campaignSession = CreateCampaignSession(paths, vehicleFuelState);
        var localSite = campaignSession.CampaignState.EnterLocalSite(PrototypeLocalSites.DefaultSiteId);
        return campaignSession.CreateLocalSiteSession(localSite.Id);
    }

    public static CampaignSession CreateCampaignSession(
        GameContentPaths paths,
        VehicleFuelState? vehicleFuelState = null)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var itemCatalog = new ItemDefinitionLoader().LoadDirectory(paths.Items);
        var firearmCatalog = new FirearmDefinitionLoader().LoadDirectory(paths.Firearms);
        var surfaceCatalog = new TileSurfaceDefinitionLoader().LoadDirectory(paths.Surfaces);
        var worldObjectCatalog = new WorldObjectDefinitionLoader().LoadDirectory(paths.WorldObjects);
        var structureCatalog = new StructureDefinitionLoader().LoadDirectory(paths.Structures);
        var npcCatalog = new NpcDefinitionLoader().LoadDirectory(paths.Npcs);
        var vehicleFuel = vehicleFuelState ?? new VehicleFuelState(
            PrototypeTravelMethods.VehicleFuelCapacity,
            PrototypeTravelMethods.VehicleStartingFuel
        );
        var time = new WorldTime();
        var player = new PlayerState();
        var statefulItems = new StatefulItemStore();
        var worldMapState = new WorldMapTravelState(
            PrototypeWorldMapSites.MapWidth,
            PrototypeWorldMapSites.MapHeight,
            PrototypeWorldMapSites.StartPosition,
            TravelMethodId.Walking,
            vehicleFuel
        );
        var campaignState = new CampaignState(time, player, statefulItems, worldMapState, vehicleFuel);

        var localSites = new LocalSiteDefinitionLoader().LoadDirectory(
            paths.LocalMaps,
            surfaceCatalog,
            worldObjectCatalog,
            structureCatalog,
            itemCatalog,
            npcCatalog
        );

        LocalSiteState? defaultLocalSite = null;
        foreach (var site in localSites)
        {
            var localSite = CreateLocalSiteState(site, player, time, statefulItems);
            campaignState.AddLocalSite(localSite);
            if (site.Id == PrototypeLocalSites.DefaultSiteId)
            {
                defaultLocalSite = localSite;
            }
        }

        if (defaultLocalSite is null)
        {
            throw new InvalidOperationException($"Required local site '{PrototypeLocalSites.DefaultSiteId}' was not loaded.");
        }

        AddStartingItems(defaultLocalSite.GameState, itemCatalog, firearmCatalog);

        var actionPipeline = new GameActionPipeline(
            itemCatalog,
            worldObjectCatalog,
            firearmCatalog,
            vehicleFuel,
            npcCatalog,
            structureCatalog,
            campaignState.TravelCargo
        );

        return new CampaignSession(
            campaignState,
            itemCatalog,
            firearmCatalog,
            surfaceCatalog,
            worldObjectCatalog,
            structureCatalog,
            npcCatalog,
            actionPipeline
        );
    }

    private static LocalSiteState CreateLocalSiteState(
        PrototypeLocalSite site,
        PlayerState player,
        WorldTime time,
        StatefulItemStore statefulItems)
    {
        return new LocalSiteState(
            CreateGameState(site, player, time, statefulItems),
            site.DisplayName,
            site.StartPosition,
            site.ArrivalAnchors
        );
    }

    private static PrototypeGameState CreateGameState(
        PrototypeLocalSite site,
        PlayerState? player = null,
        WorldTime? time = null,
        StatefulItemStore? statefulItems = null)
    {
        return new PrototypeGameState(
            new LocalMapState(
                new LocalMap(site.Bounds, site.Surfaces),
                site.GroundItems,
                site.WorldObjects,
                site.Npcs,
                structures: site.Structures
            ),
            site.StartPosition,
            player ?? new PlayerState(),
            time ?? new WorldTime(),
            statefulItems ?? new StatefulItemStore(),
            site.Id
        );
    }

    private static void AddStartingItems(
        PrototypeGameState gameState,
        ItemCatalog itemCatalog,
        FirearmCatalog firearmCatalog)
    {
        AddStack(gameState, itemCatalog, PrototypeItems.Stone, 3);
        AddStack(gameState, itemCatalog, PrototypeItems.Branch, 2);
        AddStack(gameState, itemCatalog, PrototypeItems.WaterBottle);
        AddStack(gameState, itemCatalog, PrototypeFirearms.Ammo9mmStandard, 35);
        AddStack(gameState, itemCatalog, PrototypeFirearms.Ammo9mmHollowPoint, 20);
        AddStack(gameState, itemCatalog, PrototypeFirearms.Ammo762x39Standard, 60);
        AddStack(gameState, itemCatalog, PrototypeFirearms.Ammo556Standard, 60);
        AddStack(gameState, itemCatalog, PrototypeFirearms.Ammo308Standard, 20);
        AddStack(gameState, itemCatalog, PrototypeFirearms.Ammo12GaugeBuckshot, 20);
        AddStack(gameState, itemCatalog, PrototypeFirearms.Ammo12GaugeSlug, 10);
        AddStack(gameState, itemCatalog, PrototypeFirearms.Ammo22LrStandard, 100);
        AddStartingStatefulItems(gameState, itemCatalog, firearmCatalog);
    }

    private static void AddStartingStatefulItems(
        PrototypeGameState gameState,
        ItemCatalog itemCatalog,
        FirearmCatalog firearmCatalog)
    {
        CreateCarriedStatefulItem(gameState, itemCatalog, firearmCatalog, PrototypeFirearms.Pistol9mm);
        CreateCarriedStatefulItem(gameState, itemCatalog, firearmCatalog, PrototypeFirearms.Carbine556);
        CreateCarriedStatefulItem(gameState, itemCatalog, firearmCatalog, PrototypeItems.Ak47);
        CreateCarriedStatefulItem(gameState, itemCatalog, firearmCatalog, PrototypeItems.HuntingRifle);
        CreateCarriedStatefulItem(gameState, itemCatalog, firearmCatalog, PrototypeFirearms.Shotgun12Gauge);
        CreateCarriedStatefulItem(gameState, itemCatalog, firearmCatalog, PrototypeFirearms.Rifle22);
        CreateCarriedStatefulItem(gameState, itemCatalog, firearmCatalog, PrototypeItems.FuelCan);

        var loadedMagazine = CreateCarriedStatefulItem(
            gameState,
            itemCatalog,
            firearmCatalog,
            PrototypeFirearms.Magazine9mmStandard
        );
        loadedMagazine.FeedDevice?.Load(firearmCatalog.GetAmmunition(PrototypeFirearms.Ammo9mmStandard), 15);

        var loadedCarbineMagazine = CreateCarriedStatefulItem(
            gameState,
            itemCatalog,
            firearmCatalog,
            PrototypeFirearms.Magazine55630Round
        );
        loadedCarbineMagazine.FeedDevice?.Load(firearmCatalog.GetAmmunition(PrototypeFirearms.Ammo556Standard), 30);

        CreateCarriedStatefulItem(gameState, itemCatalog, firearmCatalog, PrototypeFirearms.Magazine9mmStandard);
        CreateCarriedStatefulItem(gameState, itemCatalog, firearmCatalog, PrototypeFirearms.Magazine9mmExtended);
        CreateCarriedStatefulItem(gameState, itemCatalog, firearmCatalog, PrototypeFirearms.Magazine55630Round);
        CreateCarriedStatefulItem(gameState, itemCatalog, firearmCatalog, PrototypeFirearms.MagazineAk30Round);
        CreateCarriedStatefulItem(gameState, itemCatalog, firearmCatalog, PrototypeFirearms.MagazineAkDamaged20Round);
        CreateCarriedStatefulItem(gameState, itemCatalog, firearmCatalog, PrototypeFirearms.RedDotSight);
        CreateCarriedStatefulItem(gameState, itemCatalog, firearmCatalog, PrototypeFirearms.HuntingScope);
        CreateCarriedStatefulItem(gameState, itemCatalog, firearmCatalog, PrototypeFirearms.MatchBarrel);

        var droppedMagazine = gameState.StatefulItems.Create(
            PrototypeFirearms.Magazine9mmStandard,
            1,
            StatefulItemLocation.Ground(new GridPosition(11, 7), gameState.SiteId),
            firearmCatalog
        );
        droppedMagazine.FeedDevice?.Load(firearmCatalog.GetAmmunition(PrototypeFirearms.Ammo9mmHollowPoint), 8);

        var backpack = CreateCarriedStatefulItem(gameState, itemCatalog, firearmCatalog, new ItemId("school_backpack"));
        var food = gameState.StatefulItems.Create(
            new ItemId("canned_beans"),
            1,
            StatefulItemLocation.PlayerInventory(),
            firearmCatalog
        );
        gameState.StatefulItems.MoveToContained(food.Id, backpack.Id);
    }

    private static void AddStack(PrototypeGameState gameState, ItemCatalog itemCatalog, ItemId itemId, int quantity = 1)
    {
        gameState.Player.Inventory.Add(
            itemId,
            quantity,
            GetInventorySize(itemCatalog, itemId),
            UsesInventoryGrid(itemCatalog, itemId)
        );
    }

    private static StatefulItem CreateCarriedStatefulItem(
        PrototypeGameState gameState,
        ItemCatalog itemCatalog,
        FirearmCatalog firearmCatalog,
        ItemId itemId)
    {
        var item = gameState.StatefulItems.Create(
            itemId,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearmCatalog,
            itemCatalog: itemCatalog
        );
        if (!gameState.Player.Inventory.Container.TryAutoPlace(
            ContainerItemRef.Stateful(item.Id),
            GetInventorySize(itemCatalog, item.ItemId)))
        {
            throw new InvalidOperationException($"No inventory space available for '{item.ItemId}'.");
        }

        return item;
    }

    private static InventoryItemSize GetInventorySize(ItemCatalog itemCatalog, ItemId itemId)
    {
        return itemCatalog.TryGet(itemId, out var item)
            ? item.InventorySize
            : InventoryItemSize.Default;
    }

    private static bool UsesInventoryGrid(ItemCatalog itemCatalog, ItemId itemId)
    {
        return !itemCatalog.TryGet(itemId, out var item) || InventoryGridRules.UsesGrid(item);
    }
}
