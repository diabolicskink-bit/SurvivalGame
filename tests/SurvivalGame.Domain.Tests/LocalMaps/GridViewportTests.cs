using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class GridViewportTests
{
    [Fact]
    public void LargerMapCentersOnFocusWhenThereIsRoom()
    {
        var viewport = GridViewport.Create(
            new GridBounds(40, 28),
            new GridPosition(21, 14),
            width: 27,
            height: 18
        );

        Assert.Equal(new GridPosition(8, 5), viewport.Origin);
        Assert.Equal(new GridPosition(13, 9), viewport.CenterCell);
        Assert.True(viewport.TryMapToViewport(new GridPosition(21, 14), out var viewportPosition));
        Assert.Equal(new GridPosition(13, 9), viewportPosition);
    }

    [Fact]
    public void LargerMapClampsNearTopLeftEdge()
    {
        var viewport = GridViewport.Create(
            new GridBounds(40, 28),
            new GridPosition(2, 3),
            width: 27,
            height: 18
        );

        Assert.Equal(new GridPosition(0, 0), viewport.Origin);
        Assert.True(viewport.TryMapToViewport(new GridPosition(2, 3), out var viewportPosition));
        Assert.Equal(new GridPosition(2, 3), viewportPosition);
    }

    [Fact]
    public void LargerMapClampsNearBottomRightEdge()
    {
        var viewport = GridViewport.Create(
            new GridBounds(40, 28),
            new GridPosition(39, 27),
            width: 27,
            height: 18
        );

        Assert.Equal(new GridPosition(13, 10), viewport.Origin);
        Assert.True(viewport.TryMapToViewport(new GridPosition(39, 27), out var viewportPosition));
        Assert.Equal(new GridPosition(26, 17), viewportPosition);
    }

    [Fact]
    public void SmallerMapKeepsFixedViewportWithPadding()
    {
        var viewport = GridViewport.Create(
            new GridBounds(19, 13),
            new GridPosition(9, 6),
            width: 27,
            height: 18
        );

        Assert.Equal(new GridPosition(-4, -2), viewport.Origin);
        Assert.Equal(27, viewport.Width);
        Assert.Equal(18, viewport.Height);
        Assert.True(viewport.TryMapToViewport(new GridPosition(0, 0), out var topLeft));
        Assert.Equal(new GridPosition(4, 2), topLeft);
        Assert.True(viewport.TryMapToViewport(new GridPosition(18, 12), out var bottomRight));
        Assert.Equal(new GridPosition(22, 14), bottomRight);
    }

    [Fact]
    public void ViewportToMapRejectsPaddingOutsideMap()
    {
        var viewport = GridViewport.Create(
            new GridBounds(19, 13),
            new GridPosition(9, 6),
            width: 27,
            height: 18
        );

        Assert.False(viewport.TryViewportToMap(new GridPosition(0, 0), out _));
        Assert.False(viewport.TryViewportToMap(new GridPosition(26, 17), out _));
        Assert.True(viewport.TryViewportToMap(new GridPosition(4, 2), out var mapPosition));
        Assert.Equal(new GridPosition(0, 0), mapPosition);
        Assert.True(viewport.TryViewportToMap(new GridPosition(22, 14), out var farMapPosition));
        Assert.Equal(new GridPosition(18, 12), farMapPosition);
    }
}
