using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class NpcStateTests
{
    [Fact]
    public void NpcStateTracksIdentityPositionAndHealth()
    {
        var npc = new NpcState(
            PrototypeNpcs.TestDummy,
            "Test Dummy",
            new GridPosition(2, 3),
            currentHealth: 200,
            maximumHealth: 200
        );

        Assert.Equal(PrototypeNpcs.TestDummy, npc.Id);
        Assert.Equal("Test Dummy", npc.Name);
        Assert.Equal(new GridPosition(2, 3), npc.Position);
        Assert.Equal(200, npc.Health.Current);
        Assert.Equal(200, npc.Health.Maximum);
    }

    [Fact]
    public void NpcRosterTracksOneNpcPerTile()
    {
        var roster = new NpcRoster();
        var position = new GridPosition(2, 3);
        var npc = new NpcState(PrototypeNpcs.TestDummy, "Test Dummy", position, 200, 200);

        roster.Add(npc);

        Assert.True(roster.TryGet(PrototypeNpcs.TestDummy, out var foundNpc));
        Assert.Same(npc, foundNpc);
        Assert.True(roster.TryGetAt(position, out var foundAtPosition));
        Assert.Same(npc, foundAtPosition);
        Assert.Throws<InvalidOperationException>(() =>
            roster.Add(new NpcState(new NpcId("second_dummy"), "Second Dummy", position)));
    }

    [Fact]
    public void NpcRosterMovesNpcAndUpdatesTileIndex()
    {
        var roster = new NpcRoster();
        var npc = new NpcState(PrototypeNpcs.TestDummy, "Test Dummy", new GridPosition(2, 3), 200, 200);

        roster.Add(npc);
        roster.Move(PrototypeNpcs.TestDummy, new GridPosition(3, 3));

        Assert.False(roster.TryGetAt(new GridPosition(2, 3), out _));
        Assert.True(roster.TryGetAt(new GridPosition(3, 3), out var movedNpc));
        Assert.Same(npc, movedNpc);
        Assert.Equal(new GridPosition(3, 3), npc.Position);
    }

    [Fact]
    public void NpcDamageReducesHealthAndClampsAtZero()
    {
        var npc = new NpcState(PrototypeNpcs.TestDummy, "Test Dummy", new GridPosition(2, 3), 200, 200);

        var firstDamage = npc.TakeDamage(45);

        Assert.Equal(45, firstDamage);
        Assert.Equal(155, npc.Health.Current);

        var secondDamage = npc.TakeDamage(500);

        Assert.Equal(155, secondDamage);
        Assert.Equal(0, npc.Health.Current);
        Assert.True(npc.IsDisabled);
    }
}
