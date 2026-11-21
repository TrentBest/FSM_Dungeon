namespace TheSingularityWorkshop.FSM_Dungeon;

/// <summary>
/// Identifies the host-world location that contains or exposes this dungeon without coupling
/// the dungeon domain to the host's implementation.
/// </summary>
public sealed record DungeonSiteReference
{
    public DungeonSiteReference(string hostId, string hostKind, string anchorId)
    {
        HostId = Required(hostId, nameof(hostId));
        HostKind = Required(hostKind, nameof(hostKind));
        AnchorId = Required(anchorId, nameof(anchorId));
    }

    public string HostId { get; }
    public string HostKind { get; }
    public string AnchorId { get; }

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A meaningful value is required.", parameterName);
        return value.Trim();
    }
}
