using TheSingularityWorkshop.FSM_Dungeon;
using TheSingularityWorkshop.FSM_Dungeon.Geology;
using Xunit;

namespace TheSingularityWorkshop.FSM_Dungeon.Tests;

public sealed class GeologicalWorldTests
{
    [Fact]
    public void Profiles_bind_to_spatial_regions_and_resolve_material_occurrence()
    {
        var topology = new SpatialTopology(
            [
                new SpatialRegion("cavern-west", "Western Cavern", new(-10, 0, 0), new(10, 8, 12), 0,
                    "Natural dissolution enlarged the chamber."),
                new SpatialRegion("mine-east", "Eastern Mine", new(8, 0, -5), new(8, 4, 10), -5,
                    "A later excavation followed a mineralized zone.")
            ]);

        var limestone = new MaterialReference("materials", "mineral/limestone");
        var hematite = new MaterialReference("materials", "mineral/hematite");
        var world = new GeologicalWorld(topology,
        [
            new GeologicalProfile("west-strata", "cavern-west", "m", "survey-datum",
            [
                new GeologicalLayer("upper", "Upper Limestone", limestone, 6, 0,
                    "carbonate-deposition", "older"),
                new GeologicalLayer("lower", "Iron-bearing Bed", hematite, 0, -4,
                    "hydrothermal-alteration", "younger")
            ])
        ]);

        Assert.Same(topology, world.Topology);
        Assert.Equal("west-strata", world.GetProfile("cavern-west")!.Id);
        Assert.Null(world.GetProfile("mine-east"));
        Assert.Equal("mineral/limestone", world.FindMaterialAt("cavern-west", 3)!.Id);
        Assert.Equal("mineral/hematite", world.FindMaterialAt("cavern-west", -2)!.Id);
        Assert.Null(world.FindMaterialAt("cavern-west", -8));
        Assert.Null(world.FindMaterialAt("mine-east", -5));
    }

    [Fact]
    public void Rejects_profiles_that_reference_unknown_regions()
    {
        var topology = new SpatialTopology(
        [
            new SpatialRegion("known", "Known Region", new(0, 0, 0), new(2, 2, 2), 0,
                "Natural formation.")
        ]);
        var profile = new GeologicalProfile("orphan-profile", "missing", "m", "datum",
        [
            new GeologicalLayer("layer", "Layer", new MaterialReference("materials", "rock/shale"),
                2, 0, "sedimentation", "unknown")
        ]);

        Assert.Throws<ArgumentException>(() => new GeologicalWorld(topology, [profile]));
    }

    [Fact]
    public void Allows_multiple_profiles_for_different_regions_but_not_duplicate_profile_ids()
    {
        var topology = new SpatialTopology(
        [
            new SpatialRegion("a", "A", new(0, 0, 0), new(2, 2, 2), 0, "Natural formation."),
            new SpatialRegion("b", "B", new(3, 0, 0), new(2, 2, 2), 0, "Excavated.")
        ]);
        GeologicalProfile Profile(string region) => new("section-" + region, region, "m", "datum",
        [
            new GeologicalLayer("bed", "Bed", new MaterialReference("materials", "rock/basalt"),
                2, 0, "volcanic emplacement", "unknown")
        ]);

        var world = new GeologicalWorld(topology, [Profile("a"), Profile("b")]);

        Assert.Equal(2, world.Profiles.Count);
        Assert.Throws<ArgumentException>(() => new GeologicalWorld(topology, [Profile("a"), Profile("b") with { }]));
    }
}
