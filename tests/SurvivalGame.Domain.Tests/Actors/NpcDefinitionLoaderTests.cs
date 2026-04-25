using SurvivalGame.Domain;
using Xunit;

namespace SurvivalGame.Domain.Tests;

public sealed class NpcDefinitionLoaderTests
{
    [Fact]
    public void NpcDataLoadsPrototypeDefinitions()
    {
        var catalog = new NpcDefinitionLoader().LoadDirectory(GetNpcDataPath());

        Assert.Equal(6, catalog.Definitions.Count);
        Assert.Equal("Test Dummy", catalog.Get(PrototypeNpcs.TestDummyDefinition).DisplayName);
        Assert.Equal("Cautious Survivor", catalog.Get(PrototypeNpcs.CautiousSurvivor).DisplayName);
        Assert.Equal("Wandering Scavenger", catalog.Get(PrototypeNpcs.WanderingScavenger).DisplayName);
        Assert.Equal("Injured Traveller", catalog.Get(PrototypeNpcs.InjuredTraveller).DisplayName);
        Assert.Equal("Quiet Mechanic", catalog.Get(PrototypeNpcs.QuietMechanic).DisplayName);
        Assert.Equal("Field Researcher", catalog.Get(PrototypeNpcs.FieldResearcher).DisplayName);
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
        Assert.Equal(NpcBehaviorKind.Inert, definition.Behavior.Kind);
        Assert.True(definition.HasTag("target"));
        Assert.True(definition.Behavior.HasTag("does_not_act"));
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
