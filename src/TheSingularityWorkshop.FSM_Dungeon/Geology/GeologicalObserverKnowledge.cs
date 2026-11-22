namespace TheSingularityWorkshop.FSM_Dungeon.Geology;

/// <summary>
/// An immutable, observer-specific view of geological history.
/// The authoritative GeologicalWorld remains unchanged when an observer learns something.
/// </summary>
public sealed class GeologicalObserverKnowledge
{
    private readonly GeologicalWorld _world;
    private readonly IReadOnlyDictionary<string, GeologicalEvent> _eventsById;

    public GeologicalObserverKnowledge(
        GeologicalWorld world,
        string observerId,
        IEnumerable<GeologicalKnowledgeRecord>? records = null)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        if (string.IsNullOrWhiteSpace(observerId))
            throw new ArgumentException("An observer identifier is required.", nameof(observerId));

        ObserverId = observerId.Trim();
        var knownRecords = (records ?? []).ToArray();
        if (knownRecords.Any(record => record is null))
            throw new ArgumentException("Knowledge records cannot contain null entries.", nameof(records));
        if (knownRecords.Select(record => record.EventId).Distinct(StringComparer.Ordinal).Count()
            != knownRecords.Length)
            throw new ArgumentException("An observer can record each history event only once.", nameof(records));

        var eventsById = world.Events.ToDictionary(item => item.Id, StringComparer.Ordinal);
        var unknownIds = knownRecords.Where(record => !eventsById.ContainsKey(record.EventId))
            .Select(record => record.EventId).ToArray();
        if (unknownIds.Length > 0)
            throw new ArgumentException(
                $"Knowledge records reference unknown geological events: {string.Join(", ", unknownIds)}.",
                nameof(records));

        Records = Array.AsReadOnly(knownRecords.OrderBy(record => record.ObservedSequence).ToArray());
        _eventsById = new System.Collections.ObjectModel.ReadOnlyDictionary<string, GeologicalEvent>(
            Records.ToDictionary(record => record.EventId, record => eventsById[record.EventId],
                StringComparer.Ordinal));
    }

    public string ObserverId { get; }
    public IReadOnlyList<GeologicalKnowledgeRecord> Records { get; }

    /// <summary>
    /// Returns a new knowledge snapshot after the host has adjudicated that this observer
    /// perceived evidence for the specified world-history event.
    /// This method records knowledge; it does not perform visibility, sensing, or discovery tests.
    /// </summary>
    public GeologicalObserverKnowledge LearnEvent(
        string eventId,
        long observedSequence,
        double confidence = 1,
        string? interpretation = null)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            throw new ArgumentException("An event identifier is required.", nameof(eventId));
        if (!_world.Events.Any(item => string.Equals(item.Id, eventId, StringComparison.Ordinal)))
            throw new ArgumentException($"Unknown geological event '{eventId}'.", nameof(eventId));
        if (KnowsEvent(eventId))
            throw new InvalidOperationException($"Observer '{ObserverId}' already knows event '{eventId}'.");
        if (Records.Count > 0 && observedSequence <= Records[^1].ObservedSequence)
            throw new ArgumentOutOfRangeException(nameof(observedSequence),
                "A new knowledge sequence must follow the observer's existing knowledge timeline.");

        var nextRecords = Records.Append(
            new GeologicalKnowledgeRecord(eventId, observedSequence, confidence, interpretation));
        return new GeologicalObserverKnowledge(_world, ObserverId, nextRecords);
    }

    /// <summary>Returns whether this observer knows the specified history event.</summary>
    public bool KnowsEvent(string eventId) =>
        !string.IsNullOrWhiteSpace(eventId) && _eventsById.ContainsKey(eventId);

    /// <summary>
    /// Returns whether this observer knows of an occurrence through at least one recorded event.
    /// Existence in the authoritative world does not imply observer knowledge.
    /// </summary>
    public bool KnowsOccurrence(string occurrenceId) =>
        !string.IsNullOrWhiteSpace(occurrenceId)
        && _eventsById.Values.Any(item =>
            item.OccurrenceIds.Contains(occurrenceId, StringComparer.Ordinal));

    /// <summary>Returns known events in the order this observer learned them.</summary>
    public IReadOnlyList<GeologicalEvent> FindKnownEvents() =>
        Array.AsReadOnly(Records.Select(record => _eventsById[record.EventId]).ToArray());

    /// <summary>Returns this observer's knowledge record for an event, or null when it is unknown.</summary>
    public GeologicalKnowledgeRecord? FindRecord(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            throw new ArgumentException("An event identifier is required.", nameof(eventId));
        return Records.SingleOrDefault(record =>
            string.Equals(record.EventId, eventId, StringComparison.Ordinal));
    }
}
