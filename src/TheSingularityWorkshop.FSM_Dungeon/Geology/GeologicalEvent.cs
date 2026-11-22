namespace TheSingularityWorkshop.FSM_Dungeon.Geology;

/// <summary>
/// A dated-by-sequence fact in a region's geological or human history.
/// Event kinds are intentionally open strings so domains can add formation, exposure,
/// discovery, survey, excavation, disturbance, and other event vocabularies.
/// </summary>
/// <remarks>
/// Sequence is an ordering key within the supplied world history, not an absolute
/// geological age or a claim that intervals between events are uniform.
/// </remarks>
public sealed class GeologicalEvent
{
    public GeologicalEvent(
        string id,
        string regionId,
        long sequence,
        string kind,
        string description,
        IEnumerable<string>? occurrenceIds = null,
        string? evidence = null)
    {
        Id = Required(id, nameof(id));
        RegionId = Required(regionId, nameof(regionId));
        if (sequence < 0)
            throw new ArgumentOutOfRangeException(nameof(sequence), "Event sequence must be non-negative.");
        Sequence = sequence;
        Kind = Required(kind, nameof(kind));
        Description = Required(description, nameof(description));
        Evidence = string.IsNullOrWhiteSpace(evidence) ? null : evidence.Trim();

        var references = (occurrenceIds ?? []).Select(id => Required(id, nameof(occurrenceIds))).ToArray();
        if (references.Distinct(StringComparer.Ordinal).Count() != references.Length)
            throw new ArgumentException("An event cannot reference the same occurrence more than once.", nameof(occurrenceIds));
        OccurrenceIds = Array.AsReadOnly(references);
    }

    public string Id { get; }
    public string RegionId { get; }
    public long Sequence { get; }
    public string Kind { get; }
    public string Description { get; }
    public IReadOnlyList<string> OccurrenceIds { get; }
    public string? Evidence { get; }

    private static string Required(string value, string parameterName) =>
        !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw new ArgumentException("A meaningful value is required.", parameterName);
}
