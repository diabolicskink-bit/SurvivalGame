namespace SurvivalGame.Domain;

public sealed record WorldMapTravelResult(
    bool Moved,
    bool Arrived,
    bool FuelDepleted,
    int ElapsedTicks,
    IReadOnlyList<string> Messages
)
{
    public static WorldMapTravelResult Idle { get; } = new(
        Moved: false,
        Arrived: false,
        FuelDepleted: false,
        ElapsedTicks: 0,
        Messages: Array.Empty<string>()
    );
}
