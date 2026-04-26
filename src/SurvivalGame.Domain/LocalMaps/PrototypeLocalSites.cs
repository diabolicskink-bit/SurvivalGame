namespace SurvivalGame.Domain;

public static class PrototypeLocalSites
{
    public const string DefaultSiteId = "prototype_local";
    public const string GasStationSiteId = "gas_station";

    public static readonly GridBounds DefaultBounds = new(19, 13);
    public static readonly GridBounds GasStationBounds = new(40, 28);
}
