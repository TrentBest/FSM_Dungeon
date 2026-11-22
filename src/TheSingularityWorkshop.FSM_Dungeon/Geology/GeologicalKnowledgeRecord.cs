namespace TheSingularityWorkshop.FSM_Dungeon.Geology;

/// <summary>
/// An observer's record that a particular geological history event has become known to them.
/// </summary>
/// <remarks>
/// ObservedSequence belongs to the observer's knowledge timeline; it is not geological age.
/// Confidence describes the observer's confidence in the recorded interpretation.
/// </remarks>
public sealed record GeologicalKnowledgeRecord
{
    public GeologicalKnowledgeRecord(
        string eventId,
        long observedSequence,
        double confidence = 1,
        string? interpretation = null)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            throw new ArgumentException("An event identifier is required.", nameof(eventId));
        if (observedSequence < 0)
            throw new ArgumentOutOfRangeException(nameof(observedSequence));
        if (!double.IsFinite(confidence) || confidence < 0 || confidence > 1)
            throw new ArgumentOutOfRangeException(nameof(confidence));

        EventId = eventId.Trim();
        ObservedSequence = observedSequence;
        Confidence = confidence;
        Interpretation = string.IsNullOrWhiteSpace(interpretation) ? null : interpretation.Trim();
    }

    public string EventId { get; }
    public long ObservedSequence { get; }
    public double Confidence { get; }
    public string? Interpretation { get; }
}
