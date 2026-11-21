using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.FSM_Dungeon;

/// <summary>A chamber with retained formation and historical provenance.</summary>
public sealed record DungeonChamber(
    int Index,
    string Name,
    string FormationReason,
    string HistoricalUse,
    string CurrentPurpose);

/// <summary>A traversable connection between two chambers.</summary>
public sealed record DungeonPassage(int FromChamberIndex, int ToChamberIndex, string FormationReason);

/// <summary>A group placed in its declared home territory with an explicit motivation and activity.</summary>
public sealed record DungeonInhabitantGroup(
    string Id,
    string Name,
    string Motivation,
    string CurrentActivity,
    int HomeChamberIndex);

/// <summary>
/// Generated world data. The model is immutable from the public API and can be inspected without a renderer.
/// </summary>
public sealed class DungeonWorld
{
    internal DungeonWorld(
        DungeonDefinition definition,
        IEnumerable<DungeonChamber> chambers,
        IEnumerable<DungeonPassage> passages,
        IEnumerable<DungeonInhabitantGroup> inhabitantGroups)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        Chambers = new ReadOnlyCollection<DungeonChamber>(chambers.ToArray());
        Passages = new ReadOnlyCollection<DungeonPassage>(passages.ToArray());
        InhabitantGroups = new ReadOnlyCollection<DungeonInhabitantGroup>(inhabitantGroups.ToArray());
    }

    public DungeonDefinition Definition { get; }
    public IReadOnlyList<DungeonChamber> Chambers { get; }
    public IReadOnlyList<DungeonPassage> Passages { get; }
    public IReadOnlyList<DungeonInhabitantGroup> InhabitantGroups { get; }

    /// <summary>Returns true when every chamber can be reached from chamber zero.</summary>
    public bool IsConnected()
    {
        if (Chambers.Count == 0) return false;

        var visited = new HashSet<int> { Chambers[0].Index };
        var pending = new Queue<int>();
        pending.Enqueue(Chambers[0].Index);

        while (pending.Count > 0)
        {
            var current = pending.Dequeue();
            foreach (var passage in Passages)
            {
                var next = passage.FromChamberIndex == current
                    ? passage.ToChamberIndex
                    : passage.ToChamberIndex == current
                        ? passage.FromChamberIndex
                        : -1;

                if (next >= 0 && visited.Add(next))
                    pending.Enqueue(next);
            }
        }

        return visited.Count == Chambers.Count;
    }

    /// <summary>Checks identity, topology, provenance, and inhabitant-reference invariants.</summary>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        var chamberIds = Chambers.Select(chamber => chamber.Index).ToHashSet();

        if (Chambers.Count != Definition.ChamberCount)
            errors.Add("Generated chamber count does not match the definition.");
        if (chamberIds.Count != Chambers.Count)
            errors.Add("Chamber indices must be unique.");
        if (!Enumerable.Range(0, Chambers.Count).All(chamberIds.Contains))
            errors.Add("Chamber indices must be contiguous and start at zero.");
        if (Chambers.Any(chamber => string.IsNullOrWhiteSpace(chamber.Name)
                                    || string.IsNullOrWhiteSpace(chamber.FormationReason)
                                    || string.IsNullOrWhiteSpace(chamber.HistoricalUse)
                                    || string.IsNullOrWhiteSpace(chamber.CurrentPurpose)))
            errors.Add("Every chamber must retain a name, formation, history, and purpose provenance.");

        if (Passages.Any(passage => !chamberIds.Contains(passage.FromChamberIndex)
                                    || !chamberIds.Contains(passage.ToChamberIndex)
                                    || passage.FromChamberIndex == passage.ToChamberIndex))
            errors.Add("Every passage must connect two distinct chambers in this dungeon.");
        var normalizedEdges = Passages
            .Select(passage => passage.FromChamberIndex < passage.ToChamberIndex
                ? (passage.FromChamberIndex, passage.ToChamberIndex)
                : (passage.ToChamberIndex, passage.FromChamberIndex))
            .ToArray();
        if (normalizedEdges.Distinct().Count() != normalizedEdges.Length)
            errors.Add("A chamber pair may have only one passage.");
        if (Passages.Any(passage => string.IsNullOrWhiteSpace(passage.FormationReason)))
            errors.Add("Every passage must retain its formation reason.");
        if (!IsConnected())
            errors.Add("Every chamber must be reachable from the entrance.");

        if (InhabitantGroups.Select(group => group.Id).Distinct(StringComparer.Ordinal).Count()
            != InhabitantGroups.Count)
            errors.Add("Inhabitant group identifiers must be unique.");
        if (InhabitantGroups.Any(group => string.IsNullOrWhiteSpace(group.Id)
                                          || string.IsNullOrWhiteSpace(group.Name)
                                          || !chamberIds.Contains(group.HomeChamberIndex)
                                          || string.IsNullOrWhiteSpace(group.Motivation)
                                          || string.IsNullOrWhiteSpace(group.CurrentActivity)))
            errors.Add("Every inhabitant group must have a valid identity, territory, motivation, and activity.");

        return errors.AsReadOnly();
    }
}
