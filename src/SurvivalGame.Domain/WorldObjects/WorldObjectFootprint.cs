namespace SurvivalGame.Domain;

public readonly record struct WorldObjectFootprint
{
    public static WorldObjectFootprint SingleTile { get; } = new(1, 1);

    public WorldObjectFootprint(int width, int height)
    {
        if (width < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "World object footprint width must be at least 1.");
        }

        if (height < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "World object footprint height must be at least 1.");
        }

        Width = width;
        Height = height;
    }

    public int Width { get; }

    public int Height { get; }

    public WorldObjectFootprint Rotated(WorldObjectFacing facing)
    {
        return facing is WorldObjectFacing.East or WorldObjectFacing.West
            ? new WorldObjectFootprint(Height, Width)
            : this;
    }

    public IEnumerable<GridPosition> PositionsFrom(GridPosition anchor)
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                yield return new GridPosition(anchor.X + x, anchor.Y + y);
            }
        }
    }
}
