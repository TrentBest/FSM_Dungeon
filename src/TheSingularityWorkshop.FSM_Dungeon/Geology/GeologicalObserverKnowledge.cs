namespace TheSingularityWorkshop.FSM_Dungeon.Geology;

/// <summary>
/// An immutable, observer-specific view of geological history.
/// The authoritative GeologicalWorld remains unchanged when an observer learns something.
/// </summary>
public sealed class GeologicalObserverKnowledge
{
    private readonly IReadOnlyDictionary<string, GeologicalEvent> _eventsById;

    public GeologicalObserverKnowledge(
        GeologicalWorld world,
        string observerId,
        IEnumerable<GeologicalKnowledgeRecord>? records = null)
    {
        ArgumentNullException.ThrowIfNull(world);
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
