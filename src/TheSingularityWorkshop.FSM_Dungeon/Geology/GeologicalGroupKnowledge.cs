namespace TheSingularityWorkshop.FSM_Dungeon.Geology;

/// <summary>
/// Knowledge held by a population, separate from the private knowledge of its scouts.
/// Records enter this snapshot only when a living scout physically returns and reports.
/// </summary>
public sealed class GeologicalGroupKnowledge
{
    public GeologicalGroupKnowledge(string groupId, IEnumerable<GeologicalKnowledgeRecord>? records = null)
    {
        if (string.IsNullOrWhiteSpace(groupId))
            throw new ArgumentException("A group identifier is required.", nameof(groupId));

        GroupId = groupId.Trim();
        var known = (records ?? []).ToArray();
        if (known.Any(record => record is null))
            throw new ArgumentException("Group knowledge cannot contain null records.", nameof(records));
        if (known.Select(record => record.EventId).Distinct(StringComparer.Ordinal).Count() != known.Length)
            throw new ArgumentException("A group can record each event only once.", nameof(records));

        Records = Array.AsReadOnly(known.OrderBy(record => record.ObservedSequence).ToArray());
    }

    public string GroupId { get; }
    public IReadOnlyList<GeologicalKnowledgeRecord> Records { get; }

    public bool KnowsEvent(string eventId) =>
        !string.IsNullOrWhiteSpace(eventId)
        && Records.Any(record => string.Equals(record.EventId, eventId, StringComparison.Ordinal));

    /// <summary>
    /// Accepts a scout's report only after the host has established that the scout
    /// physically returned to this group. No remote or automatic knowledge transfer occurs.
    /// </summary>
    public GeologicalGroupKnowledge ReceiveReport(ScoutGeologicalReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        if (!string.Equals(report.GroupId, GroupId, StringComparison.Ordinal))
            throw new ArgumentException("The report is addressed to a different group.", nameof(report));

        var additions = report.Records.Where(record => !KnowsEvent(record.EventId)).ToArray();
        if (additions.Length == 0)
            return this;

        var merged = Records.Concat(additions)
            .OrderBy(record => record.ObservedSequence)
            .ToArray();
        return new GeologicalGroupKnowledge(GroupId, merged);
    }
}

/// <summary>
/// A report carried by an individual scout. Creating a report does not deliver it;
/// the group receives it only through ReceiveReport after the scout returns.
/// </summary>
public sealed class ScoutGeologicalReport
{
    public ScoutGeologicalReport(
        string scoutId,
        string groupId,
        IEnumerable<GeologicalKnowledgeRecord> records,
        long returnedAtTick)
    {
        if (string.IsNullOrWhiteSpace(scoutId))
            throw new ArgumentException("A scout identifier is required.", nameof(scoutId));
        if (string.IsNullOrWhiteSpace(groupId))
            throw new ArgumentException("A group identifier is required.", nameof(groupId));
        if (returnedAtTick < 0)
            throw new ArgumentOutOfRangeException(nameof(returnedAtTick));

        ScoutId = scoutId.Trim();
        GroupId = groupId.Trim();
        var report = (records ?? throw new ArgumentNullException(nameof(records))).ToArray();
        if (report.Any(record => record is null))
            throw new ArgumentException("A report cannot contain null records.", nameof(records));
        if (report.Select(record => record.EventId).Distinct(StringComparer.Ordinal).Count() != report.Length)
            throw new ArgumentException("A report cannot repeat an event.", nameof(records));

        Records = Array.AsReadOnly(report.OrderBy(record => record.ObservedSequence).ToArray());
        ReturnedAtTick = returnedAtTick;
    }

    public string ScoutId { get; }
    public string GroupId { get; }
    public IReadOnlyList<GeologicalKnowledgeRecord> Records { get; }
    public long ReturnedAtTick { get; }
}

/// <summary>
/// The group's limited assessment of an expedition's silence. It is not proof of death
/// and conveys none of the missing scout's private discoveries.
/// </summary>
public sealed class ScoutReportingStatus
{
    public ScoutReportingStatus(string scoutId, long expectedReportTick)
    {
        if (string.IsNullOrWhiteSpace(scoutId))
            throw new ArgumentException("A scout identifier is required.", nameof(scoutId));
        if (expectedReportTick < 0)
            throw new ArgumentOutOfRangeException(nameof(expectedReportTick));
        ScoutId = scoutId.Trim();
        ExpectedReportTick = expectedReportTick;
    }

    public string ScoutId { get; }
    public long ExpectedReportTick { get; }
    public bool IsOverdue(long currentTick)
    {
        if (currentTick < 0) throw new ArgumentOutOfRangeException(nameof(currentTick));
        return currentTick > ExpectedReportTick;
    }

    /// <summary>Returns uncertainty, not a claim that the scout is dead or captured.</summary>
    public string Assess(long currentTick) =>
        IsOverdue(currentTick) ? "Overdue — whereabouts unknown" : "Report not yet due";
}
