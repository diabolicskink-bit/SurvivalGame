namespace SurvivalGame.Domain;

public static class PrototypeWorldMapSites
{
    public const double MapWidth = 2100.0;
    public const double MapHeight = 1300.0;
    public const double VisibleWidth = 1200.0;
    public const double VisibleHeight = 760.0;

    public static readonly WorldMapPosition StartPosition = new(260.0, 470.0);

    public static IReadOnlyList<WorldMapPointOfInterest> All { get; } = new[]
    {
        new WorldMapPointOfInterest(
            "farmstead",
            "Abandoned Farmstead",
            new WorldMapPosition(230.0, 320.0),
            enterRadius: 42.0
        ),
        new WorldMapPointOfInterest(
            "roadside_store",
            "Roadside Store",
            new WorldMapPosition(780.0, 650.0),
            enterRadius: 42.0
        ),
        new WorldMapPointOfInterest(
            "radio_tower",
            "Radio Tower",
            new WorldMapPosition(980.0, 245.0),
            enterRadius: 42.0
        ),
        new WorldMapPointOfInterest(
            PrototypeLocalSites.GasStationSiteId.Value,
            "Route 18 Gas Station",
            new WorldMapPosition(420.0, 560.0),
            enterRadius: 46.0
        ),
        new WorldMapPointOfInterest(
            "old_quarry",
            "Old Quarry",
            new WorldMapPosition(1440.0, 300.0),
            enterRadius: 42.0
        ),
        new WorldMapPointOfInterest(
            "water_yard",
            "Water Treatment Yard",
            new WorldMapPosition(1660.0, 520.0),
            enterRadius: 42.0
        ),
        new WorldMapPointOfInterest(
            "ranger_cabin",
            "Ranger Cabin",
            new WorldMapPosition(1880.0, 870.0),
            enterRadius: 42.0
        ),
        new WorldMapPointOfInterest(
            "rail_siding",
            "Rail Siding",
            new WorldMapPosition(1260.0, 950.0),
            enterRadius: 42.0
        ),
        new WorldMapPointOfInterest(
            "collapsed_checkpoint",
            "Collapsed Checkpoint",
            new WorldMapPosition(1040.0, 1120.0),
            enterRadius: 42.0
        ),
        new WorldMapPointOfInterest(
            "orchard_camp",
            "Orchard Camp",
            new WorldMapPosition(560.0, 1010.0),
            enterRadius: 42.0
        ),
        new WorldMapPointOfInterest(
            "service_depot",
            "Service Depot",
            new WorldMapPosition(1620.0, 1140.0),
            enterRadius: 42.0
        ),
        new WorldMapPointOfInterest(
            "hilltop_substation",
            "Hilltop Substation",
            new WorldMapPosition(1960.0, 220.0),
            enterRadius: 42.0
        )
    };
}
