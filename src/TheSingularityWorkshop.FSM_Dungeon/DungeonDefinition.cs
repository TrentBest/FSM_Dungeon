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
        IEnumerable<InhabitantGroupDefinition>? inhabitantGroups = null,
        int? additionalPassageCount = null,
        DungeonSiteReference? site = null,
        IEnumerable<DungeonCondition>? conditions = null)
    {
        Id = Required(id, nameof(id));
        Name = Required(name, nameof(name));
        Origin = origin ?? throw new ArgumentNullException(nameof(origin));
        if (chamberCount is < 2 or > MaximumChambers)
            throw new ArgumentOutOfRangeException(nameof(chamberCount), $"Chamber count must be between 2 and {MaximumChambers}.");
        ChamberCount = chamberCount;

        var maximumAdditionalPassages = chamberCount * (chamberCount - 1) / 2 - (chamberCount - 1);
        var selectedAdditionalPassageCount = additionalPassageCount ?? chamberCount / 3;
        if (selectedAdditionalPassageCount < 0 || selectedAdditionalPassageCount > maximumAdditionalPassages)
            throw new ArgumentOutOfRangeException(nameof(additionalPassageCount), $"Additional passage count must be between 0 and {maximumAdditionalPassages}.");
        AdditionalPassageCount = selectedAdditionalPassageCount;

        var groups = (inhabitantGroups ?? []).ToArray();
        if (groups.Any(group => group is null))
            throw new ArgumentException("Inhabitant groups cannot contain null values.", nameof(inhabitantGroups));
        if (groups.Select(group => group.Id).Distinct(StringComparer.Ordinal).Count() != groups.Length)
            throw new ArgumentException("Inhabitant group identifiers must be unique.", nameof(inhabitantGroups));
        if (groups.Any(group => group.HomeChamberIndex >= chamberCount))
            throw new ArgumentException("Every inhabitant group must have a home chamber within this dungeon.", nameof(inhabitantGroups));

        var authoredConditions = (conditions ?? []).ToArray();
        if (authoredConditions.Any(condition => condition is null))
            throw new ArgumentException("Conditions cannot contain null values.", nameof(conditions));
        if (authoredConditions.Select(condition => condition.Id).Distinct(StringComparer.Ordinal).Count() != authoredConditions.Length)
            throw new ArgumentException("Condition identifiers must be unique.", nameof(conditions));

        InhabitantGroups = new ReadOnlyCollection<InhabitantGroupDefinition>(groups);
        Conditions = new ReadOnlyCollection<DungeonCondition>(authoredConditions);
        Site = site;
        Seed = seed;
    }

    public string Id { get; }
    public string Name { get; }
    public ulong Seed { get; }
    public DungeonOrigin Origin { get; }
    public int ChamberCount { get; }
    /// <summary>Exact number of non-spanning passages to add after the connected backbone.</summary>
    public int AdditionalPassageCount { get; }
    public IReadOnlyList<InhabitantGroupDefinition> InhabitantGroups { get; }
    /// <summary>Optional host-world anchor; the referenced host need not be loaded by this domain.</summary>
    public DungeonSiteReference? Site { get; }
    /// <summary>Open-ended authored facts describing environmental and historical conditions.</summary>
    public IReadOnlyList<DungeonCondition> Conditions { get; }

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A meaningful value is required.", parameterName);
        return value.Trim();
    }
}
