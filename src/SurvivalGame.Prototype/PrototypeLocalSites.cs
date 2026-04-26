namespace SurvivalGame.Domain;

public static class PrototypeLocalSites
{
    public static readonly SiteId DefaultSiteId = SiteId.Default;
    public static readonly SiteId GasStationSiteId = new("gas_station");
    public static readonly SiteId FarmsteadSiteId = new("farmstead");

    public static readonly GridBounds DefaultBounds = new(19, 13);
    public static readonly GridBounds GasStationBounds = new(40, 28);
    public static readonly GridBounds FarmsteadBounds = new(64, 44);
}
