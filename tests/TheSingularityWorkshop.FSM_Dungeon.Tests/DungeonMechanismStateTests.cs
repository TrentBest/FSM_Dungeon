using TheSingularityWorkshop.FSM_Dungeon;
using Xunit;

namespace TheSingularityWorkshop.FSM_Dungeon.Tests;

public sealed class DungeonMechanismStateTests
{
    private static SpatialTopology CreateTopology() => new(
        [new SpatialRegion("gallery", "Pressure Gallery", new(0, 0, 0), new(8, 3, 6), 0, "Excavated gallery.")],
        [new SpatialFeature("pressure-slab", "mechanism", "Pressure Slab", "gallery", true, "Installed by the original builders.")]);

    private static DungeonMechanismState CreateState(bool oneShot = true) => new(
        CreateTopology(),
        [new DungeonMechanismDefinition(
            "counterweight-trap",
            "pressure-slab",
            "gallery",
            new DungeonMechanismTrigger("step-on-slab", "contact", "pressure-slab"),
            oneShot,
            [
                new DungeonMechanismEffect("environment.condition.request", "gallery.air-pressure", "increase", "kPa", "A counterweight releases into the ventilation shaft."),
                new DungeonMechanismEffect("actor.consequence.request", "surps.actor.external", "evaluate-impact", null, "A falling counterweight may affect an occupant.")
            ])]);

    [Fact]
    public void Trigger_requires_a_known_identity_and_valid_actor_location()
    {
        var state = CreateState();
        Assert.Throws<KeyNotFoundException>(() => state.Trigger("missing", "actor.external", "gallery"));
        Assert.Throws<KeyNotFoundException>(() => state.Trigger("step-on-slab", "actor.external", "missing"));
        Assert.Throws<InvalidOperationException>(() => state.Trigger("step-on-slab", "actor.external", "elsewhere"));
    }

    [Fact]
    public void One_shot_mechanism_fires_once_and_records_ordered_effect_requests()
    {
        var initial = CreateState();
        var fired = initial.Trigger("step-on-slab", "actor.external", "gallery");

        Assert.Equal(0, initial.Events.Count);
        Assert.Equal(2, fired.Events.Count);
        Assert.Equal("counterweight-trap:1:1", fired.Events[0].EventId);
        Assert.Equal("counterweight-trap:1:2", fired.Events[1].EventId);
        Assert.Equal("actor.external", fired.Events[0].ActorReferenceId);
        Assert.Equal(1, fired.GetActivationCount("counterweight-trap"));
        Assert.Throws<InvalidOperationException>(() => fired.Trigger("step-on-slab", "actor.external", "gallery"));
    }

    [Fact]
    public void Repeatable_mechanism_appends_a_new_activation_without_overwriting_history()
    {
        var initial = CreateState(oneShot: false);
        var first = initial.Trigger("step-on-slab", "actor.one", "gallery");
        var second = first.Trigger("step-on-slab", "actor.two", "gallery");

        Assert.Equal(2, second.GetActivationCount("counterweight-trap"));
        Assert.Equal(4, second.Events.Count);
        Assert.Equal(3, second.Events[2].Sequence);
        Assert.Equal("counterweight-trap:2:1", second.Events[2].EventId);
        Assert.Equal("actor.two", second.Events[2].ActorReferenceId);
        Assert.Equal(2, first.Events.Count);
    }

    [Fact]
    public void Same_trigger_replay_produces_the_same_digest()
    {
        var first = CreateState().Trigger("step-on-slab", "actor.external", "gallery");
        var second = CreateState().Trigger("step-on-slab", "actor.external", "gallery");

        Assert.Equal(first.ComputeStateDigest(), second.ComputeStateDigest());
        Assert.Equal(64, first.ComputeStateDigest().Length);
    }

    [Fact]
    public void Different_actor_cause_changes_the_event_digest()
    {
        var first = CreateState().Trigger("step-on-slab", "actor.one", "gallery");
        var second = CreateState().Trigger("step-on-slab", "actor.two", "gallery");

        Assert.NotEqual(first.ComputeStateDigest(), second.ComputeStateDigest());
    }

    [Fact]
    public void Duplicate_trigger_identities_are_rejected()
    {
        var topology = CreateTopology();
        var definition = new DungeonMechanismDefinition(
            "one", "pressure-slab", "gallery",
            new DungeonMechanismTrigger("duplicate", "contact", "pressure-slab"), true,
            [new DungeonMechanismEffect("environment.request", "gallery.air", "change", null, "Test.")]);
        var duplicate = definition with { Id = "two" };

        Assert.Throws<ArgumentException>(() => new DungeonMechanismState(topology, [definition, duplicate]));
    }

    [Fact]
    public void Mechanism_must_be_anchored_to_the_feature_region()
    {
        var definition = new DungeonMechanismDefinition(
            "bad", "pressure-slab", "elsewhere",
            new DungeonMechanismTrigger("trigger", "contact", "pressure-slab"), true,
            [new DungeonMechanismEffect("environment.request", "gallery.air", "change", null, "Test.")]);

        Assert.Throws<ArgumentException>(() => new DungeonMechanismState(CreateTopology(), [definition]));
    }
}
