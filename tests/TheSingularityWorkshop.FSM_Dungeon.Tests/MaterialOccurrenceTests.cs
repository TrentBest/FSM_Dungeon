using TheSingularityWorkshop.FSM_Dungeon;
using TheSingularityWorkshop.FSM_Dungeon.Geology;
using Xunit;

namespace TheSingularityWorkshop.FSM_Dungeon.Tests;

public sealed class MaterialOccurrenceTests
{
    [Fact]
    public void Occurrences_are_bounded_to_regions_and_keep_observations_separate_from_material_identity()
    {
        var topology = Topology();
        var hematite = new MaterialReference("materials", "mineral/hematite");
        var first = new MaterialOccurrence(
            "vein-north", "Northern Vein", "vein", "cavern",
            hematite, new(new(1, 1, 1), new(3, 3, 3)), "Hydrothermal fracture filling.",
            [new MaterialGradeObservation(4.2, "%", "mass", "assay", 0.9)],
            [new MaterialQuantityEstimate(120, "tonnes", "mapped volume estimate", 0.65)]);
        var second = new MaterialOccurrence(
            "vein-south", "Southern Vein", "vein", "cavern",
            hematite, new(new(5, 1, 1), new(7, 3, 3)), "A separate mineralized fracture.");

        var world = new GeologicalWorld(topology, [Profile()], [first, second]);

        Assert.Equal(2, world.FindOccurrences("cavern").Count);
        Assert.Equal(2, world.FindOccurrences("cavern", hematite).Count);
        Assert.Equal(1, world.FindOccurrences("cavern")[0].GradeObservations.Count);
        Assert.Equal(1, world.FindOccurrences("cavern")[0].QuantityEstimates.Count);
        Assert.Empty(second.GradeObservations);
        Assert.Empty(second.QuantityEstimates);
        Assert.Equal("mineral/hematite", first.Material.Id);
    }

    [Fact]
    public void Point_queries_return_only_containing_occurrences_in_stable_world_order()
    {
        var material = new MaterialReference("materials", "mineral/quartz");
        var first = new MaterialOccurrence("first", "First", "vein", "cavern", material,
            new(new(1, 1, 1), new(4, 4, 4)), "First fracture.");
        var overlap = new MaterialOccurrence("overlap", "Overlap", "lens", "cavern", material,
            new(new(2, 2, 2), new(5, 5, 5)), "Cross-cutting lens.");
        var outside = new MaterialOccurrence("outside-point", "Other", "pocket", "cavern", material,
            new(new(6, 6, 6), new(8, 8, 8)), "Separate pocket.");
        var world = new GeologicalWorld(Topology(), [Profile()], [first, overlap, outside]);

        var found = world.FindOccurrencesAt("cavern", new(3, 3, 3));

        Assert.Equal(new[] { "first", "overlap" }, found.Select(item => item.Id));
        Assert.Empty(world.FindOccurrencesAt("cavern", new(5.5, 5.5, 5.5)));
        Assert.Throws<ArgumentOutOfRangeException>(() => world.FindOccurrencesAt("cavern", new(11, 1, 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => world.FindOccurrencesAt("cavern", new(double.NaN, 1, 1)));
        Assert.Throws<KeyNotFoundException>(() => world.FindOccurrencesAt("missing", new(1, 1, 1)));
    }

    [Fact]
    public void Bounds_queries_find_intersections_and_touching_boundaries()
    {
        var material = new MaterialReference("materials", "mineral/quartz");
        var occurrence = new MaterialOccurrence("vein", "Vein", "vein", "cavern", material,
            new(new(1, 1, 1), new(3, 3, 3)), "Fracture filling.");
        var world = new GeologicalWorld(Topology(), [Profile()], [occurrence]);

        Assert.Single(world.FindOccurrencesIntersecting("cavern", new(new(3, 2, 2), new(4, 4, 4))));
        Assert.Empty(world.FindOccurrencesIntersecting("cavern", new(new(4, 4, 4), new(5, 5, 5))));
        Assert.Throws<ArgumentException>(() => world.FindOccurrencesIntersecting("cavern", new(new(9, 9, 9), new(11, 10, 10))));
    }

    [Fact]
    public void Rejects_occurrences_outside_their_region()
    {
        var occurrence = new MaterialOccurrence(
            "outside", "Outside Vein", "vein", "cavern",
            new MaterialReference("materials", "mineral/quartz"),
            new(new(9, 1, 1), new(11, 3, 3)), "Fracture filling.");

        Assert.Throws<ArgumentException>(() => new GeologicalWorld(Topology(), [Profile()], [occurrence]));
    }

    [Fact]
    public void Rejects_occurrences_that_reference_unknown_regions()
    {
        var occurrence = new MaterialOccurrence(
            "orphan", "Orphan Deposit", "deposit", "missing",
            new MaterialReference("materials", "mineral/copper"),
            new(new(1, 1, 1), new(2, 2, 2)), "Unknown.");

        Assert.Throws<ArgumentException>(() => new GeologicalWorld(Topology(), [Profile()], [occurrence]));
    }

    [Fact]
    public void Rejects_duplicate_occurrence_identifiers()
    {
        var material = new MaterialReference("materials", "mineral/calcite");
        MaterialOccurrence Occurrence() => new(
            "same", "Occurrence", "lens", "cavern", material,
            new(new(1, 1, 1), new(2, 2, 2)), "Replacement.");

        Assert.Throws<ArgumentException>(() =>
            new GeologicalWorld(Topology(), [Profile()], [Occurrence(), Occurrence()]));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Rejects_invalid_quantity_estimates(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MaterialQuantityEstimate(value, "tonnes", "survey", 0.8));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    public void Rejects_invalid_observation_confidence(double confidence)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MaterialGradeObservation(1, "%", "mass", "assay", confidence));
    }

    [Fact]
    public void Rejects_non_positive_spatial_bounds()
    {
        Assert.Throws<ArgumentException>(() =>
            new SpatialBounds3(new(1, 1, 1), new(1, 2, 2)));
    }

    private static SpatialTopology Topology() => new(
    [
        new SpatialRegion("cavern", "Cavern", new(0, 0, 0), new(10, 10, 10), 0, "Natural dissolution.")
    ]);

    private static GeologicalProfile Profile() => new(
        "profile", "cavern", "m", "datum",
        [new GeologicalLayer("bed", "Host Rock",
            new MaterialReference("materials", "rock/limestone"), 10, 0, "sedimentation", "older")]);
}
