using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class NpcDefinitionLoaderTests
{
    [Fact]
    public void NpcDataLoadsPrototypeDefinitions()
    {
        var catalog = new NpcDefinitionLoader().LoadDirectory(GetNpcDataPath());

        Assert.Equal(7, catalog.Definitions.Count);
        Assert.Equal("Test Dummy", catalog.Get(PrototypeNpcs.TestDummyDefinition).DisplayName);
        Assert.Null(catalog.Get(PrototypeNpcs.TestDummyDefinition).SpriteRender);
        Assert.Equal("Cautious Survivor", catalog.Get(PrototypeNpcs.CautiousSurvivor).DisplayName);
        Assert.Equal("Wandering Scavenger", catalog.Get(PrototypeNpcs.WanderingScavenger).DisplayName);
        Assert.Equal("Injured Traveller", catalog.Get(PrototypeNpcs.InjuredTraveller).DisplayName);
        Assert.Equal("Quiet Mechanic", catalog.Get(PrototypeNpcs.QuietMechanic).DisplayName);
        Assert.Equal("Field Researcher", catalog.Get(PrototypeNpcs.FieldResearcher).DisplayName);
        Assert.Equal("Automated Turret", catalog.Get(PrototypeNpcs.AutomatedTurretDefinition).DisplayName);
        Assert.Equal("npc_automated_turret", catalog.Get(PrototypeNpcs.AutomatedTurretDefinition).SpriteId);
        Assert.Equal(1.5f, catalog.Get(PrototypeNpcs.AutomatedTurretDefinition).SpriteRender!.WidthTiles, precision: 3);
        Assert.Equal(0.25f, catalog.Get(PrototypeNpcs.AutomatedTurretDefinition).SpriteRender!.OffsetXTiles, precision: 3);
        Assert.Equal(NpcBehaviorKind.Wander, catalog.Get(PrototypeNpcs.WanderingScavenger).Behavior.Kind);
    }

    [Fact]
    public void LoadsNpcDefinitionsFromJson()
    {
        var directoryPath = CreateTemporaryDirectory();
        var filePath = Path.Combine(directoryPath, "npcs.json");
        File.WriteAllText(filePath, """
        [
          {
            "id": "test_dummy",
            "name": "Test Dummy",
            "description": "A stationary target.",
            "species": "Training Dummy",
            "tags": ["prototype", "target"],
            "maximumHealth": 200,
            "blocksMovement": true,
            "mapColor": "#c75a3b",
            "spriteId": "npc_test_dummy",
            "spriteRender": {
              "widthTiles": 1.25,
              "heightTiles": 1.5,
              "offsetXTiles": 0.1,
              "offsetYTiles": -0.2,
              "sortOffsetYTiles": 0.4
            },
            "behavior": {
              "kind": "Inert",
              "perceptionRange": 0,
              "tags": ["does_not_act"]
            }
          }
        ]
        """);

        var catalog = new NpcDefinitionLoader().LoadDirectory(directoryPath);
        var definition = catalog.Get(new NpcDefinitionId("test_dummy"));

        Assert.Equal("Test Dummy", definition.DisplayName);
        Assert.Equal("Training Dummy", definition.Species);
        Assert.Equal(200, definition.MaximumHealth);
        Assert.True(definition.BlocksMovement);
        Assert.Equal("npc_test_dummy", definition.SpriteId);
        Assert.NotNull(definition.SpriteRender);
        Assert.Equal(1.25f, definition.SpriteRender.WidthTiles, precision: 3);
        Assert.Equal(1.5f, definition.SpriteRender.HeightTiles, precision: 3);
        Assert.Equal(0.1f, definition.SpriteRender.OffsetXTiles, precision: 3);
        Assert.Equal(-0.2f, definition.SpriteRender.OffsetYTiles, precision: 3);
        Assert.Equal(0.4f, definition.SpriteRender.SortOffsetYTiles, precision: 3);
        Assert.Equal(NpcBehaviorKind.Inert, definition.Behavior.Kind);
        Assert.True(definition.HasTag("target"));
        Assert.True(definition.Behavior.HasTag("does_not_act"));
    }

    [Fact]
    public void NpcDefinitionSpriteRenderRejectsNonPositiveSize()
    {
        var directoryPath = CreateTemporaryDirectory();
        var filePath = Path.Combine(directoryPath, "npcs.json");
        File.WriteAllText(filePath, """
        [
          {
            "id": "bad_sprite",
            "name": "Bad Sprite",
            "species": "Machine",
            "maximumHealth": 1,
            "spriteRender": {
              "widthTiles": 0,
              "heightTiles": 1
            }
          }
        ]
        """);

        Assert.Throws<ArgumentOutOfRangeException>(() => new NpcDefinitionLoader().LoadDirectory(directoryPath));
    }

    [Fact]
    public void NpcCatalogRejectsDuplicateDefinitions()
    {
        var catalog = new NpcCatalog();
        var definition = new NpcDefinition(
            new NpcDefinitionId("test_dummy"),
            "Test Dummy",
            string.Empty,
            "Training Dummy",
            maximumHealth: 200
        );

        catalog.Add(definition);

        Assert.Throws<InvalidOperationException>(() => catalog.Add(definition));
    }

    private static string CreateTemporaryDirectory()
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), "SurvivalGameNpcTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);
        return directoryPath;
    }

    private static string GetNpcDataPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var npcDataPath = Path.Combine(directory.FullName, "data", "npcs");
            if (Directory.Exists(npcDataPath))
            {
                return npcDataPath;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate data/npcs from the test output directory.");
    }
}
