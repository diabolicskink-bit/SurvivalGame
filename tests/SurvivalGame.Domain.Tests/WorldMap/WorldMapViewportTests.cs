using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class WorldMapViewportTests
{
    [Fact]
    public void CentersOnFocusWhenAwayFromMapEdges()
    {
        var viewport = WorldMapViewport.Create(
            mapWidth: 2100,
            mapHeight: 1300,
            visibleWidth: 1200,
            visibleHeight: 760,
            focus: new WorldMapPosition(1100, 700)
        );

        Assert.Equal(new WorldMapPosition(500, 320), viewport.Origin);
        Assert.Equal(1200, viewport.Width);
        Assert.Equal(760, viewport.Height);
    }

    [Fact]
    public void ClampsToLeftAndTopNearMapStart()
    {
        var viewport = WorldMapViewport.Create(
            mapWidth: 2100,
            mapHeight: 1300,
            visibleWidth: 1200,
            visibleHeight: 760,
            focus: new WorldMapPosition(260, 470)
        );

        Assert.Equal(new WorldMapPosition(0, 90), viewport.Origin);
    }

    [Fact]
    public void ClampsToRightAndBottomNearFarMapEdges()
    {
        var viewport = WorldMapViewport.Create(
            mapWidth: 2100,
            mapHeight: 1300,
            visibleWidth: 1200,
            visibleHeight: 760,
            focus: new WorldMapPosition(2050, 1260)
        );

        Assert.Equal(new WorldMapPosition(900, 540), viewport.Origin);
    }

    [Fact]
    public void ConvertsBetweenFullMapAndViewportCoordinates()
    {
        var viewport = WorldMapViewport.Create(
            mapWidth: 2100,
            mapHeight: 1300,
            visibleWidth: 1200,
            visibleHeight: 760,
            focus: new WorldMapPosition(1100, 700)
        );

        var viewportPosition = viewport.MapToViewport(new WorldMapPosition(650, 500));
        var mapPosition = viewport.ViewportToMap(viewportPosition);

        Assert.Equal(new WorldMapPosition(150, 180), viewportPosition);
        Assert.Equal(new WorldMapPosition(650, 500), mapPosition);
    }

    [Fact]
    public void ReportsWhetherMapPositionsAreVisible()
    {
        var viewport = WorldMapViewport.Create(
            mapWidth: 2100,
            mapHeight: 1300,
            visibleWidth: 1200,
            visibleHeight: 760,
            focus: new WorldMapPosition(1100, 700)
        );

        Assert.True(viewport.Contains(new WorldMapPosition(650, 500)));
        Assert.False(viewport.Contains(new WorldMapPosition(50, 500)));
        Assert.False(viewport.Contains(new WorldMapPosition(650, 50)));
    }
}
