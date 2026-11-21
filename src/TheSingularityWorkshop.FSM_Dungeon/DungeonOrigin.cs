namespace TheSingularityWorkshop.FSM_Dungeon;

/// <summary>
/// Describes why a dungeon exists. Kind is intentionally an open string rather than a closed enum,
/// allowing experiences to introduce new formation mechanisms without changing the domain package.
/// </summary>
public sealed record DungeonOrigin
{
    public DungeonOrigin(string kind, string description, string purpose, string history)
    {
        Kind = Required(kind, nameof(kind));
        Description = Required(description, nameof(description));
        Purpose = Required(purpose, nameof(purpose));
        History = Required(history, nameof(history));
    }

    public string Kind { get; }
    public string Description { get; }
    public string Purpose { get; }
    public string History { get; }

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A meaningful value is required.", parameterName);
        return value.Trim();
    }
}