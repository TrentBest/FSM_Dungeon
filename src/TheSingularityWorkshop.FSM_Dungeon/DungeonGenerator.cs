namespace TheSingularityWorkshop.FSM_Dungeon;

/// <summary>
/// Generates a connected dungeon from explicit causal inputs. The generator is deterministic:
/// identical definitions produce identical chamber, passage, and group data.
/// </summary>
public static class DungeonGenerator
{
    public static DungeonWorld Generate(DungeonDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var random = new DeterministicRandom(definition.Seed);
        var chambers = new List<DungeonChamber>(definition.ChamberCount);
        var passages = new List<DungeonPassage>(definition.ChamberCount * 2);

        for (var index = 0; index < definition.ChamberCount; index++)
        {
            var formationDetail = random.Next(4) switch
            {
                0 => "The original formation remains visible in the chamber walls.",
                1 => "Later occupants altered the chamber while preserving its earlier structure.",
                2 => "The chamber widened where the formation encountered a natural obstruction.",
                _ => "The chamber retains evidence of successive periods of use."
            };

            chambers.Add(new DungeonChamber(
                index,
                index == 0 ? "The Descent" : $"Chamber {index:000}",
                $"{definition.Origin.Kind}: {definition.Origin.Description} {formationDetail}",
                definition.Origin.History,
                definition.Origin.Purpose));
        }

        // A spanning chain guarantees connectivity before optional cross-passages are added.
        for (var index = 1; index < definition.ChamberCount; index++)
        {
            passages.Add(new DungeonPassage(
                index - 1,
                index,
                $"The passage follows the same formation history: {definition.Origin.Description}"));
        }

        // Add a small number of deterministic loops to avoid a purely linear dungeon.
        var extraPassages = definition.ChamberCount / 3;
        var existing = new HashSet<(int, int)>(passages.Select(p => (p.FromChamberIndex, p.ToChamberIndex)));
        for (var attempt = 0; attempt < extraPassages * 4 && extraPassages > 0; attempt++)
        {
            var left = random.Next(definition.ChamberCount);
            var right = random.Next(definition.ChamberCount);
            if (left == right) continue;
            var edge = left < right ? (left, right) : (right, left);
            if (Math.Abs(left - right) < 2 || !existing.Add(edge)) continue;

            passages.Add(new DungeonPassage(
                edge.Item1,
                edge.Item2,
                $"A later route was opened through the existing formation: {definition.Origin.History}"));
            extraPassages--;
        }

        var groups = definition.InhabitantGroups.Select(group =>
            new DungeonInhabitantGroup(
                group.Id,
                group.Name,
                group.Motivation,
                group.InitialActivity,
                group.HomeChamberIndex)).ToArray();

        var world = new DungeonWorld(definition, chambers, passages, groups);
        var errors = world.Validate();
        if (errors.Count > 0)
            throw new InvalidOperationException("Generated dungeon failed validation: " + string.Join(" ", errors));

        return world;
    }

    /// <summary>Small fixed algorithm so generated worlds do not depend on runtime Random implementation changes.</summary>
    private sealed class DeterministicRandom
    {
        private ulong _state;

        public DeterministicRandom(ulong seed) => _state = seed;

        public int Next(int exclusiveMaximum)
        {
            if (exclusiveMaximum <= 0) throw new ArgumentOutOfRangeException(nameof(exclusiveMaximum));
            _state += 0x9E3779B97F4A7C15UL;
            var value = _state;
            value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
            value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
            value ^= value >> 31;
            return (int)(value % (uint)exclusiveMaximum);
        }
    }
}