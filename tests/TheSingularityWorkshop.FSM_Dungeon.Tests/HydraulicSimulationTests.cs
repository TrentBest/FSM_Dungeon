using TheSingularityWorkshop.FSM_Dungeon;
using Xunit;

namespace TheSingularityWorkshop.FSM_Dungeon.Tests;

public sealed class HydraulicSimulationTests
{
    [Fact]
    public void Water_moves_down_head_gradient_and_is_conserved_without_inflow()
    {
        var network = new HydraulicNetworkDefinition(
            [
                new HydraulicChamber(0, 0, 2, 2, 5),
                new HydraulicChamber(1, 0, 2, 0, 5)
            ],
            [new HydraulicPassage(0, 1, -1, 1, 0.5)]);
        var simulation = new HydraulicSimulation(network);

        var next = simulation.Step(1);

        Assert.Single(next.Transfers);
        Assert.Equal(0, next.Transfers[0].FromChamberIndex);
        Assert.Equal(1, next.Transfers[0].ToChamberIndex);
        Assert.Equal(2, next.Chambers.Sum(x => x.WaterVolumeCubicMeters), 10);
        Assert.Equal(1, next.Tick);
    }

    [Fact]
    public void Full_blockage_prevents_transfer()
    {
        var network = new HydraulicNetworkDefinition(
            [new HydraulicChamber(0, 0, 1, 1, 3), new HydraulicChamber(1, 0, 1, 0, 3)],
            [new HydraulicPassage(0, 1, -1, 1, 1, blockageFraction: 1)]);
        var simulation = new HydraulicSimulation(network);

        var next = simulation.Step(1);

        Assert.Empty(next.Transfers);
        Assert.Equal(1, next.Chambers[0].WaterVolumeCubicMeters, 10);
        Assert.Equal(0, next.Chambers[1].WaterVolumeCubicMeters, 10);
    }

    [Fact]
    public void Inflow_is_applied_before_flow_and_is_bounded_by_capacity()
    {
        var network = new HydraulicNetworkDefinition(
            [new HydraulicChamber(0, 0, 1, 0, 1, inflowCubicMetersPerSecond: 2)],
            []);
        var simulation = new HydraulicSimulation(network);

        var next = simulation.Step(1);

        Assert.Equal(1, next.Chambers[0].WaterVolumeCubicMeters, 10);
        Assert.Equal(1, next.CumulativeOverflowVolumeCubicMeters, 10);
        Assert.Empty(next.Transfers);
    }

    [Fact]
    public void Transfers_into_a_full_chamber_are_recorded_as_overflow()
    {
        var network = new HydraulicNetworkDefinition(
            [new HydraulicChamber(0, 0, 1, 2, 2), new HydraulicChamber(1, 0, 1, 1, 1)],
            [new HydraulicPassage(0, 1, -1, 1, 1)]);
        var simulation = new HydraulicSimulation(network);

        var next = simulation.Step(1);

        Assert.Single(next.Transfers);
        Assert.Equal(2, next.Chambers.Sum(x => x.WaterVolumeCubicMeters), 10);
        Assert.Equal(1, next.CumulativeOverflowVolumeCubicMeters, 10);
        Assert.Equal(3, next.Chambers.Sum(x => x.WaterVolumeCubicMeters)
            + next.CumulativeOverflowVolumeCubicMeters, 10);
    }

    [Fact]
    public void Multiple_outlets_cannot_withdraw_more_than_the_source_contains()
    {
        var network = new HydraulicNetworkDefinition(
            [
                new HydraulicChamber(0, 0, 1, 0, 2),
                new HydraulicChamber(1, 1, 1, 1, 2),
                new HydraulicChamber(2, 0, 1, 0, 2)
            ],
            [
                new HydraulicPassage(1, 0, -1, 1, 1),
                new HydraulicPassage(1, 2, -1, 1, 1)
            ]);
        var simulation = new HydraulicSimulation(network);

        var next = simulation.Step(1);

        Assert.Equal(2, next.Transfers.Count);
        Assert.All(next.Chambers, chamber => Assert.True(chamber.WaterVolumeCubicMeters >= 0));
        Assert.Equal(1, next.Chambers.Sum(x => x.WaterVolumeCubicMeters), 10);
    }

    [Fact]
    public void Hydraulic_edges_must_exist_in_the_dungeon_topology()
    {
        var world = DungeonGenerator.Generate(new DungeonDefinition(
            "hydraulic-topology", "Hydraulic Topology", 17,
            new DungeonOrigin("constructed", "Built chambers.", "Drain water.", "Built in sequence."),
            3, additionalPassageCount: 0));
        var chambers = new[]
        {
            new HydraulicChamber(0, 0, 1, 1, 2),
            new HydraulicChamber(1, 0, 1, 0, 2),
            new HydraulicChamber(2, 0, 1, 0, 2)
        };
        var passages = new[] { new HydraulicPassage(0, 2, -1, 1, 1) };

        Assert.Throws<ArgumentException>(() => new HydraulicNetworkDefinition(world, chambers, passages));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Step_rejects_invalid_duration(double duration)
    {
        var network = new HydraulicNetworkDefinition([new HydraulicChamber(0, 0, 1, 0, 1)], []);
        var simulation = new HydraulicSimulation(network);
        Assert.Throws<ArgumentOutOfRangeException>(() => simulation.Step(duration));
    }
}
