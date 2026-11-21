using TheSingularityWorkshop.FSM_Dungeon;
using Xunit;

namespace TheSingularityWorkshop.FSM_Dungeon.Tests;

public sealed class SpatialTopologyTests
{
    private static SpatialTopology CreateCavern()
    {
        var regions = new[]
        {
            new SpatialRegion("west-ledge", "Western Ledge", new(-12, 0, 0), new(8, 5, 6), 0,
                "A natural limestone shelf formed beside a deep fracture."),
            new SpatialRegion("east-ledge", "Eastern Ledge", new(4, 0, 0), new(9, 4, 6), 0,
                "A separate shelf remains across the cavern void.")
        };
        var features = new[]
        {
            new SpatialFeature("great-chasm", "chasm", "The Great Chasm", null, true,
                "A fault opened a deep void between the shelves."),
            new SpatialFeature("rope-bridge", "suspension-bridge", "Old Rope Bridge", null, true,
                "Occupants later installed a crossing between the ledges.")
        };
        var connections = new[]
        {
            new SpatialConnection("bridge-crossing", "west-ledge", "east-ledge", "walk", "rope-bridge")
        };
        return new SpatialTopology(regions, features, connections);
    }

    [Fact]
    public void Destroying_bridge_removes_traversal_not_the_chasm_or_ledges()
    {
        var intact = CreateCavern();
        Assert.True(intact.CanTraverse("bridge-crossing"));

        var destroyed = intact.WithFeatureOperationalState("rope-bridge", false);

        Assert.False(destroyed.CanTraverse("bridge-crossing"));
        Assert.Equal(2, destroyed.Regions.Count);
        Assert.Contains(destroyed.Features, feature => feature.Id == "great-chasm" && feature.Kind == "chasm");
        Assert.Contains(destroyed.Connections, connection => connection.Id == "bridge-crossing");
        Assert.True(intact.CanTraverse("bridge-crossing"));
    }

    [Fact]
    public void Spatial_adjacency_can_exist_without_a_traversable_crossing()
    {
        var topology = CreateCavern().WithFeatureOperationalState("rope-bridge", false);

        Assert.Single(topology.Connections);
        Assert.False(topology.CanTraverse("bridge-crossing"));
    }

    [Fact]
    public void Vertical_and_natural_regions_are_supported_without_closed_type_enums()
    {
        var topology = new SpatialTopology(
            [
                new SpatialRegion("upper-cavern", "Upper Cavern", new(0, 0, 12), new(20, 8, 16), 12,
                    "Natural dissolution enlarged a limestone chamber."),
                new SpatialRegion("lower-mine", "Lower Mine", new(0, 0, -8), new(12, 5, 10), -8,
                    "Miners excavated a lower working.")
            ],
            [
                new SpatialFeature("vertical-shaft", "ventilated-shaft", "Old Ventilation Shaft", "upper-cavern", true,
                    "A shaft was cut down to the lower workings.")
            ],
            [
                new SpatialConnection("shaft-route", "upper-cavern", "lower-mine", "ladder", "vertical-shaft")
            ]);

        Assert.True(topology.CanTraverse("shaft-route"));
        Assert.Equal(-8, topology.Regions.Single(region => region.Id == "lower-mine").FloorElevation);
    }

    [Fact]
    public void Rejects_connections_to_unknown_regions_and_features()
    {
        Assert.Throws<ArgumentException>(() => new SpatialTopology(
            [new SpatialRegion("a", "A", new(0, 0, 0), new(1, 1, 1), 0, "Natural formation.")],
            connections: [new SpatialConnection("bad", "a", "missing", "crawl")]));

        Assert.Throws<ArgumentException>(() => new SpatialTopology(
            [
                new SpatialRegion("a", "A", new(0, 0, 0), new(1, 1, 1), 0, "Natural formation."),
                new SpatialRegion("b", "B", new(2, 0, 0), new(1, 1, 1), 0, "Excavated.")
            ],
            connections: [new SpatialConnection("bad", "a", "b", "walk", "missing")]));
    }
}
