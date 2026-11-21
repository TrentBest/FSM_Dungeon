using System.Text.Json;
using TheSingularityWorkshop.FSM_Dungeon;

var outputPath = args.Length > 0 ? args[0] : "docs/data/dungeon.json";
var definition = new DungeonDefinition(
    "beneath-the-first-lantern",
    "The Catacombs Beneath the First Lantern",
    20261121,
    new DungeonOrigin(
        "constructed-excavation",
        "The first settlement cut a route into the limestone shelf to reach a buried spring.",
        "The lower chambers protected the water route; later occupants sealed the deepest access.",
        "The first chambers predate the current town. Flooding, repairs, occupation, and abandonment left distinct traces."),
    18,
    [
        new InhabitantGroupDefinition(
            "lantern-wardens",
            "Lantern Wardens",
            "They guard the sealed spring route under an old order that has never been formally revoked.",
            "Patrolling the threshold and inspecting the old seals.",
            3),
        new InhabitantGroupDefinition(
            "mossbound-scavengers",
            "Mossbound Scavengers",
            "They shelter in the damp upper chambers and salvage metal left by the excavation crews.",
            "Sorting salvage and tending a sheltered sleeping place.",
            11),
        new InhabitantGroupDefinition(
            "deep-burrowers",
            "Deep Burrowers",
            "They follow the spring's mineral deposits and avoid the wardens' lit territory.",
            "Foraging near the damp lower passages.",
            16)
    ]);

var world = DungeonGenerator.Generate(definition);
var fullPath = Path.GetFullPath(outputPath);
Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
var json = JsonSerializer.Serialize(world, new JsonSerializerOptions { WriteIndented = true });
await File.WriteAllTextAsync(fullPath, json);

Console.WriteLine($"Generated {world.Definition.Name}");
Console.WriteLine($"Seed: {world.Definition.Seed}");
Console.WriteLine($"Chambers: {world.Chambers.Count}; passages: {world.Passages.Count}; inhabitant groups: {world.InhabitantGroups.Count}");
Console.WriteLine($"Connected: {world.IsConnected()}; validation errors: {world.Validate().Count}");
Console.WriteLine($"Wrote: {fullPath}");
