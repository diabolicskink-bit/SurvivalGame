using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SurvivalGame.Domain;

public sealed class PrototypeGameplaySession
{
    public PrototypeGameplaySession(
        LocalSiteState localSite,
        ItemCatalog itemCatalog,
        FirearmCatalog firearmCatalog,
        TileSurfaceCatalog surfaceCatalog,
        WorldObjectCatalog worldObjectCatalog,
        StructureCatalog structureCatalog,
        NpcCatalog npcCatalog,
        GameActionPipeline actionPipeline,
        WorldObjectInstanceId? activeTravelAnchorInstanceId = null
    )
    {
        LocalSite = localSite;
        GameState = localSite.GameState;
        SiteDisplayName = localSite.DisplayName;
        EntryPosition = localSite.EntryPosition;
        ItemCatalog = itemCatalog;
        FirearmCatalog = firearmCatalog;
        SurfaceCatalog = surfaceCatalog;
        WorldObjectCatalog = worldObjectCatalog;
        StructureCatalog = structureCatalog;
        NpcCatalog = npcCatalog;
        ActionPipeline = actionPipeline;
        ActiveTravelAnchorInstanceId = activeTravelAnchorInstanceId;
    }

    public LocalSiteState LocalSite { get; }

    public PrototypeGameState GameState { get; }

    public string SiteDisplayName { get; }

    public GridPosition EntryPosition { get; }

    public ItemCatalog ItemCatalog { get; }

    public FirearmCatalog FirearmCatalog { get; }

    public TileSurfaceCatalog SurfaceCatalog { get; }

    public WorldObjectCatalog WorldObjectCatalog { get; }

    public StructureCatalog StructureCatalog { get; }

    public NpcCatalog NpcCatalog { get; }

    public GameActionPipeline ActionPipeline { get; }

    public WorldObjectInstanceId? ActiveTravelAnchorInstanceId { get; }
}

public sealed class PrototypeCampaignSession
{
    public PrototypeCampaignSession(
        CampaignState campaignState,
        ItemCatalog itemCatalog,
        FirearmCatalog firearmCatalog,
        TileSurfaceCatalog surfaceCatalog,
        WorldObjectCatalog worldObjectCatalog,
        StructureCatalog structureCatalog,
        NpcCatalog npcCatalog,
        GameActionPipeline actionPipeline
    )
    {
        CampaignState = campaignState;
        ItemCatalog = itemCatalog;
        FirearmCatalog = firearmCatalog;
        SurfaceCatalog = surfaceCatalog;
        WorldObjectCatalog = worldObjectCatalog;
        StructureCatalog = structureCatalog;
        NpcCatalog = npcCatalog;
        ActionPipeline = actionPipeline;
    }

    public CampaignState CampaignState { get; }

    public ItemCatalog ItemCatalog { get; }

    public FirearmCatalog FirearmCatalog { get; }

    public TileSurfaceCatalog SurfaceCatalog { get; }

    public WorldObjectCatalog WorldObjectCatalog { get; }

    public StructureCatalog StructureCatalog { get; }

    public NpcCatalog NpcCatalog { get; }

    public GameActionPipeline ActionPipeline { get; }

    public PrototypeGameplaySession CreateGameplaySession(SiteId siteId)
    {
        var localSite = CampaignState.GetLocalSite(siteId);
        var activeAnchor = TravelAnchorService.EnsureAnchor(
            localSite,
            CampaignState.WorldMap.CurrentTravelMethod,
            WorldObjectCatalog
        );
        if (activeAnchor is not null)
        {
            localSite.GameState.SetActiveTravelAnchor(activeAnchor.Value.InstanceId);
            if (TravelAnchorService.TryFindEntryPosition(
                localSite.GameState,
                activeAnchor.Value.InstanceId,
                WorldObjectCatalog,
                out var entryPosition))
            {
                localSite.GameState.SetPlayerPosition(entryPosition);
            }
        }
        else
        {
            localSite.GameState.ClearActiveTravelAnchor();
        }

        return new PrototypeGameplaySession(
            localSite,
            ItemCatalog,
            FirearmCatalog,
            SurfaceCatalog,
            WorldObjectCatalog,
            StructureCatalog,
            NpcCatalog,
            ActionPipeline,
            activeAnchor?.InstanceId
        );
    }
}

public static class PrototypeSessionFactory
{
    public static PrototypeGameplaySession CreateGameplaySession(VehicleFuelState? vehicleFuelState = null)
    {
        var campaignSession = CreateCampaignSession(vehicleFuelState);
        var localSite = campaignSession.CampaignState.EnterLocalSite(PrototypeLocalSites.DefaultSiteId);
        return campaignSession.CreateGameplaySession(localSite.Id);
    }

    public static PrototypeCampaignSession CreateCampaignSession(VehicleFuelState? vehicleFuelState = null)
    {
        var itemCatalog = LoadItemCatalog();
        var firearmCatalog = LoadFirearmCatalog();
        var surfaceCatalog = LoadSurfaceCatalog();
        var worldObjectCatalog = LoadWorldObjectCatalog();
        var structureCatalog = LoadStructureCatalog();
        var npcCatalog = LoadNpcCatalog();
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

        var localSites = LoadLocalSites(surfaceCatalog, worldObjectCatalog, structureCatalog, itemCatalog, npcCatalog);

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

        AddPrototypeStartingItems(defaultLocalSite.GameState, itemCatalog, firearmCatalog);

        var actionPipeline = new GameActionPipeline(
            itemCatalog,
            worldObjectCatalog,
            firearmCatalog,
            vehicleFuel,
            npcCatalog,
            structureCatalog,
            campaignState.TravelCargo
        );

        return new PrototypeCampaignSession(
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

    private static void AddPrototypeStartingItems(
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
        AddPrototypeStatefulItems(gameState, itemCatalog, firearmCatalog);
    }

    private static void AddPrototypeStatefulItems(
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

    private static ItemCatalog LoadItemCatalog()
    {
        var dataPath = ProjectSettings.GlobalizePath("res://data/items");
        return new ItemDefinitionLoader().LoadDirectory(dataPath);
    }

    private static FirearmCatalog LoadFirearmCatalog()
    {
        var dataPath = ProjectSettings.GlobalizePath("res://data/firearms");
        return new FirearmDefinitionLoader().LoadDirectory(dataPath);
    }

    private static TileSurfaceCatalog LoadSurfaceCatalog()
    {
        var dataPath = ProjectSettings.GlobalizePath("res://data/surfaces");
        return new TileSurfaceDefinitionLoader().LoadDirectory(dataPath);
    }

    private static WorldObjectCatalog LoadWorldObjectCatalog()
    {
        var dataPath = ProjectSettings.GlobalizePath("res://data/world_objects");
        return new WorldObjectDefinitionLoader().LoadDirectory(dataPath);
    }

    private static StructureCatalog LoadStructureCatalog()
    {
        var dataPath = ProjectSettings.GlobalizePath("res://data/structures");
        return new StructureDefinitionLoader().LoadDirectory(dataPath);
    }

    private static NpcCatalog LoadNpcCatalog()
    {
        var dataPath = ProjectSettings.GlobalizePath("res://data/npcs");
        return new NpcDefinitionLoader().LoadDirectory(dataPath);
    }

    private static IReadOnlyList<PrototypeLocalSite> LoadLocalSites(
        TileSurfaceCatalog surfaceCatalog,
        WorldObjectCatalog worldObjectCatalog,
        StructureCatalog structureCatalog,
        ItemCatalog itemCatalog,
        NpcCatalog npcCatalog)
    {
        var dataPath = ProjectSettings.GlobalizePath("res://data/local_maps");
        return new LocalSiteDefinitionLoader().LoadDirectory(
            dataPath,
            surfaceCatalog,
            worldObjectCatalog,
            structureCatalog,
            itemCatalog,
            npcCatalog
        );
    }

}
