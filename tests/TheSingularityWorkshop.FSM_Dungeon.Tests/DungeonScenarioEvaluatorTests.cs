using TheSingularityWorkshop.FSM_Dungeon;
using Xunit;

namespace TheSingularityWorkshop.FSM_Dungeon.Tests;

public sealed class DungeonScenarioEvaluatorTests
{
    private static DungeonScenarioState CreateState()
    {
        var topology = new SpatialTopology(
            [
                new SpatialRegion("entry", "Entry", new(0, 0, 0), new(5, 3, 5), 0, "Worked access."),
                new SpatialRegion("vault", "Vault", new(6, 0, 0), new(5, 3, 5), 0, "Constructed chamber.")
            ],
            [new SpatialFeature("gate", "gate", "Gate", "vault", false, "Installed barrier.")],
            [new SpatialConnection("entry-vault", "entry", "vault", "doorway", "gate")]);
        return new DungeonScenarioState(topology,
            [new DungeonScenarioActorReference("party", "external.party", "entry")],
            [new DungeonScenarioCache("ledger", "vault", "content.ledger", "Stored for safekeeping.")]);
    }

    private static DungeonScenarioState CompleteRecovery()
    {
        var state = CreateState();
        state = state.Apply(new DungeonScenarioCommand(1, "party", DungeonScenarioCommandKind.Interact, "gate"));
        state = state.Apply(new DungeonScenarioCommand(2, "party", DungeonScenarioCommandKind.Move, "vault"));
        return state.Apply(new DungeonScenarioCommand(3, "party", DungeonScenarioCommandKind.Recover, "ledger"));
    }

    private static DungeonScenarioObjectiveDefinition[] Objectives() =>
    [
        new("recover", "Recover the ledger.", "cache.recovered:ledger"),
        new("open-route", "Restore the route.", "connection.traversable:entry-vault")
    ];

    [Fact]
    public void Objectives_are_incomplete_until_their_world_predicates_are_true()
    {
        var evaluation = DungeonScenarioEvaluator.Evaluate(CreateState(), Objectives());

        Assert.False(evaluation.Objectives[0].IsComplete);
        Assert.False(evaluation.Objectives[1].IsComplete);
        Assert.False(evaluation.AllObjectivesComplete);
        Assert.False(evaluation.HasCompletedObjective);
    }

    [Fact]
    public void Cache_recovery_completes_only_the_matching_objective()
    {
        var state = CreateState()
            .Apply(new DungeonScenarioCommand(1, "party", DungeonScenarioCommandKind.Interact, "gate"))
            .Apply(new DungeonScenarioCommand(2, "party", DungeonScenarioCommandKind.Move, "vault"))
            .Apply(new DungeonScenarioCommand(3, "party", DungeonScenarioCommandKind.Recover, "ledger"));
        var evaluation = DungeonScenarioEvaluator.Evaluate(state, Objectives());

        Assert.True(evaluation.Objectives[0].IsComplete);
        Assert.True(evaluation.Objectives[1].IsComplete);
    }

    [Fact]
    public void Traversable_connection_completes_route_objective_without_actor_movement()
    {
        var state = CreateState().Apply(
            new DungeonScenarioCommand(1, "party", DungeonScenarioCommandKind.Interact, "gate"));
        var evaluation = DungeonScenarioEvaluator.Evaluate(state, Objectives());

        Assert.True(evaluation.Objectives[1].IsComplete);
        Assert.False(evaluation.Objectives[0].IsComplete);
    }

    [Fact]
    public void Evaluating_objectives_does_not_mutate_the_source_state()
    {
        var state = CreateState();
        var digest = state.ComputeStateDigest();

        _ = DungeonScenarioEvaluator.Evaluate(state, Objectives());

        Assert.Equal(digest, state.ComputeStateDigest());
        Assert.Equal(0, state.CommandLog.Count);
    }

    [Fact]
    public void Most_specific_matching_outcome_wins_independent_of_declaration_order()
    {
        var outcomes = new[]
        {
            new DungeonScenarioOutcomeDefinition("both", ["objective.complete:recover", "objective.complete:open-route"], "Both goals."),
            new DungeonScenarioOutcomeDefinition("recovered", ["objective.complete:recover"], "Ledger recovered.")
        };
        var evaluation = DungeonScenarioEvaluator.Evaluate(CompleteRecovery(), Objectives(), outcomes);

        Assert.Equal("both", evaluation.SelectedOutcome?.Id);
        Assert.True(evaluation.AllObjectivesComplete);
    }

    [Fact]
    public void Outcome_requires_every_declared_objective_predicate()
    {
        var state = CreateState().Apply(
            new DungeonScenarioCommand(1, "party", DungeonScenarioCommandKind.Interact, "gate"));
        var outcomes = new[]
        {
            new DungeonScenarioOutcomeDefinition("both", ["objective.complete:recover", "objective.complete:open-route"], "Both goals."),
            new DungeonScenarioOutcomeDefinition("route", ["objective.complete:open-route"], "Route restored.")
        };
        var evaluation = DungeonScenarioEvaluator.Evaluate(state, Objectives(), outcomes);

        Assert.Equal("route", evaluation.SelectedOutcome?.Id);
    }

    [Fact]
    public void No_outcome_is_selected_when_no_predicate_matches()
    {
        var evaluation = DungeonScenarioEvaluator.Evaluate(CreateState(), Objectives(),
        [
            new DungeonScenarioOutcomeDefinition("recovered", ["objective.complete:recover"], "Recovered.")
        ]);

        Assert.Null(evaluation.SelectedOutcome);
    }

    [Fact]
    public void Outcome_ties_preserve_declaration_order()
    {
        var outcomes = new[]
        {
            new DungeonScenarioOutcomeDefinition("first", ["objective.complete:recover"], "First."),
            new DungeonScenarioOutcomeDefinition("second", ["objective.complete:recover"], "Second.")
        };
        var evaluation = DungeonScenarioEvaluator.Evaluate(CompleteRecovery(), Objectives(), outcomes);

        Assert.Equal("first", evaluation.SelectedOutcome?.Id);
    }

    [Fact]
    public void Unknown_cache_condition_is_rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            DungeonScenarioEvaluator.Evaluate(CreateState(),
                [new("bad", "Unknown cache.", "cache.recovered:missing")]));
    }

    [Fact]
    public void Unknown_connection_condition_is_rejected()
    {
        Assert.Throws<KeyNotFoundException>(() =>
            DungeonScenarioEvaluator.Evaluate(CreateState(),
                [new("bad", "Unknown connection.", "connection.traversable:missing")]));
    }

    [Fact]
    public void Unknown_objective_reference_in_outcome_is_rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            DungeonScenarioEvaluator.Evaluate(CreateState(), Objectives(),
                [new("bad", ["objective.complete:missing"], "Invalid reference.")]));
    }

    [Fact]
    public void Duplicate_objective_ids_are_rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            DungeonScenarioEvaluator.Evaluate(CreateState(),
                [new("duplicate", "One.", "cache.recovered:ledger"),
                 new("duplicate", "Two.", "connection.traversable:entry-vault")]));
    }

    [Fact]
    public void Malformed_later_outcome_predicate_is_rejected_even_when_an_earlier_predicate_is_false()
    {
        var outcomes = new[]
        {
            new DungeonScenarioOutcomeDefinition(
                "malformed",
                ["objective.complete:recover", "objective.complete:missing"],
                "Must not be silently skipped.")
        };

        Assert.Throws<ArgumentException>(() =>
            DungeonScenarioEvaluator.Evaluate(CreateState(), Objectives(), outcomes));
    }

    [Fact]
    public void Empty_objective_set_does_not_claim_all_objectives_complete()
    {
        var evaluation = DungeonScenarioEvaluator.Evaluate(CreateState(), []);

        Assert.False(evaluation.AllObjectivesComplete);
        Assert.False(evaluation.HasCompletedObjective);
    }
}
