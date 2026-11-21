namespace TheSingularityWorkshop.FSM_Dungeon;

/// <summary>
/// Authored intent for a group that belongs in a dungeon for a reason.
/// </summary>
public sealed record InhabitantGroupDefinition
{
    public InhabitantGroupDefinition(
        string id,
        string name,
        string motivation,
        string initialActivity,
        int homeChamberIndex)
    {
        Id = Required(id, nameof(id));
        Name = Required(name, nameof(name));
        Motivation = Required(motivation, nameof(motivation));
        InitialActivity = Required(initialActivity, nameof(initialActivity));
        if (homeChamberIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(homeChamberIndex));
        HomeChamberIndex = homeChamberIndex;
    }

    public string Id { get; }
    public string Name { get; }
    public string Motivation { get; }
    public string InitialActivity { get; }
    public int HomeChamberIndex { get; }

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A meaningful value is required.", parameterName);
        return value.Trim();
    }
}