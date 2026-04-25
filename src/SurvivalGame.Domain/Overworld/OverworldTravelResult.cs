namespace SurvivalGame.Domain;

public sealed record OverworldTravelResult(
    bool Moved,
    bool Arrived,
    bool FuelDepleted,
    int ElapsedTicks,
    IReadOnlyList<string> Messages
)
{
    public static OverworldTravelResult Idle { get; } = new(
        Moved: false,
        Arrived: false,
        FuelDepleted: false,
        ElapsedTicks: 0,
        Messages: Array.Empty<string>()
    );
}
