using TheSingularityWorkshop.FSM_Dungeon;
using Xunit;

namespace TheSingularityWorkshop.FSM_Dungeon.Tests;

public sealed class DungeonScenarioStateTests
{
    private static DungeonScenarioState CreateScenario()
    {
        var topology = new SpatialTopology(
            [
                new SpatialRegion("entry", "Entry Gallery", new(0, 0, 0), new(8, 3, 6), 0, "Worked access gallery."),
                new SpatialRegion("silt", "Silt Hall", new(10, 0, 0), new(8, 3, 6), 0, "Water widened a natural fracture."),
                new SpatialRegion("vault", "Sealed Vault", new(20, 0, 0), new(8, 4, 8), 0, "Constructed chamber.")
            ],
            [
                new SpatialFeature("pressure-slab", "trap", "Pressure Slab", "silt", true, "Installed defensive mechanism."),
                new SpatialFeature("iron-door", "door", "Iron Door", "vault", false, "Installed vault barrier.")
            ],
            [
                new SpatialConnection("entry-silt", "entry", "silt", "passage"),
                new SpatialConnection("silt-vault", "silt", "vault", "doorway", "iron-door")
            ]);

        return new DungeonScenarioState(
            topology,
            [
                new DungeonScenarioActorReference("party", "surps.party.test-party", "entry"),
                new DungeonScenarioActorReference("sentinel", "surps.actor.test-sentinel", "vault")
            ],
            [
                new DungeonScenarioCache("survey-ledger", "vault", "content.survey-ledger", "Stored by the original surveyors.")
            ]);
    }

    private static DungeonScenarioState ReachSilt(DungeonScenarioState state) =>
        state.Apply(new DungeonScenarioCommand(1, "party", DungeonScenarioCommandKind.Move, "silt"));

    private static DungeonScenarioState ReachVault(DungeonScenarioState state)
    {
        state = ReachSilt(state);
        state = state.Apply(new DungeonScenarioCommand(2, "party", DungeonScenarioCommandKind.Interact, "iron-door"));
        return state.Apply(new DungeonScenarioCommand(3, "party", DungeonScenarioCommandKind.Move, "vault"));
    }

    [Fact]
    public void Initial_state_knows_only_the_actor_initial_region()
    {
        var state = CreateScenario();

        Assert.Equal("entry", state.GetActorRegion("party"));
        Assert.True(state.KnowsRegion("party", "entry"));
        Assert.False(state.KnowsRegion("party", "vault"));
        Assert.False(state.KnowsFeature("party", "pressure-slab"));
    }

    [Fact]
    public void Movement_requires_a_direct_traversable_spatial_connection()
    {
        var state = CreateScenario();

        Assert.Throws<InvalidOperationException>(() =>
            state.Apply(new DungeonScenarioCommand(1, "party", DungeonScenarioCommandKind.Move, "vault")));
    }

    [Fact]
    public void Movement_discovers_the_destination_without_replacing_prior_snapshot()
    {
        var initial = CreateScenario();
        var moved = ReachSilt(initial);

        Assert.Equal("entry", initial.GetActorRegion("party"));
        Assert.Equal("silt", moved.GetActorRegion("party"));
        Assert.True(moved.KnowsRegion("party", "silt"));
    }

    [Fact]
    public void Closed_door_blocks_traversal_until_interacted_with_from_adjacent_region()
    {
        var initial = CreateScenario();
        var atSilt = ReachSilt(initial);

        Assert.False(atSilt.Topology.CanTraverse("silt-vault"));
        var opened = atSilt.Apply(new DungeonScenarioCommand(2, "party", DungeonScenarioCommandKind.Interact, "iron-door"));

        Assert.True(opened.Topology.CanTraverse("silt-vault"));
        Assert.False(atSilt.Topology.CanTraverse("silt-vault"));
        Assert.Equal(2, opened.Topology.Regions.Count);
    }

    [Fact]
    public void Inspect_requires_a_local_feature_and_records_observer_scoped_knowledge()
    {
        var state = ReachSilt(CreateScenario());
        var inspected = state.Apply(new DungeonScenarioCommand(2, "party", DungeonScenarioCommandKind.Inspect, "pressure-slab"));

        Assert.True(inspected.KnowsFeature("party", "pressure-slab"));
        Assert.False(inspected.KnowsFeature("sentinel", "pressure-slab"));
        Assert.Throws<InvalidOperationException>(() =>
            inspected.Apply(new DungeonScenarioCommand(3, "party", DungeonScenarioCommandKind.Inspect, "iron-door")));
    }

    [Fact]
    public void Recovery_requires_actor_to_be_in_the_cache_region()
    {
        var state = CreateScenario();

        Assert.Throws<InvalidOperationException>(() =>
            state.Apply(new DungeonScenarioCommand(1, "party", DungeonScenarioCommandKind.Recover, "survey-ledger")));
    }

    [Fact]
    public void Recovery_is_recorded_against_the_external_actor_reference()
    {
        var state = ReachVault(CreateScenario());
        var recovered = state.Apply(new DungeonScenarioCommand(4, "party", DungeonScenarioCommandKind.Recover, "survey-ledger"));

        Assert.True(recovered.IsCacheRecovered("survey-ledger"));
        Assert.Contains("survey-ledger", recovered.RecoveredCacheIds);
        Assert.False(state.IsCacheRecovered("survey-ledger"));
    }

    [Fact]
    public void A_cache_cannot_be_recovered_twice()
    {
        var state = ReachVault(CreateScenario())
            .Apply(new DungeonScenarioCommand(4, "party", DungeonScenarioCommandKind.Recover, "survey-ledger"));

        Assert.Throws<InvalidOperationException>(() =>
            state.Apply(new DungeonScenarioCommand(5, "sentinel", DungeonScenarioCommandKind.Recover, "survey-ledger")));
    }

    [Fact]
    public void Command_sequence_must_be_contiguous_and_monotonic()
    {
        var state = CreateScenario();

        Assert.Throws<InvalidOperationException>(() =>
            state.Apply(new DungeonScenarioCommand(2, "party", DungeonScenarioCommandKind.Move, "silt")));
    }

    [Fact]
    public void Unknown_actor_and_unknown_targets_are_rejected()
    {
        var state = CreateScenario();

        Assert.Throws<KeyNotFoundException>(() => state.GetActorRegion("missing"));
        Assert.Throws<KeyNotFoundException>(() =>
            state.Apply(new DungeonScenarioCommand(1, "party", DungeonScenarioCommandKind.Move, "missing")));
    }

    [Fact]
    public void Replaying_the_same_command_log_produces_the_same_state_digest()
    {
        var first = ReachVault(CreateScenario())
            .Apply(new DungeonScenarioCommand(4, "party", DungeonScenarioCommandKind.Recover, "survey-ledger"));
        var second = ReachVault(CreateScenario())
            .Apply(new DungeonScenarioCommand(4, "party", DungeonScenarioCommandKind.Recover, "survey-ledger"));

        Assert.Equal(first.ComputeStateDigest(), second.ComputeStateDigest());
        Assert.Equal(64, first.ComputeStateDigest().Length);
        Assert.Equal(4, first.CommandLog.Count);
    }

    [Fact]
    public void Different_authoritative_states_produce_different_digests()
    {
        var initial = CreateScenario();
        var moved = ReachSilt(initial);

        Assert.NotEqual(initial.ComputeStateDigest(), moved.ComputeStateDigest());
    }
}
