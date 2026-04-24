namespace SurvivalGame.Domain;

public readonly record struct GridPosition(int X, int Y)
{
    public static GridPosition operator +(GridPosition position, GridOffset offset)
    {
        return new GridPosition(position.X + offset.X, position.Y + offset.Y);
    }
}

public readonly record struct GridOffset(int X, int Y)
{
    public static readonly GridOffset Zero = new(0, 0);
    public static readonly GridOffset Up = new(0, -1);
    public static readonly GridOffset Down = new(0, 1);
    public static readonly GridOffset Left = new(-1, 0);
    public static readonly GridOffset Right = new(1, 0);
}

public readonly record struct GridBounds
{
    public GridBounds(int width, int height)
    {
        if (width < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Grid width must be at least 1.");
        }

        if (height < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Grid height must be at least 1.");
        }

        Width = width;
        Height = height;
    }

    public int Width { get; }

    public int Height { get; }

    public GridPosition Center => new(Width / 2, Height / 2);

    public bool Contains(GridPosition position)
    {
        return position.X >= 0
            && position.Y >= 0
            && position.X < Width
            && position.Y < Height;
    }

    public GridPosition Clamp(GridPosition position)
    {
        return new GridPosition(
            Math.Clamp(position.X, 0, Width - 1),
            Math.Clamp(position.Y, 0, Height - 1)
        );
    }
}
