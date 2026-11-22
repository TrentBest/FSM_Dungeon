using System.Text.Json;
using TheSingularityWorkshop.FSM_Dungeon;

var outputPath = args.Length > 0 ? args[0] : "docs/data/dungeon.json";
var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(outputPath))!;
Directory.CreateDirectory(outputDirectory);
var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

var catacombs = DungeonGenerator.Generate(new DungeonDefinition(
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
            "lantern-wardens", "Lantern Wardens",
            "They guard the sealed spring route under an old order that has never been formally revoked.",
            "Patrolling the threshold and inspecting the old seals.", 3),
        new InhabitantGroupDefinition(
            "mossbound-scavengers", "Mossbound Scavengers",
            "They shelter in the damp upper chambers and salvage metal left by the excavation crews.",
            "Sorting salvage and tending a sheltered sleeping place.", 11),
        new InhabitantGroupDefinition(
            "deep-burrowers", "Deep Burrowers",
            "They follow the spring's mineral deposits and avoid the wardens' lit territory.",
            "Foraging near the damp lower passages.", 16)
    ],
    additionalPassageCount: 3,
    site: new DungeonSiteReference("singularity-city", "city", "first-lantern"),
    conditions:
    [
        new DungeonCondition("spring-seal", "water-control", "sealed", "", "An old stone gate protects the buried spring route."),
        new DungeonCondition("burial-layers", "historical-strata", "4", "layers", "Successive burial and occupation layers remain visible."),
        new DungeonCondition("disturbance", "recent-disturbance", "localized", "", "Fresh marks appear near a passage that was sealed for generations.")
    ]));

var sewer = DungeonGenerator.Generate(new DungeonDefinition(
    "singularity-city-underworks",
    "The Underworks of Singularity City",
    20261122,
    new DungeonOrigin(
        "municipal-infrastructure",
        "A stormwater and service network was excavated as the city expanded across the basin.",
        "Carry runoff toward the outfall, provide inspection access, and keep essential city services reachable.",
        "The oldest culverts were built first; later districts added branches, reinforced junctions, and bypasses."),
    22,
    [
        new InhabitantGroupDefinition(
            "maintenance-crew-seven", "Maintenance Crew Seven",
            "They keep the flood gates and inspection routes functioning to protect the districts above.",
            "Inspecting a pressure gate and clearing debris from a service channel.", 2),
        new InhabitantGroupDefinition(
            "culvert-dwellers", "Culvert Dwellers",
            "They seek dry shelter and food scraps while avoiding the lit maintenance routes.",
            "Gathering salvage above the normal waterline.", 10),
        new InhabitantGroupDefinition(
            "outfall-hunters", "Outfall Hunters",
            "They follow fish and other food sources toward the outfall, defending access to the deeper channels.",
            "Watching the current and guarding a submerged crossing.", 19)
    ],
    additionalPassageCount: 24,
    site: new DungeonSiteReference("singularity-city", "city", "stormwater-district"),
    conditions:
    [
        new DungeonCondition("main-flow", "hydraulic-flow-rate", "2.4", "m3/s", "Runoff moves east through the main collector toward the outfall."),
        new DungeonCondition("water-level", "water-level", "0.8", "m", "The ordinary waterline leaves narrow maintenance ledges exposed."),
        new DungeonCondition("gate-12", "flow-control", "partially-blocked", "", "Debris has reduced the capacity of a downstream gate."),
        new DungeonCondition("air-quality", "atmospheric-condition", "poor", "", "Stagnant pockets require maintenance crews to use protective equipment."),
        new DungeonCondition("service-access", "maintenance-access", "limited", "", "Two inspection ladders are damaged and one route is flooded.")
    ]));


var cavernRegions = new[]
{
    new SpatialRegion("west-ledge", "West Ledge", new SpatialPoint3(0, 0, 0),
        new SpatialExtent3(18, 7, 12), 0, "A limestone shelf exposed by a deep erosional fissure."),
    new SpatialRegion("east-ledge", "East Ledge", new SpatialPoint3(34, -2, 0),
        new SpatialExtent3(16, 9, 12), -2, "A fractured shelf separated from the west ledge by the fissure."),
    new SpatialRegion("upper-gallery", "Upper Gallery", new SpatialPoint3(4, 18, 7),
        new SpatialExtent3(12, 8, 10), 18, "A higher passage reached by a narrow vertical shaft.")
};
var cavernFeatures = new[]
{
    new SpatialFeature("great-fissure", "chasm", "The Great Fissure", null, true,
        "A deep geological fracture split the original limestone shelf."),
    new SpatialFeature("rope-bridge", "bridge", "The Suspended Rope Bridge", null, true,
        "Explorers installed a temporary crossing between the separated ledges."),
    new SpatialFeature("west-shaft-ladder", "ladder", "The West Shaft Ladder", "west-ledge", true,
        "A fixed ladder descends from the upper gallery to the west ledge.")
};
var cavernConnections = new[]
{
    new SpatialConnection("ledge-crossing", "west-ledge", "east-ledge", "walk", "rope-bridge"),
    new SpatialConnection("shaft-access", "west-ledge", "upper-gallery", "climb", "west-shaft-ladder")
};
var intactCavern = new SpatialTopology(cavernRegions, cavernFeatures, cavernConnections);
var collapsedCavern = intactCavern.WithFeatureOperationalState("rope-bridge", false);
var cavernProof = new
{
    name = "The Great Fissure",
    description = "Two natural ledges are divided by a deep chasm. A suspended bridge provides a mutable crossing; a ladder reaches an upper gallery.",
    regions = intactCavern.Regions,
    features = intactCavern.Features,
    connections = intactCavern.Connections,
    states = new[]
    {
        new
        {
            id = "bridge-intact",
            name = "Before the bridge is destroyed",
            bridgeOperational = intactCavern.Features.Single(feature => feature.Id == "rope-bridge").IsOperational,
            traversableConnections = intactCavern.Connections.Where(connection => intactCavern.CanTraverse(connection.Id)).Select(connection => connection.Id).ToArray()
        },
        new
        {
            id = "bridge-destroyed",
            name = "After the bridge is destroyed",
            bridgeOperational = collapsedCavern.Features.Single(feature => feature.Id == "rope-bridge").IsOperational,
            traversableConnections = collapsedCavern.Connections.Where(connection => collapsedCavern.CanTraverse(connection.Id)).Select(connection => connection.Id).ToArray()
        }
    }
};

var hydraulicSimulation = new HydraulicSimulation(new HydraulicNetworkDefinition(sewer,
    [
        new HydraulicChamber(0, 3.0, 2.0, 2.0, 4.0),
        new HydraulicChamber(1, 2.0, 2.0, 0.2, 3.0),
        new HydraulicChamber(2, 0.5, 2.0, 0.1, 2.0, inflowCubicMetersPerSecond: 0.15)
    ],
    [
        new HydraulicPassage(0, 1, 0.0, 0.4, 0.2),
        new HydraulicPassage(1, 2, 0.0, 0.25, 0.15, blockageFraction: 0.35)
    ]));
var hydraulicSnapshots = Enumerable.Range(0, 6)
    .Select(_ => hydraulicSimulation.Step(10))
    .ToArray();

var worlds = new[] { catacombs, sewer };
await File.WriteAllTextAsync(Path.Combine(outputDirectory, "dungeon.json"),
    JsonSerializer.Serialize(catacombs, jsonOptions));
await File.WriteAllTextAsync(Path.Combine(outputDirectory, "sewer.json"),
    JsonSerializer.Serialize(sewer, jsonOptions));
await File.WriteAllTextAsync(Path.Combine(outputDirectory, "worlds.json"),
    JsonSerializer.Serialize(worlds, jsonOptions));
await File.WriteAllTextAsync(Path.Combine(outputDirectory, "hydraulic.json"),
    JsonSerializer.Serialize(hydraulicSnapshots, jsonOptions));
await File.WriteAllTextAsync(Path.Combine(outputDirectory, "spatial.json"),
    JsonSerializer.Serialize(cavernProof, jsonOptions));

foreach (var world in worlds)
{
    Console.WriteLine($"Generated {world.Definition.Name}");
    Console.WriteLine($"Seed: {world.Definition.Seed}");
    Console.WriteLine($"Chambers: {world.Chambers.Count}; passages: {world.Passages.Count}; inhabitant groups: {world.InhabitantGroups.Count}; conditions: {world.Conditions.Count}");
    Console.WriteLine($"Host anchor: {world.Site?.HostId}/{world.Site?.AnchorId}");
    Console.WriteLine($"Connected: {world.IsConnected()}; validation errors: {world.Validate().Count}");
}
Console.WriteLine($"Hydraulic proof: {hydraulicSnapshots.Length} ticks; final stored water: {hydraulicSnapshots[^1].Chambers.Sum(chamber => chamber.WaterVolumeCubicMeters):F3} m3; overflow: {hydraulicSnapshots[^1].CumulativeOverflowVolumeCubicMeters:F3} m3");
Console.WriteLine($"Wrote proof data to: {outputDirectory}");
