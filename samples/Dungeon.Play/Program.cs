using System.Text.Json;
using TheSingularityWorkshop.FSM_Dungeon;

var scenarioPath = args.Length > 0
    ? args[0]
    : Path.Combine(AppContext.BaseDirectory, "Scenarios", "lantern-vault.json");

if (!File.Exists(scenarioPath))
{
    Console.Error.WriteLine($"Scenario fixture not found: {scenarioPath}");
    return 2;
}

using var document = JsonDocument.Parse(await File.ReadAllTextAsync(scenarioPath));
var root = document.RootElement;
var regions = root.GetProperty("regions").EnumerateArray().Select(region =>
{
    var origin = region.GetProperty("origin");
    var extent = region.GetProperty("extent");
    return new SpatialRegion(
        region.GetProperty("id").GetString()!,
        region.GetProperty("name").GetString()!,
        new SpatialPoint3(origin.GetProperty("x").GetDouble(), origin.GetProperty("y").GetDouble(), origin.GetProperty("z").GetDouble()),
        new SpatialExtent3(extent.GetProperty("width").GetDouble(), extent.GetProperty("height").GetDouble(), extent.GetProperty("depth").GetDouble()),
        region.GetProperty("floorElevation").GetDouble(),
        region.GetProperty("formationReason").GetString()!);
}).ToArray();
var features = root.GetProperty("features").EnumerateArray().Select(feature =>
    new SpatialFeature(
        feature.GetProperty("id").GetString()!,
        feature.GetProperty("kind").GetString()!,
        feature.GetProperty("name").GetString()!,
        feature.GetProperty("regionId").ValueKind == JsonValueKind.Null ? null : feature.GetProperty("regionId").GetString(),
        feature.GetProperty("isOperational").GetBoolean(),
        feature.GetProperty("formationReason").GetString()!)).ToArray();
var connections = root.GetProperty("connections").EnumerateArray().Select(connection =>
    new SpatialConnection(
        connection.GetProperty("id").GetString()!,
        connection.GetProperty("fromRegionId").GetString()!,
        connection.GetProperty("toRegionId").GetString()!,
        connection.GetProperty("traversalMode").GetString()!,
        connection.TryGetProperty("requiredFeatureId", out var required) ? required.GetString() : null)).ToArray();
var actors = root.GetProperty("actorReferences").EnumerateArray().Select(actor =>
    new DungeonScenarioActorReference(
        actor.GetProperty("id").GetString()!,
        actor.GetProperty("externalIdentity").GetString()!,
        actor.GetProperty("initialRegionId").GetString()!)).ToArray();
var cacheElement = root.GetProperty("cache");
var caches = new[]
{
    new DungeonScenarioCache(
        cacheElement.GetProperty("id").GetString()!,
        cacheElement.GetProperty("regionId").GetString()!,
        cacheElement.GetProperty("contentReference").GetString()!,
        cacheElement.GetProperty("placementReason").GetString()!)
};
var objectives = root.GetProperty("objectives").EnumerateArray().Select(item =>
    new DungeonScenarioObjectiveDefinition(
        item.GetProperty("id").GetString()!,
        item.GetProperty("description").GetString()!,
        item.GetProperty("completionCondition").GetString()!)).ToArray();
var outcomes = root.GetProperty("outcomes").EnumerateArray().Select(item =>
    new DungeonScenarioOutcomeDefinition(
        item.GetProperty("id").GetString()!,
        Array.AsReadOnly(item.GetProperty("when").EnumerateArray().Select(value => value.GetString()!).ToArray()),
        item.GetProperty("result").GetString()!)).ToArray();

var state = new DungeonScenarioState(new SpatialTopology(regions, features, connections), actors, caches);
var actorId = actors.First(actor => actor.Id.EndsWith(".party", StringComparison.Ordinal)).Id;
var title = root.GetProperty("title").GetString() ?? "Dungeon scenario";

Console.WriteLine($"\n{title} — developer playtest host");
Console.WriteLine("This is a host-neutral command harness, not the finished player UI or SURPS character experience.");
Console.WriteLine("Type 'help' for commands. Type 'quit' to leave.\n");
ShowLocation();

while (true)
{
    Console.Write("\n> ");
    var line = Console.ReadLine();
    if (line is null)
        break;

    var separator = line.IndexOf(' ');
    var verb = (separator < 0 ? line : line[..separator]).Trim().ToLowerInvariant();
    var target = separator < 0 ? string.Empty : line[(separator + 1)..].Trim();

    if (verb is "quit" or "exit")
        break;
    if (verb is "help" or "?")
    {
        Console.WriteLine("look | status | move <region name or id> | inspect <feature name or id>");
        Console.WriteLine("interact <feature name or id> | recover <cache name or id> | quit");
        continue;
    }
    if (verb is "look" or "l")
    {
        ShowLocation();
        continue;
    }
    if (verb is "status")
    {
        ShowStatus();
        continue;
    }
    if (string.IsNullOrWhiteSpace(target))
    {
        Console.WriteLine("That command needs a target. Type 'help' for the command list.");
        continue;
    }

    try
    {
        var kind = verb switch
        {
            "move" or "go" => DungeonScenarioCommandKind.Move,
            "inspect" or "examine" => DungeonScenarioCommandKind.Inspect,
            "interact" or "use" => DungeonScenarioCommandKind.Interact,
            "recover" or "take" => DungeonScenarioCommandKind.Recover,
            _ => (DungeonScenarioCommandKind?)null
        };
        if (kind is null)
        {
            Console.WriteLine($"Unknown command '{verb}'. Type 'help'.");
            continue;
        }

        var resolvedTarget = kind.Value switch
        {
            DungeonScenarioCommandKind.Move => Resolve(target, regions.Select(item => (item.Id, item.Name))),
            DungeonScenarioCommandKind.Inspect or DungeonScenarioCommandKind.Interact =>
                Resolve(target, features.Select(item => (item.Id, item.Name))),
            DungeonScenarioCommandKind.Recover => Resolve(target, caches.Select(item => (item.Id, item.Id))),
            _ => target
        };

        state = state.Apply(new DungeonScenarioCommand(
            state.CommandLog.Count + 1L, actorId, kind.Value, resolvedTarget));

        Console.WriteLine("Action accepted.");
        ShowStatus();
        ShowLocation();
    }
    catch (Exception exception) when (exception is ArgumentException
        or InvalidOperationException or KeyNotFoundException)
    {
        Console.WriteLine($"Action rejected: {exception.Message}");
        Console.WriteLine("The authoritative scenario state was not changed.");
    }
}

Console.WriteLine($"\nPlaytest ended after {state.CommandLog.Count} accepted commands.");
Console.WriteLine($"Final state digest: {state.ComputeStateDigest()}");
return 0;

string Resolve(string query, IEnumerable<(string Id, string Name)> candidates)
{
    var items = candidates.ToArray();
    var match = items.FirstOrDefault(item =>
        string.Equals(item.Id, query, StringComparison.OrdinalIgnoreCase)
        || string.Equals(item.Name, query, StringComparison.OrdinalIgnoreCase));
    if (match == default)
        throw new KeyNotFoundException($"No target named '{query}' exists in this fixture.");
    return match.Id;
}

void ShowLocation()
{
    var currentId = state.GetActorRegion(actorId);
    var current = regions.Single(region => region.Id == currentId);
    Console.WriteLine($"\n== {current.Name} ==");
    Console.WriteLine(current.FormationReason);
    Console.WriteLine("Local features:");
    foreach (var feature in features.Where(feature => feature.RegionId == currentId))
        Console.WriteLine($"  {(state.Topology.Features.Single(item => item.Id == feature.Id).IsOperational ? "[operational]" : "[inactive]")} {feature.Name} ({feature.Id})");
    Console.WriteLine("Adjacent crossings:");
    foreach (var connection in connections.Where(connection =>
        connection.FromRegionId == currentId || connection.ToRegionId == currentId))
    {
        var destinationId = connection.FromRegionId == currentId ? connection.ToRegionId : connection.FromRegionId;
        var destination = regions.Single(region => region.Id == destinationId);
        var crossingState = state.Topology.CanTraverse(connection.Id) ? "open" : "blocked";
        Console.WriteLine($"  {destination.Name}: {crossingState} via {connection.TraversalMode} ({connection.Id})");
    }
}

void ShowStatus()
{
    var evaluation = DungeonScenarioEvaluator.Evaluate(state, objectives, outcomes);
    Console.WriteLine($"Actor: {actorId} | Location: {state.GetActorRegion(actorId)} | Commands: {state.CommandLog.Count}");
    foreach (var objective in evaluation.Objectives)
    {
        var definition = objectives.Single(item => item.Id == objective.ObjectiveId);
        Console.WriteLine($"  {(objective.IsComplete ? "[x]" : "[ ]")} {definition.Description}");
    }
    if (evaluation.SelectedOutcome is not null)
        Console.WriteLine($"Outcome: {evaluation.SelectedOutcome.Result}");
}
