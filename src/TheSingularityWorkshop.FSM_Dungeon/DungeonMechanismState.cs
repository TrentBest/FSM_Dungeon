using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TheSingularityWorkshop.FSM_Dungeon;

/// <summary>A stable, externally supplied activation condition for a dungeon mechanism.</summary>
public sealed record DungeonMechanismTrigger(string Id, string Kind, string TargetId);

/// <summary>
/// A host-neutral effect request. The dungeon records the declared effect but does not apply
/// physics, actor injury, health, or other externally owned consequences.
/// </summary>
public sealed record DungeonMechanismEffect(
    string Kind,
    string TargetId,
    string Value,
    string? Unit,
    string Reason);

/// <summary>A deterministic mechanism definition anchored to existing spatial identities.</summary>
public sealed record DungeonMechanismDefinition(
    string Id,
    string FeatureId,
    string RegionId,
    DungeonMechanismTrigger Trigger,
    bool IsOneShot,
    IReadOnlyList<DungeonMechanismEffect> Effects);

/// <summary>An immutable, provenance-bearing record that a mechanism activation was accepted.</summary>
public sealed record DungeonMechanismEvent(
    long Sequence,
    string EventId,
    string MechanismId,
    string TriggerId,
    string ActorReferenceId,
    string RegionId,
    int Activation,
    int EffectIndex,
    DungeonMechanismEffect Effect);

/// <summary>
/// Immutable event state for deterministic mechanism activation. Effects are requests for host
/// adapters; this class never mutates topology, actor ontology, or physical simulation state.
/// </summary>
public sealed class DungeonMechanismState
{
    private readonly IReadOnlyDictionary<string, int> _activationCounts;

    public DungeonMechanismState(
        SpatialTopology topology,
        IEnumerable<DungeonMechanismDefinition> mechanisms)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(mechanisms);
        Topology = topology;
        Mechanisms = Array.AsReadOnly(mechanisms.ToArray());
        Events = Array.AsReadOnly(Array.Empty<DungeonMechanismEvent>());
        ValidateDefinitions();
        _activationCounts = new Dictionary<string, int>(StringComparer.Ordinal);
    }

    private DungeonMechanismState(
        SpatialTopology topology,
        IReadOnlyList<DungeonMechanismDefinition> mechanisms,
        IReadOnlyDictionary<string, int> activationCounts,
        IReadOnlyList<DungeonMechanismEvent> events)
    {
        Topology = topology;
        Mechanisms = mechanisms;
        _activationCounts = activationCounts;
        Events = events;
    }

    public SpatialTopology Topology { get; }
    public IReadOnlyList<DungeonMechanismDefinition> Mechanisms { get; }
    public IReadOnlyList<DungeonMechanismEvent> Events { get; }

    public int GetActivationCount(string mechanismId) =>
        _activationCounts.TryGetValue(mechanismId, out var count)
            ? count
            : Mechanisms.Any(item => item.Id == mechanismId)
                ? 0
                : throw new KeyNotFoundException($"Unknown mechanism '{mechanismId}'.");

    /// <summary>
    /// Accepts a trigger only when its stable identity, feature, region, and actor location agree.
    /// A one-shot mechanism cannot be activated twice. Each successful activation appends effects
    /// in authored order and returns a new snapshot.
    /// </summary>
    public DungeonMechanismState Trigger(
        string triggerId,
        string actorReferenceId,
        string actorRegionId)
    {
        if (string.IsNullOrWhiteSpace(triggerId))
            throw new ArgumentException("A trigger identity is required.", nameof(triggerId));
        if (string.IsNullOrWhiteSpace(actorReferenceId))
            throw new ArgumentException("An opaque actor reference is required.", nameof(actorReferenceId));
        if (!Topology.Regions.Any(item => item.Id == actorRegionId))
            throw new KeyNotFoundException($"Unknown actor region '{actorRegionId}'.");

        var candidates = Mechanisms.Where(item => item.Trigger.Id == triggerId).ToArray();
        if (candidates.Length == 0)
            throw new KeyNotFoundException($"Unknown mechanism trigger '{triggerId}'.");
        if (candidates.Length > 1)
            throw new InvalidOperationException($"Trigger '{triggerId}' is ambiguous; trigger identities must be unique.");

        var mechanism = candidates[0];
        if (mechanism.RegionId != actorRegionId)
            throw new InvalidOperationException($"Mechanism '{mechanism.Id}' is not in the actor's current region.");
        if (mechanism.IsOneShot && GetActivationCount(mechanism.Id) > 0)
            throw new InvalidOperationException($"One-shot mechanism '{mechanism.Id}' has already fired.");

        var activation = GetActivationCount(mechanism.Id) + 1;
        var appended = mechanism.Effects.Select((effect, index) =>
            new DungeonMechanismEvent(
                Events.Count + index + 1L,
                $"{mechanism.Id}:{activation}:{index + 1}",
                mechanism.Id,
                triggerId,
                actorReferenceId,
                actorRegionId,
                activation,
                index + 1,
                effect)).ToArray();

        var counts = new Dictionary<string, int>(_activationCounts, StringComparer.Ordinal)
        {
            [mechanism.Id] = activation
        };
        return new DungeonMechanismState(
            Topology,
            Mechanisms,
            new System.Collections.ObjectModel.ReadOnlyDictionary<string, int>(counts),
            Array.AsReadOnly(Events.Concat(appended).ToArray()));
    }

    /// <summary>Hashes mechanism definitions, activation counts, and ordered effect events.</summary>
    public string ComputeStateDigest()
    {
        var canonical = new
        {
            Mechanisms = Mechanisms.OrderBy(item => item.Id, StringComparer.Ordinal),
            ActivationCounts = _activationCounts.OrderBy(item => item.Key, StringComparer.Ordinal),
            Events
        };
        return Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(canonical)))).ToLowerInvariant();
    }

    private void ValidateDefinitions()
    {
        EnsureUnique(Mechanisms.Select(item => item.Id), "mechanism");
        EnsureUnique(Mechanisms.Select(item => item.Trigger?.Id ?? string.Empty), "trigger");
        var featureIds = Topology.Features.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var regionIds = Topology.Regions.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var mechanism in Mechanisms)
        {
            if (string.IsNullOrWhiteSpace(mechanism.Id)
                || string.IsNullOrWhiteSpace(mechanism.FeatureId)
                || !featureIds.Contains(mechanism.FeatureId)
                || string.IsNullOrWhiteSpace(mechanism.RegionId)
                || !regionIds.Contains(mechanism.RegionId)
                || mechanism.Trigger is null
                || string.IsNullOrWhiteSpace(mechanism.Trigger.Id)
                || string.IsNullOrWhiteSpace(mechanism.Trigger.Kind)
                || string.IsNullOrWhiteSpace(mechanism.Trigger.TargetId)
                || mechanism.Effects is null
                || mechanism.Effects.Count == 0)
                throw new ArgumentException($"Mechanism '{mechanism.Id}' has incomplete or invalid spatial/trigger/effect references.", nameof(Mechanisms));

            var feature = Topology.Features.Single(item => item.Id == mechanism.FeatureId);
            if (feature.RegionId is not null && feature.RegionId != mechanism.RegionId)
                throw new ArgumentException($"Mechanism '{mechanism.Id}' is not anchored to its feature's region.", nameof(Mechanisms));
            if (mechanism.Effects.Any(effect => effect is null
                || string.IsNullOrWhiteSpace(effect.Kind)
                || string.IsNullOrWhiteSpace(effect.TargetId)
                || string.IsNullOrWhiteSpace(effect.Value)
                || string.IsNullOrWhiteSpace(effect.Reason)))
                throw new ArgumentException($"Mechanism '{mechanism.Id}' contains an incomplete effect descriptor.", nameof(Mechanisms));
        }
    }

    private static void EnsureUnique(IEnumerable<string> values, string label)
    {
        var materialized = values.ToArray();
        if (materialized.Any(string.IsNullOrWhiteSpace)
            || materialized.Distinct(StringComparer.Ordinal).Count() != materialized.Length)
            throw new ArgumentException($"{label} identifiers must be unique non-empty strings.");
    }
}
