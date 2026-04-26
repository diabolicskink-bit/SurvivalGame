namespace SurvivalGame.Domain;

public sealed record SpriteRenderProfile
{
    public SpriteRenderProfile(
        float widthTiles,
        float heightTiles,
        float offsetXTiles = 0f,
        float offsetYTiles = 0f,
        float sortOffsetYTiles = 0f)
    {
        if (widthTiles <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(widthTiles), "Sprite render width must be positive.");
        }

        if (heightTiles <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(heightTiles), "Sprite render height must be positive.");
        }

        WidthTiles = widthTiles;
        HeightTiles = heightTiles;
        OffsetXTiles = offsetXTiles;
        OffsetYTiles = offsetYTiles;
        SortOffsetYTiles = sortOffsetYTiles;
    }

    public float WidthTiles { get; }

    public float HeightTiles { get; }

    public float OffsetXTiles { get; }

    public float OffsetYTiles { get; }

    public float SortOffsetYTiles { get; }
}
