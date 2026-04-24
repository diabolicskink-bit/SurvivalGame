using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class PlayerVitalsTests
{
    [Fact]
    public void PlayerStartsWithBaselineVitals()
    {
        var player = new PlayerState();

        Assert.Equal(100, player.Vitals.Health.Current);
        Assert.Equal(0, player.Vitals.Hunger.Current);
        Assert.Equal(0, player.Vitals.Thirst.Current);
        Assert.Equal(0, player.Vitals.Fatigue.Current);
        Assert.Equal(0, player.Vitals.SleepDebt.Current);
        Assert.Equal(0, player.Vitals.Pain.Current);
        Assert.Equal(37.0f, player.Vitals.BodyTemperatureCelsius);
    }

    [Fact]
    public void VitalsCanTrackIndependentCurrentValues()
    {
        var vitals = new PlayerVitals();

        vitals.SetHealth(75);
        vitals.SetHunger(20);
        vitals.SetThirst(35);
        vitals.SetFatigue(45);
        vitals.SetSleepDebt(55);
        vitals.SetPain(15);
        vitals.SetBodyTemperatureCelsius(38.2f);

        Assert.Equal(75, vitals.Health.Current);
        Assert.Equal(20, vitals.Hunger.Current);
        Assert.Equal(35, vitals.Thirst.Current);
        Assert.Equal(45, vitals.Fatigue.Current);
        Assert.Equal(55, vitals.SleepDebt.Current);
        Assert.Equal(15, vitals.Pain.Current);
        Assert.Equal(38.2f, vitals.BodyTemperatureCelsius);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void MetersRejectValuesOutsideRange(int value)
    {
        var vitals = new PlayerVitals();

        Assert.Throws<ArgumentOutOfRangeException>(() => vitals.SetHunger(value));
    }

    [Theory]
    [InlineData(19.9f)]
    [InlineData(45.1f)]
    public void BodyTemperatureRejectsImplausibleValues(float value)
    {
        var vitals = new PlayerVitals();

        Assert.Throws<ArgumentOutOfRangeException>(() => vitals.SetBodyTemperatureCelsius(value));
    }
}
