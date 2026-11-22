using TheSingularityWorkshop.FSM_Dungeon.Geology;
using Xunit;

namespace TheSingularityWorkshop.FSM_Dungeon.Tests;

public sealed class GeologicalProfileTests
{
    private static MaterialReference Limestone => new("materials", "mineral/limestone");
    private static MaterialReference Shale => new("materials", "rock/shale");

    [Fact]
    public void Profile_preserves_ordered_layers_and_external_material_identity()
    {
        var profile = new GeologicalProfile(
            "north-wall-section",
            "north-wall",
            "m",
            "local-survey-datum",
            [
                new GeologicalLayer("soil", "Overburden", new MaterialReference("materials", "soil/topsoil"),
                    12, 10, "weathering-and-organic-accumulation", "youngest"),
                new GeologicalLayer("limestone", "Limestone Bed", Limestone,
                    10, 4, "marine-carbonate-sedimentation", "older"),
                new GeologicalLayer("shale", "Shale Bed", Shale,
                    4, -3, "fine-grained-marine-sedimentation", "oldest")
            ]);

        Assert.Equal(3, profile.Layers.Count);
        Assert.Equal(6, profile.Layers[1].Thickness);
        Assert.Equal("mineral/limestone", profile.FindLayerAt(7)!.Material.Id);
        Assert.Equal("rock/shale", profile.FindLayerAt(0)!.Material.Id);
        Assert.Null(profile.FindLayerAt(13));
        Assert.Null(profile.FindLayerAt(-4));
    }

    [Fact]
    public void Profile_allows_gaps_to_represent_uncharacterized_intervals()
    {
        var profile = new GeologicalProfile(
            "faulted-section", "fault-zone", "m", "survey-datum",
            [
                new GeologicalLayer("upper", "Upper Bed", Limestone, 20, 15,
                    "carbonate-deposition", "younger"),
                new GeologicalLayer("lower", "Lower Bed", Shale, 10, 2,
                    "marine-mud-deposition", "older")
            ]);

        Assert.Null(profile.FindLayerAt(12));
        Assert.Equal("upper", profile.FindLayerAt(17)!.Id);
        Assert.Equal("lower", profile.FindLayerAt(6)!.Id);
    }

    [Fact]
    public void Profile_rejects_overlapping_or_misordered_layers()
    {
        Assert.Throws<ArgumentException>(() => new GeologicalProfile(
            "invalid", "region", "m", "datum",
            [
                new GeologicalLayer("upper", "Upper", Limestone, 20, 10, "deposition", "young"),
                new GeologicalLayer("lower", "Lower", Shale, 12, 2, "deposition", "old")
            ]));
    }

    [Fact]
    public void Profile_rejects_duplicate_layer_ids()
    {
        Assert.Throws<ArgumentException>(() => new GeologicalProfile(
            "invalid", "region", "m", "datum",
            [
                new GeologicalLayer("same", "Upper", Limestone, 20, 10, "deposition", "young"),
                new GeologicalLayer("same", "Lower", Shale, 8, 2, "deposition", "old")
            ]));
    }

    [Fact]
    public void Layer_rejects_zero_or_non_finite_thickness()
    {
        Assert.Throws<ArgumentException>(() =>
            new GeologicalLayer("flat", "Flat", Limestone, 4, 4, "deposition", "unknown"));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeologicalLayer("nan", "NaN", Limestone, double.NaN, 0, "deposition", "unknown"));
    }

    [Fact]
    public void Lookup_rejects_non_finite_elevation()
    {
        var profile = new GeologicalProfile(
            "section", "region", "m", "datum",
            [new GeologicalLayer("bed", "Bed", Limestone, 4, 0, "deposition", "unknown")]);

        Assert.Throws<ArgumentOutOfRangeException>(() => profile.FindLayerAt(double.PositiveInfinity));
    }
}
