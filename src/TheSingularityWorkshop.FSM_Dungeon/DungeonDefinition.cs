using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.FSM_Dungeon;

/// <summary>
/// Complete deterministic input for generating one dungeon.
/// </summary>
public sealed class DungeonDefinition
{
    public const int MaximumChambers = 256;

    public DungeonDefinition(
        string id,
        string name,
        ulong seed,
        DungeonOrigin origin,
        int chamberCount,
        IEnumerable<InhabitantGroupDefinition>? inhabitantGroups = null)
    {
        Id = Required(id, nameof(id));
        Name = Required(name, nameof(name));
        Origin = origin ?? throw new ArgumentNullException(nameof(origin));
        if (chamberCount is < 2 or > MaximumChambers)
            throw new ArgumentOutOfRangeException(nameof(chamberCount), $"Chamber count must be between 2 and {MaximumChambers}.");
        ChamberCount = chamberCount;

        var groups = (inhabitantGroups ?? []).ToArray();
        if (groups.Any(group => group is null))
            throw new ArgumentException("Inhabitant groups cannot contain null values.", nameof(inhabitantGroups));
        if (groups.Select(group => group.Id).Distinct(StringComparer.Ordinal).Count() != groups.Length)
            throw new ArgumentException("Inhabitant group identifiers must be unique.", nameof(inhabitantGroups));
        if (groups.Any(group => group.HomeChamberIndex >= chamberCount))
            throw new ArgumentException("Every inhabitant group must have a home chamber within this dungeon.", nameof(inhabitantGroups));

        InhabitantGroups = new ReadOnlyCollection<InhabitantGroupDefinition>(groups);
        Seed = seed;
    }

    public string Id { get; }
    public string Name { get; }
    public ulong Seed { get; }
    public DungeonOrigin Origin { get; }
    public int ChamberCount { get; }
    public IReadOnlyList<InhabitantGroupDefinition> InhabitantGroups { get; }

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A meaningful value is required.", parameterName);
        return value.Trim();
    }
}