using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TheSingularityWorkshop.FSM_Dungeon;

/// <summary>
/// An opaque reference to an actor owned by an external system such as SURPS.
/// This record carries identity and placement only; it does not define actor ontology or behavior.
/// </summary>
public sealed record DungeonScenarioActorReference(
    string Id,
    string ExternalIdentity,
    string InitialRegionId);

/// <summary>A recoverable scenario object placed in an authoritative spatial region.</summary>
public sealed record DungeonScenarioCache(
    string Id,
    string RegionId,
    string ContentReference,
    string PlacementReason);

/// <summary>The deliberately small command vocabulary supported by the host-neutral scenario proof.</summary>
public enum DungeonScenarioCommandKind
{
    Move,
    Inspect,
    Interact,
    Recover
}

/// <summary>
/// A deterministic command addressed to an opaque external actor reference.
/// TargetId is a region for Move and a feature/cache identity for the other commands.
/// </summary>
public sealed record DungeonScenarioCommand(
    long Sequence,
    string ActorReferenceId,
    DungeonScenarioCommandKind Kind,
    string TargetId);

/// <summary>
/// Immutable execution state for a small, reusable dungeon scenario.
/// Every successful command returns a new state; the prior snapshot remains unchanged.
/// </summary>
public sealed class DungeonScenarioState
{
    private readonly IReadOnlyDictionary<string, string> _actorRegions;
    private readonly IReadOnlyDictionary<string, IReadOnlySet<string>> _knownRegions;
    private readonly IReadOnlyDictionary<string, IReadOnlySet<string>> _knownFeatures;
    private readonly IReadOnlyDictionary<string, string> _recoveredCaches;

    public DungeonScenarioState(
        SpatialTopology topology,
        IEnumerable<DungeonScenarioActorReference> actors,
        IEnumerable<DungeonScenarioCache>? caches = null)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(actors);

        Topology = topology;
        Actors = Array.AsReadOnly(actors.ToArray());
        Caches = Array.AsReadOnly((caches ?? []).ToArray());
        CommandLog = Array.AsReadOnly(Array.Empty<DungeonScenarioCommand>());

        EnsureUnique(Actors.Select(actor => actor.Id), "actor reference");
        EnsureUnique(Caches.Select(cache => cache.Id), "cache");

        var regionIds = topology.Regions.Select(region => region.Id).ToHashSet(StringComparer.Ordinal);
        if (Actors.Any(actor => string.IsNullOrWhiteSpace(actor.Id)
            || string.IsNullOrWhiteSpace(actor.ExternalIdentity)
            || !regionIds.Contains(actor.InitialRegionId)))
            throw new ArgumentException("Every actor reference needs an opaque identity and a valid initial region.", nameof(actors));
        if (Caches.Any(cache => string.IsNullOrWhiteSpace(cache.Id)
            || !regionIds.Contains(cache.RegionId)
            || string.IsNullOrWhiteSpace(cache.ContentReference)
            || string.IsNullOrWhiteSpace(cache.PlacementReason)))
            throw new ArgumentException("Every cache needs a valid identity, region, content reference, and placement reason.", nameof(caches));

        _actorRegions = new ReadOnlyDictionary<string, string>(
            Actors.ToDictionary(actor => actor.Id, actor => actor.InitialRegionId, StringComparer.Ordinal));
        _knownRegions = new ReadOnlyDictionary<string, IReadOnlySet<string>>(
            Actors.ToDictionary(actor => actor.Id,
                actor => (IReadOnlySet<string>)new HashSet<string>([actor.InitialRegionId], StringComparer.Ordinal),
                StringComparer.Ordinal));
        _knownFeatures = new ReadOnlyDictionary<string, IReadOnlySet<string>>(
            Actors.ToDictionary(actor => actor.Id,
                _ => (IReadOnlySet<string>)new HashSet<string>(StringComparer.Ordinal),
                StringComparer.Ordinal));
        _recoveredCaches = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(StringComparer.Ordinal));
    }

    private DungeonScenarioState(
        SpatialTopology topology,
        IReadOnlyList<DungeonScenarioActorReference> actors,
        IReadOnlyList<DungeonScenarioCache> caches,
        IReadOnlyDictionary<string, string> actorRegions,
        IReadOnlyDictionary<string, IReadOnlySet<string>> knownRegions,
        IReadOnlyDictionary<string, IReadOnlySet<string>> knownFeatures,
        IReadOnlyDictionary<string, string> recoveredCaches,
        IReadOnlyList<DungeonScenarioCommand> commandLog)
    {
        Topology = topology;
        Actors = actors;
        Caches = caches;
        _actorRegions = actorRegions;
        _knownRegions = knownRegions;
        _knownFeatures = knownFeatures;
        _recoveredCaches = recoveredCaches;
        CommandLog = commandLog;
    }

    public SpatialTopology Topology { get; }
    public IReadOnlyList<DungeonScenarioActorReference> Actors { get; }
    public IReadOnlyList<DungeonScenarioCache> Caches { get; }
    public IReadOnlyList<DungeonScenarioCommand> CommandLog { get; }
    public IReadOnlyCollection<string> RecoveredCacheIds => _recoveredCaches.Keys.ToArray();

    public string GetActorRegion(string actorReferenceId) =>
        _actorRegions.TryGetValue(actorReferenceId, out var regionId)
            ? regionId
            : throw new KeyNotFoundException($"Unknown actor reference '{actorReferenceId}'.");

    public bool KnowsRegion(string actorReferenceId, string regionId) =>
        GetKnowledge(_knownRegions, actorReferenceId).Contains(regionId);

    public bool KnowsFeature(string actorReferenceId, string featureId) =>
        GetKnowledge(_knownFeatures, actorReferenceId).Contains(featureId);

    public bool IsCacheRecovered(string cacheId) => _recoveredCaches.ContainsKey(cacheId);

    /// <summary>Validates and applies one command, returning a new immutable state snapshot.</summary>
    public DungeonScenarioState Apply(DungeonScenarioCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.Sequence != CommandLog.Count + 1L)
            throw new InvalidOperationException($"Expected command sequence {CommandLog.Count + 1L}, received {command.Sequence}.");
        if (string.IsNullOrWhiteSpace(command.TargetId))
            throw new ArgumentException("A command target is required.", nameof(command));

        var currentRegion = GetActorRegion(command.ActorReferenceId);
        var actorRegions = new Dictionary<string, string>(_actorRegions, StringComparer.Ordinal);
        var knownRegions = CloneKnowledge(_knownRegions);
        var knownFeatures = CloneKnowledge(_knownFeatures);
        var recoveredCaches = new Dictionary<string, string>(_recoveredCaches, StringComparer.Ordinal);
        var topology = Topology;

        switch (command.Kind)
        {
            case DungeonScenarioCommandKind.Move:
            {
                if (!Topology.Regions.Any(region => region.Id == command.TargetId))
                    throw new KeyNotFoundException($"Unknown destination region '{command.TargetId}'.");
                var crossing = Topology.Connections.FirstOrDefault(connection =>
                    (connection.FromRegionId == currentRegion && connection.ToRegionId == command.TargetId)
                    || (connection.ToRegionId == currentRegion && connection.FromRegionId == command.TargetId));
                if (crossing is null)
                    throw new InvalidOperationException($"No spatial connection joins '{currentRegion}' to '{command.TargetId}'.");
                if (!Topology.CanTraverse(crossing.Id))
                    throw new InvalidOperationException($"Connection '{crossing.Id}' is not currently traversable.");

                actorRegions[command.ActorReferenceId] = command.TargetId;
                knownRegions[command.ActorReferenceId].Add(command.TargetId);
                break;
            }
            case DungeonScenarioCommandKind.Inspect:
            {
                var feature = FindFeature(command.TargetId);
                if (feature.RegionId is not null && feature.RegionId != currentRegion)
                    throw new InvalidOperationException($"Feature '{feature.Id}' is not in the actor's current region.");
                knownFeatures[command.ActorReferenceId].Add(feature.Id);
                break;
            }
            case DungeonScenarioCommandKind.Interact:
            {
                var feature = FindFeature(command.TargetId);
                var isLocal = feature.RegionId is null || feature.RegionId == currentRegion;
                var isBoundaryFeature = Topology.Connections.Any(connection =>
                    connection.RequiredFeatureId == feature.Id
                    && ((connection.FromRegionId == currentRegion && connection.ToRegionId == feature.RegionId)
                        || (connection.ToRegionId == currentRegion && connection.FromRegionId == feature.RegionId)));
                if (!isLocal && !isBoundaryFeature)
                    throw new InvalidOperationException($"Feature '{feature.Id}' is neither local nor on an adjacent crossing.");
                topology = Topology.WithFeatureOperationalState(feature.Id, true);
                knownFeatures[command.ActorReferenceId].Add(feature.Id);
                break;
            }
            case DungeonScenarioCommandKind.Recover:
            {
                var cache = Caches.FirstOrDefault(item => item.Id == command.TargetId)
                    ?? throw new KeyNotFoundException($"Unknown cache '{command.TargetId}'.");
                if (cache.RegionId != currentRegion)
                    throw new InvalidOperationException($"Cache '{cache.Id}' is not in the actor's current region.");
                if (recoveredCaches.ContainsKey(cache.Id))
                    throw new InvalidOperationException($"Cache '{cache.Id}' has already been recovered.");
                recoveredCaches.Add(cache.Id, command.ActorReferenceId);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(command), command.Kind, "Unsupported scenario command.");
        }

        var nextLog = CommandLog.Append(command).ToArray();
        return new DungeonScenarioState(
            topology,
            Actors,
            Caches,
            new ReadOnlyDictionary<string, string>(actorRegions),
            FreezeKnowledge(knownRegions),
            FreezeKnowledge(knownFeatures),
            new ReadOnlyDictionary<string, string>(recoveredCaches),
            Array.AsReadOnly(nextLog));
    }

    /// <summary>Returns a stable SHA-256 digest for the complete authoritative scenario snapshot.</summary>
    public string ComputeStateDigest()
    {
        var canonicalState = new
        {
            Regions = Topology.Regions.OrderBy(item => item.Id, StringComparer.Ordinal),
            Features = Topology.Features.OrderBy(item => item.Id, StringComparer.Ordinal),
            Connections = Topology.Connections.OrderBy(item => item.Id, StringComparer.Ordinal),
            ActorRegions = _actorRegions.OrderBy(item => item.Key, StringComparer.Ordinal),
            KnownRegions = _knownRegions.OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => new { Actor = item.Key, Values = item.Value.Order(StringComparer.Ordinal) }),
            KnownFeatures = _knownFeatures.OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => new { Actor = item.Key, Values = item.Value.Order(StringComparer.Ordinal) }),
            RecoveredCaches = _recoveredCaches.OrderBy(item => item.Key, StringComparer.Ordinal),
            Commands = CommandLog
        };
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(canonicalState)));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private SpatialFeature FindFeature(string featureId) =>
        Topology.Features.FirstOrDefault(feature => feature.Id == featureId)
        ?? throw new KeyNotFoundException($"Unknown feature '{featureId}'.");

    private static IReadOnlySet<string> GetKnowledge(
        IReadOnlyDictionary<string, IReadOnlySet<string>> knowledge,
        string actorReferenceId) =>
        knowledge.TryGetValue(actorReferenceId, out var known)
            ? known
            : throw new KeyNotFoundException($"Unknown actor reference '{actorReferenceId}'.");

    private static Dictionary<string, HashSet<string>> CloneKnowledge(
        IReadOnlyDictionary<string, IReadOnlySet<string>> source) =>
        source.ToDictionary(item => item.Key,
            item => new HashSet<string>(item.Value, StringComparer.Ordinal),
            StringComparer.Ordinal);

    private static IReadOnlyDictionary<string, IReadOnlySet<string>> FreezeKnowledge(
        Dictionary<string, HashSet<string>> source) =>
        new ReadOnlyDictionary<string, IReadOnlySet<string>>(
            source.ToDictionary(item => item.Key,
                item => (IReadOnlySet<string>)new HashSet<string>(item.Value, StringComparer.Ordinal),
                StringComparer.Ordinal));

    private static void EnsureUnique(IEnumerable<string> values, string label)
    {
        var materialized = values.ToArray();
        if (materialized.Any(string.IsNullOrWhiteSpace)
            || materialized.Distinct(StringComparer.Ordinal).Count() != materialized.Length)
            throw new ArgumentException($"{label} identifiers must be unique non-empty strings.");
    }
}
