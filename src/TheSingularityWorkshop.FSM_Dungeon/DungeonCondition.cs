namespace TheSingularityWorkshop.FSM_Dungeon;

/// <summary>
/// An authored or observed condition of a place. Kind and Unit are open strings so
/// experiences can describe hydraulic, structural, ecological, historical, or novel systems.
/// </summary>
public sealed record DungeonCondition
{
    public DungeonCondition(string id, string kind, string value, string unit, string description)
    {
        Id = Required(id, nameof(id));
        Kind = Required(kind, nameof(kind));
        Value = Required(value, nameof(value));
        Unit = unit?.Trim() ?? string.Empty;
        Description = Required(description, nameof(description));
    }

    public string Id { get; }
    public string Kind { get; }
    public string Value { get; }
    public string Unit { get; }
    public string Description { get; }

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A meaningful value is required.", parameterName);
        return value.Trim();
    }
}
