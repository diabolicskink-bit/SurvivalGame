namespace SurvivalGame.Domain;

public static class PrototypeOverworldSites
{
    public const double MapWidth = 1200.0;
    public const double MapHeight = 760.0;

    public static readonly OverworldPosition StartPosition = new(260.0, 470.0);

    public static IReadOnlyList<OverworldPointOfInterest> All { get; } = new[]
    {
        new OverworldPointOfInterest(
            "farmstead",
            "Abandoned Farmstead",
            new OverworldPosition(420.0, 360.0),
            enterRadius: 42.0
        ),
        new OverworldPointOfInterest(
            "roadside_store",
            "Roadside Store",
            new OverworldPosition(760.0, 460.0),
            enterRadius: 42.0
        ),
        new OverworldPointOfInterest(
            "radio_tower",
            "Radio Tower",
            new OverworldPosition(940.0, 260.0),
            enterRadius: 42.0
        ),
        new OverworldPointOfInterest(
            PrototypeLocalSites.GasStationSiteId,
            "Route 18 Gas Station",
            new OverworldPosition(610.0, 575.0),
            enterRadius: 46.0
        )
    };
}
