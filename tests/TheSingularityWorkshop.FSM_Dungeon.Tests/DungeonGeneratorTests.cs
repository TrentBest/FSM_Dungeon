using TheSingularityWorkshop.FSM_Dungeon;

namespace TheSingularityWorkshop.FSM_Dungeon.Tests;

public sealed class DungeonGeneratorTests
{
    private static DungeonDefinition CreateDefinition(ulong seed = 42) =>
        new(
            "catacombs-under-the-city",
            "The Old Catacombs",
            seed,
            new DungeonOrigin(
                "constructed-excavation",
                "Workers cut passages beneath the first settlement.",
                "The original excavation became a refuge and later a guarded route.",
                "The lower chambers predate the current town; later groups occupied and modified them."),
            12,
            [
                new InhabitantGroupDefinition(
                    "gate-wardens",
                    "Gate Wardens",
                    "They were ordered to keep unauthorized travelers away from a sealed lower route.",
                    "Patrolling the approach and checking the sealed passage.",
                    2),
                new InhabitantGroupDefinition(
                    "cave-scavengers",
                    "Cave Scavengers",
                    "They shelter here and gather usable material from abandoned chambers.",
                    "Sorting salvage near their shelter.",
                    8)
            ]);

    [Fact]
    public void Same_definition_produces_identical_world()
    {
        var first = DungeonGenerator.Generate(CreateDefinition());
        var second = DungeonGenerator.Generate(CreateDefinition());

        Assert.Equal(first.Chambers, second.Chambers);
        Assert.Equal(first.Passages, second.Passages);
        Assert.Equal(first.InhabitantGroups, second.InhabitantGroups);
    }

    [Fact]
    public void Different_seed_changes_generated_structure()
    {
        var first = DungeonGenerator.Generate(CreateDefinition(42));
        var second = DungeonGenerator.Generate(CreateDefinition(43));

        Assert.NotEqual(first.Chambers, second.Chambers);
    }

    [Fact]
    public void Generated_world_is_connected_and_retains_provenance()
    {
        var world = DungeonGenerator.Generate(CreateDefinition());

        Assert.True(world.IsConnected());
        Assert.Empty(world.Validate());
        Assert.All(world.Chambers, chamber =>
        {
            Assert.Contains("constructed-excavation", chamber.FormationReason);
            Assert.Contains("predate the current town", chamber.HistoricalUse);
            Assert.Contains("guarded route", chamber.CurrentPurpose);
        });
    }

    [Fact]
    public void Inhabitant_groups_have_explicit_motivations_activities_and_territory()
    {
        var world = DungeonGenerator.Generate(CreateDefinition());

        Assert.Equal(2, world.InhabitantGroups.Count);
        Assert.All(world.InhabitantGroups, group =>
        {
            Assert.False(string.IsNullOrWhiteSpace(group.Motivation));
            Assert.False(string.IsNullOrWhiteSpace(group.CurrentActivity));
            Assert.Contains(world.Chambers, chamber => chamber.Index == group.HomeChamberIndex);
        });
    }
}