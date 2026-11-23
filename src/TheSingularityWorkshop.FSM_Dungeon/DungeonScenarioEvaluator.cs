namespace TheSingularityWorkshop.FSM_Dungeon;

/// <summary>A declarative objective whose completion is derived only from authoritative scenario state.</summary>
/// <param name="Id">Stable scenario-local objective identifier.</param>
/// <param name="Description">Player-facing explanation; never used as executable logic.</param>
/// <param name="CompletionCondition">Supported predicate: cache.recovered:{cacheId} or connection.traversable:{connectionId}.</param>
public sealed record DungeonScenarioObjectiveDefinition(
    string Id,
    string Description,
    string CompletionCondition);

/// <summary>An ordered outcome candidate selected from completed objective predicates.</summary>
/// <param name="Id">Stable scenario-local outcome identifier.</param>
/// <param name="When">All predicates must match; supported predicate: objective.complete:{objectiveId}.</param>
/// <param name="Result">Narrative result text, not executable behavior.</param>
public sealed record DungeonScenarioOutcomeDefinition(
    string Id,
    IReadOnlyList<string> When,
    string Result);

/// <summary>The evaluated state of one objective.</summary>
public sealed record DungeonScenarioObjectiveResult(
    string ObjectiveId,
    bool IsComplete,
    string CompletionCondition);

/// <summary>Immutable evaluation output for one authoritative scenario snapshot.</summary>
public sealed class DungeonScenarioEvaluation
{
    internal DungeonScenarioEvaluation(
        IEnumerable<DungeonScenarioObjectiveResult> objectives,
        DungeonScenarioOutcomeDefinition? selectedOutcome)
    {
        Objectives = Array.AsReadOnly(objectives.ToArray());
        SelectedOutcome = selectedOutcome;
    }

    public IReadOnlyList<DungeonScenarioObjectiveResult> Objectives { get; }
    public DungeonScenarioOutcomeDefinition? SelectedOutcome { get; }
    public bool AllObjectivesComplete => Objectives.Count > 0 && Objectives.All(item => item.IsComplete);
    public bool HasCompletedObjective => Objectives.Any(item => item.IsComplete);
}

/// <summary>
/// Deterministically evaluates declarative objectives and selects the most specific matching outcome.
/// This layer does not mutate the world, award rewards, or own campaign continuity.
/// </summary>
public static class DungeonScenarioEvaluator
{
    public static DungeonScenarioEvaluation Evaluate(
        DungeonScenarioState state,
        IEnumerable<DungeonScenarioObjectiveDefinition> objectives,
        IEnumerable<DungeonScenarioOutcomeDefinition>? outcomes = null)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(objectives);

        var objectiveList = objectives.ToArray();
        var outcomeList = (outcomes ?? []).ToArray();
        EnsureUnique(objectiveList.Select(item => item.Id), "objective");
        EnsureUnique(outcomeList.Select(item => item.Id), "outcome");

        var objectiveIds = objectiveList.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var results = objectiveList.Select(objective =>
        {
            if (string.IsNullOrWhiteSpace(objective.Description))
                throw new ArgumentException($"Objective '{objective.Id}' requires a description.", nameof(objectives));
            return new DungeonScenarioObjectiveResult(
                objective.Id,
                EvaluateWorldCondition(state, objective.CompletionCondition),
                objective.CompletionCondition);
        }).ToArray();

        var completed = results.Where(item => item.IsComplete)
            .Select(item => item.ObjectiveId).ToHashSet(StringComparer.Ordinal);
        // Validate every candidate before matching any candidate. A false early predicate must
        // never hide a malformed later predicate through LINQ's short-circuiting All().
        var validatedOutcomes = outcomeList.Select((outcome, index) =>
        {
            if (string.IsNullOrWhiteSpace(outcome.Result))
                throw new ArgumentException($"Outcome '{outcome.Id}' requires a result description.", nameof(outcomes));
            var conditions = outcome.When ?? throw new ArgumentException(
                $"Outcome '{outcome.Id}' must provide a condition list.", nameof(outcomes));
            if (conditions.Count == 0)
                throw new ArgumentException($"Outcome '{outcome.Id}' must provide at least one condition.", nameof(outcomes));
            foreach (var condition in conditions)
                _ = EvaluateOutcomeCondition(condition, completed, objectiveIds);
            return new { Outcome = outcome, Index = index, Conditions = conditions };
        }).ToArray();

        var matchingOutcomes = validatedOutcomes
            .Where(item => item.Conditions.All(condition =>
                EvaluateOutcomeCondition(condition, completed, objectiveIds)))
            .OrderByDescending(item => item.Conditions.Count)
            .ThenBy(item => item.Index)
            .ToArray();

        return new DungeonScenarioEvaluation(results, matchingOutcomes.FirstOrDefault()?.Outcome);
    }

    private static bool EvaluateWorldCondition(DungeonScenarioState state, string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Objective completion conditions cannot be empty.", nameof(condition));

        const string cachePrefix = "cache.recovered:";
        const string connectionPrefix = "connection.traversable:";
        if (condition.StartsWith(cachePrefix, StringComparison.Ordinal))
        {
            var cacheId = condition[cachePrefix.Length..];
            if (string.IsNullOrWhiteSpace(cacheId) || !state.Caches.Any(item => item.Id == cacheId))
                throw new ArgumentException($"Objective references unknown cache condition '{condition}'.", nameof(condition));
            return state.IsCacheRecovered(cacheId);
        }

        if (condition.StartsWith(connectionPrefix, StringComparison.Ordinal))
        {
            var connectionId = condition[connectionPrefix.Length..];
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException($"Invalid connection condition '{condition}'.", nameof(condition));
            return state.Topology.CanTraverse(connectionId);
        }

        throw new ArgumentException($"Unsupported objective completion condition '{condition}'.", nameof(condition));
    }

    private static bool EvaluateOutcomeCondition(
        string condition,
        IReadOnlySet<string> completed,
        IReadOnlySet<string> knownObjectiveIds)
    {
        const string prefix = "objective.complete:";
        if (string.IsNullOrWhiteSpace(condition) || !condition.StartsWith(prefix, StringComparison.Ordinal))
            throw new ArgumentException($"Unsupported outcome condition '{condition}'.", nameof(condition));

        var objectiveId = condition[prefix.Length..];
        if (!knownObjectiveIds.Contains(objectiveId))
            throw new ArgumentException($"Outcome references unknown objective '{objectiveId}'.", nameof(condition));
        return completed.Contains(objectiveId);
    }

    private static void EnsureUnique(IEnumerable<string> identifiers, string label)
    {
        var values = identifiers.ToArray();
        if (values.Any(string.IsNullOrWhiteSpace)
            || values.Distinct(StringComparer.Ordinal).Count() != values.Length)
            throw new ArgumentException($"{label} identifiers must be unique non-empty strings.");
    }
}
