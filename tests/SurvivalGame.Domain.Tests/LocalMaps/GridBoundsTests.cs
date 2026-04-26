using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class GridBoundsTests
{
    [Fact]
    public void CenterUsesIntegerGridCenter()
    {
        var bounds = new GridBounds(19, 13);

        Assert.Equal(new GridPosition(9, 6), bounds.Center);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(18, 12)]
    [InlineData(9, 6)]
    public void ContainsReturnsTrueForPositionsInsideBounds(int x, int y)
    {
        var bounds = new GridBounds(19, 13);

        Assert.True(bounds.Contains(new GridPosition(x, y)));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(19, 0)]
    [InlineData(0, 13)]
    public void ContainsReturnsFalseForPositionsOutsideBounds(int x, int y)
    {
        var bounds = new GridBounds(19, 13);

        Assert.False(bounds.Contains(new GridPosition(x, y)));
    }

    [Fact]
    public void ClampKeepsPositionsInsideBounds()
    {
        var bounds = new GridBounds(19, 13);

        Assert.Equal(new GridPosition(0, 12), bounds.Clamp(new GridPosition(-4, 20)));
    }
}
