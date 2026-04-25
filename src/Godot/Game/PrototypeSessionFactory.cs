using Godot;
using SurvivalGame.Domain;

public sealed class PrototypeGameplaySession
{
    public PrototypeGameplaySession(
        PrototypeGameState gameState,
        string siteDisplayName,
        GridPosition entryPosition,
        ItemCatalog itemCatalog,
        FirearmCatalog firearmCatalog,
        TileSurfaceCatalog surfaceCatalog,
        WorldObjectCatalog worldObjectCatalog,
        NpcCatalog npcCatalog,
        GameActionPipeline actionPipeline
    )
    {
        GameState = gameState;
        SiteDisplayName = siteDisplayName;
        EntryPosition = entryPosition;
        ItemCatalog = itemCatalog;
        FirearmCatalog = firearmCatalog;
        SurfaceCatalog = surfaceCatalog;
        WorldObjectCatalog = worldObjectCatalog;
        NpcCatalog = npcCatalog;
        ActionPipeline = actionPipeline;
    }

    public PrototypeGameState GameState { get; }

    public string SiteDisplayName { get; }

    public GridPosition EntryPosition { get; }

    public ItemCatalog ItemCatalog { get; }

    public FirearmCatalog FirearmCatalog { get; }

    public TileSurfaceCatalog SurfaceCatalog { get; }

    public WorldObjectCatalog WorldObjectCatalog { get; }

    public NpcCatalog NpcCatalog { get; }

    public GameActionPipeline ActionPipeline { get; }
}

public static class PrototypeSessionFactory
{
    public static PrototypeGameplaySession CreateGameplaySession(VehicleFuelState? vehicleFuelState = null)
    {
        var itemCatalog = LoadItemCatalog();
        var firearmCatalog = LoadFirearmCatalog();
        var surfaceCatalog = LoadSurfaceCatalog();
        var worldObjectCatalog = LoadWorldObjectCatalog();
        var npcCatalog = LoadNpcCatalog();
        var actionPipeline = new GameActionPipeline(itemCatalog, worldObjectCatalog, firearmCatalog, vehicleFuelState);
        var localSite = PrototypeLocalSites.CreateDefault(npcCatalog);
        var gameState = CreateGameState(localSite);

        AddPrototypeStartingItems(gameState, firearmCatalog);

        return new PrototypeGameplaySession(
            gameState,
            localSite.DisplayName,
            localSite.StartPosition,
            itemCatalog,
            firearmCatalog,
            surfaceCatalog,
            worldObjectCatalog,
            npcCatalog,
            actionPipeline
        );
    }

    public static PrototypeGameplaySession CreateGasStationSession(
        PrototypeGameplaySession sharedSession,
        VehicleFuelState vehicleFuelState)
    {
        var gasStation = PrototypeLocalSites.CreateGasStation();
        var gameState = CreateGameState(
            gasStation,
            sharedSession.GameState.Player,
            sharedSession.GameState.Time,
            sharedSession.GameState.StatefulItems
        );

        return new PrototypeGameplaySession(
            gameState,
            gasStation.DisplayName,
            gasStation.StartPosition,
            sharedSession.ItemCatalog,
            sharedSession.FirearmCatalog,
            sharedSession.SurfaceCatalog,
            sharedSession.WorldObjectCatalog,
            sharedSession.NpcCatalog,
            new GameActionPipeline(
                sharedSession.ItemCatalog,
                sharedSession.WorldObjectCatalog,
                sharedSession.FirearmCatalog,
                vehicleFuelState
            )
        );
    }

    private static PrototypeGameState CreateGameState(
        PrototypeLocalSite site,
        PlayerState? player = null,
        WorldTime? time = null,
        StatefulItemStore? statefulItems = null)
    {
        return new PrototypeGameState(
            new WorldState(
                new MapState(site.Bounds, site.Surfaces),
                site.GroundItems,
                site.WorldObjects,
                site.Npcs
            ),
            site.StartPosition,
            player ?? new PlayerState(),
            time ?? new WorldTime(),
            statefulItems ?? new StatefulItemStore(),
            site.Id
        );
    }

    private static void AddPrototypeStartingItems(PrototypeGameState gameState, FirearmCatalog firearmCatalog)
    {
        gameState.Player.Inventory.Add(PrototypeItems.Stone, 3);
        gameState.Player.Inventory.Add(PrototypeItems.Branch, 2);
        gameState.Player.Inventory.Add(PrototypeItems.WaterBottle);
        gameState.Player.Inventory.Add(PrototypeFirearms.Ammo9mmStandard, 35);
        gameState.Player.Inventory.Add(PrototypeFirearms.Ammo9mmHollowPoint, 20);
        gameState.Player.Inventory.Add(PrototypeFirearms.Ammo762x39Standard, 60);
        gameState.Player.Inventory.Add(PrototypeFirearms.Ammo308Standard, 20);
        gameState.Player.Inventory.Add(PrototypeFirearms.Ammo12GaugeBuckshot, 20);
        gameState.Player.Inventory.Add(PrototypeFirearms.Ammo12GaugeSlug, 10);
        gameState.Player.Inventory.Add(PrototypeFirearms.Ammo22LrStandard, 100);
        AddPrototypeStatefulItems(gameState, firearmCatalog);
    }

    private static void AddPrototypeStatefulItems(PrototypeGameState gameState, FirearmCatalog firearmCatalog)
    {
        gameState.StatefulItems.Create(PrototypeFirearms.Pistol9mm, 1, StatefulItemLocation.PlayerInventory(), firearmCatalog);
        gameState.StatefulItems.Create(PrototypeItems.Ak47, 1, StatefulItemLocation.PlayerInventory(), firearmCatalog);
        gameState.StatefulItems.Create(PrototypeItems.HuntingRifle, 1, StatefulItemLocation.PlayerInventory(), firearmCatalog);
        gameState.StatefulItems.Create(PrototypeFirearms.Shotgun12Gauge, 1, StatefulItemLocation.PlayerInventory(), firearmCatalog);
        gameState.StatefulItems.Create(PrototypeFirearms.Rifle22, 1, StatefulItemLocation.PlayerInventory(), firearmCatalog);

        var loadedMagazine = gameState.StatefulItems.Create(
            PrototypeFirearms.Magazine9mmStandard,
            1,
            StatefulItemLocation.PlayerInventory(),
            firearmCatalog
        );
        loadedMagazine.FeedDevice?.Load(firearmCatalog.GetAmmunition(PrototypeFirearms.Ammo9mmStandard), 15);

        gameState.StatefulItems.Create(PrototypeFirearms.Magazine9mmStandard, 1, StatefulItemLocation.PlayerInventory(), firearmCatalog);
        gameState.StatefulItems.Create(PrototypeFirearms.Magazine9mmExtended, 1, StatefulItemLocation.PlayerInventory(), firearmCatalog);
        gameState.StatefulItems.Create(PrototypeFirearms.MagazineAk30Round, 1, StatefulItemLocation.PlayerInventory(), firearmCatalog);
        gameState.StatefulItems.Create(PrototypeFirearms.MagazineAkDamaged20Round, 1, StatefulItemLocation.PlayerInventory(), firearmCatalog);

        var droppedMagazine = gameState.StatefulItems.Create(
            PrototypeFirearms.Magazine9mmStandard,
            1,
            StatefulItemLocation.Ground(new GridPosition(11, 7), gameState.SiteId),
            firearmCatalog
        );
        droppedMagazine.FeedDevice?.Load(firearmCatalog.GetAmmunition(PrototypeFirearms.Ammo9mmHollowPoint), 8);

        var backpack = gameState.StatefulItems.Create(
            new ItemId("school_backpack"),
            1,
            StatefulItemLocation.PlayerInventory(),
            firearmCatalog
        );
        var food = gameState.StatefulItems.Create(
            new ItemId("canned_beans"),
            1,
            StatefulItemLocation.PlayerInventory(),
            firearmCatalog
        );
        gameState.StatefulItems.MoveToContained(food.Id, backpack.Id);
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

    private static NpcCatalog LoadNpcCatalog()
    {
        var dataPath = ProjectSettings.GlobalizePath("res://data/npcs");
        return new NpcDefinitionLoader().LoadDirectory(dataPath);
    }
}
