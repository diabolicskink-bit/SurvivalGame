namespace SurvivalGame.Domain;

public sealed record WorldMapBounds
{
    public WorldMapBounds(double minLongitude, double maxLongitude, double minLatitude, double maxLatitude)
    {
        if (maxLongitude <= minLongitude)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLongitude), "Maximum longitude must be greater than minimum longitude.");
        }

        if (maxLatitude <= minLatitude)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLatitude), "Maximum latitude must be greater than minimum latitude.");
        }

        MinLongitude = minLongitude;
        MaxLongitude = maxLongitude;
        MinLatitude = minLatitude;
        MaxLatitude = maxLatitude;
    }

    public double MinLongitude { get; }

    public double MaxLongitude { get; }

    public double MinLatitude { get; }

    public double MaxLatitude { get; }
}
