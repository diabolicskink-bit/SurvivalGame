namespace SurvivalGame.Domain;

public static class PrototypeLocalSites
{
    public const string DefaultSiteId = "prototype_local";
    public const string GasStationSiteId = "gas_station";

    public static readonly GridBounds DefaultBounds = new(19, 13);
    public static readonly GridBounds GasStationBounds = new(40, 28);

    public static PrototypeLocalSite CreateDefault(NpcCatalog npcCatalog)
    {
        ArgumentNullException.ThrowIfNull(npcCatalog);

        return new PrototypeLocalSite(
            DefaultSiteId,
            "Prototype Local Site",
            DefaultBounds,
            DefaultBounds.Center,
            CreateDefaultGroundItems(),
            CreateDefaultSurfaceMap(),
            CreateDefaultWorldObjects(),
            CreateDefaultNpcs(npcCatalog)
        );
    }

    public static PrototypeLocalSite CreateGasStation()
    {
        return new PrototypeLocalSite(
            GasStationSiteId,
            "Route 18 Gas Station",
            GasStationBounds,
            new GridPosition(21, 14),
            new TileItemMap(),
            CreateGasStationSurfaceMap(),
            CreateGasStationWorldObjects(),
            new NpcRoster()
        );
    }

    private static TileItemMap CreateDefaultGroundItems()
    {
        var itemMap = new TileItemMap();

        itemMap.Place(new GridPosition(4, 4), PrototypeItems.Stone, 2);
        itemMap.Place(new GridPosition(7, 9), PrototypeItems.Branch, 3);
        itemMap.Place(new GridPosition(13, 5), PrototypeItems.WaterBottle);
        itemMap.Place(new GridPosition(16, 10), PrototypeItems.Ak47);
        itemMap.Place(new GridPosition(8, 7), PrototypeItems.BaseballCap);
        itemMap.Place(new GridPosition(10, 7), PrototypeItems.RunningShoes);

        return itemMap;
    }

    private static TileSurfaceMap CreateDefaultSurfaceMap()
    {
        var surfaceMap = new TileSurfaceMap(DefaultBounds, PrototypeSurfaces.Grass);

        FillRect(surfaceMap, DefaultBounds, x: 2, y: 2, width: 8, height: 5, PrototypeSurfaces.Concrete);
        FillRect(surfaceMap, DefaultBounds, x: 3, y: 3, width: 3, height: 3, PrototypeSurfaces.Carpet);
        FillRect(surfaceMap, DefaultBounds, x: 11, y: 2, width: 5, height: 4, PrototypeSurfaces.Tile);
        FillRect(surfaceMap, DefaultBounds, x: 12, y: 8, width: 4, height: 3, PrototypeSurfaces.Ice);

        return surfaceMap;
    }

    private static TileObjectMap CreateDefaultWorldObjects()
    {
        var objectMap = new TileObjectMap();

        PlaceWallLine(objectMap, y: 2, xStart: 2, xEnd: 9);
        objectMap.Place(new GridPosition(2, 3), PrototypeWorldObjects.Wall);
        objectMap.Place(new GridPosition(2, 4), PrototypeWorldObjects.Wall);
        objectMap.Place(new GridPosition(9, 3), PrototypeWorldObjects.Window);
        objectMap.Place(new GridPosition(9, 4), PrototypeWorldObjects.Wall);
        objectMap.Place(new GridPosition(6, 6), PrototypeWorldObjects.WoodenDoor);
        objectMap.Place(new GridPosition(8, 3), PrototypeWorldObjects.Fridge);
        objectMap.Place(new GridPosition(5, 4), PrototypeWorldObjects.Table);
        objectMap.Place(new GridPosition(5, 5), PrototypeWorldObjects.Chair);
        objectMap.Place(new GridPosition(12, 3), PrototypeWorldObjects.Bed);
        objectMap.Place(new GridPosition(15, 5), PrototypeWorldObjects.StorageCrate);
        objectMap.Place(new GridPosition(1, 10), PrototypeWorldObjects.Tree);
        objectMap.Place(new GridPosition(17, 2), PrototypeWorldObjects.Boulder);

        return objectMap;
    }

    private static NpcRoster CreateDefaultNpcs(NpcCatalog npcCatalog)
    {
        var npcs = new NpcRoster();
        var testDummy = npcCatalog.Get(PrototypeNpcs.TestDummyDefinition);
        npcs.Add(testDummy.CreateState(PrototypeNpcs.TestDummy, new GridPosition(14, 8)));

        return npcs;
    }

    private static TileSurfaceMap CreateGasStationSurfaceMap()
    {
        var surfaceMap = new TileSurfaceMap(GasStationBounds, PrototypeSurfaces.Asphalt);

        FillRect(surfaceMap, GasStationBounds, x: 0, y: 0, width: 40, height: 2, PrototypeSurfaces.Grass);
        FillRect(surfaceMap, GasStationBounds, x: 0, y: 24, width: 40, height: 4, PrototypeSurfaces.Grass);
        FillRect(surfaceMap, GasStationBounds, x: 0, y: 0, width: 2, height: 28, PrototypeSurfaces.Grass);
        FillRect(surfaceMap, GasStationBounds, x: 38, y: 0, width: 2, height: 28, PrototypeSurfaces.Grass);
        FillRect(surfaceMap, GasStationBounds, x: 5, y: 4, width: 15, height: 10, PrototypeSurfaces.Tile);
        FillRect(surfaceMap, GasStationBounds, x: 22, y: 7, width: 11, height: 10, PrototypeSurfaces.Concrete);
        FillRect(surfaceMap, GasStationBounds, x: 20, y: 19, width: 17, height: 4, PrototypeSurfaces.Concrete);

        return surfaceMap;
    }

    private static TileObjectMap CreateGasStationWorldObjects()
    {
        var objectMap = new TileObjectMap();

        PlaceGasStationStoreShell(objectMap);
        objectMap.Place(new GridPosition(12, 13), PrototypeWorldObjects.GlassDoor);
        objectMap.Place(new GridPosition(8, 13), PrototypeWorldObjects.Window);
        objectMap.Place(new GridPosition(16, 13), PrototypeWorldObjects.Window);
        objectMap.Place(new GridPosition(14, 8), PrototypeWorldObjects.Wall);
        objectMap.Place(new GridPosition(15, 8), PrototypeWorldObjects.Wall);
        objectMap.Place(new GridPosition(16, 8), PrototypeWorldObjects.Wall);
        objectMap.Place(new GridPosition(17, 8), PrototypeWorldObjects.Wall);
        objectMap.Place(new GridPosition(18, 8), PrototypeWorldObjects.Wall);

        objectMap.Place(new GridPosition(7, 6), PrototypeWorldObjects.StoreShelf);
        objectMap.Place(new GridPosition(8, 6), PrototypeWorldObjects.StoreShelf);
        objectMap.Place(new GridPosition(11, 6), PrototypeWorldObjects.StoreShelf);
        objectMap.Place(new GridPosition(7, 9), PrototypeWorldObjects.StoreShelf);
        objectMap.Place(new GridPosition(8, 9), PrototypeWorldObjects.StoreShelf);
        objectMap.Place(new GridPosition(11, 9), PrototypeWorldObjects.StoreShelf);
        objectMap.Place(new GridPosition(15, 11), PrototypeWorldObjects.CheckoutCounter);
        objectMap.Place(new GridPosition(16, 11), PrototypeWorldObjects.CheckoutCounter);
        objectMap.Place(new GridPosition(17, 11), PrototypeWorldObjects.RestroomFixture);
        objectMap.Place(new GridPosition(18, 11), PrototypeWorldObjects.TrashBin);

        objectMap.Place(new GridPosition(25, 9), PrototypeWorldObjects.FuelPump);
        objectMap.Place(new GridPosition(28, 9), PrototypeWorldObjects.FuelPump);
        objectMap.Place(new GridPosition(25, 14), PrototypeWorldObjects.FuelPump);
        objectMap.Place(new GridPosition(28, 14), PrototypeWorldObjects.FuelPump);
        objectMap.Place(new GridPosition(23, 7), PrototypeWorldObjects.GasStationCanopyPost);
        objectMap.Place(new GridPosition(31, 7), PrototypeWorldObjects.GasStationCanopyPost);
        objectMap.Place(new GridPosition(23, 16), PrototypeWorldObjects.GasStationCanopyPost);
        objectMap.Place(new GridPosition(31, 16), PrototypeWorldObjects.GasStationCanopyPost);
        objectMap.Place(new GridPosition(35, 4), PrototypeWorldObjects.GasStationSign);

        objectMap.Place(new GridPosition(22, 20), PrototypeWorldObjects.ParkingBollard);
        objectMap.Place(new GridPosition(23, 20), PrototypeWorldObjects.ParkingBollard);
        objectMap.Place(new GridPosition(30, 20), PrototypeWorldObjects.ParkingBollard);
        objectMap.Place(new GridPosition(31, 20), PrototypeWorldObjects.ParkingBollard);
        objectMap.Place(new GridPosition(33, 21), PrototypeWorldObjects.AbandonedVehicle);
        objectMap.Place(new GridPosition(21, 18), PrototypeWorldObjects.TrashBin);

        return objectMap;
    }

    private static void PlaceGasStationStoreShell(TileObjectMap objectMap)
    {
        const int left = 5;
        const int top = 4;
        const int right = 19;
        const int bottom = 13;
        var frontOpenings = new HashSet<int> { 8, 12, 16 };

        for (var x = left; x <= right; x++)
        {
            objectMap.Place(new GridPosition(x, top), PrototypeWorldObjects.Wall);
            if (!frontOpenings.Contains(x))
            {
                objectMap.Place(new GridPosition(x, bottom), PrototypeWorldObjects.Wall);
            }
        }

        for (var y = top + 1; y < bottom; y++)
        {
            objectMap.Place(new GridPosition(left, y), PrototypeWorldObjects.Wall);
            objectMap.Place(new GridPosition(right, y), PrototypeWorldObjects.Wall);
        }
    }

    private static void PlaceWallLine(TileObjectMap objectMap, int y, int xStart, int xEnd)
    {
        for (var x = xStart; x <= xEnd; x++)
        {
            objectMap.Place(new GridPosition(x, y), PrototypeWorldObjects.Wall);
        }
    }

    private static void FillRect(TileSurfaceMap surfaceMap, GridBounds bounds, int x, int y, int width, int height, SurfaceId surfaceId)
    {
        for (var row = y; row < y + height; row++)
        {
            for (var column = x; column < x + width; column++)
            {
                var position = new GridPosition(column, row);
                if (bounds.Contains(position))
                {
                    surfaceMap.SetSurface(position, surfaceId);
                }
            }
        }
    }
}
