namespace TheSingularityWorkshop.FSM_Dungeon;

/// <summary>A position in the dungeon's engine-independent three-dimensional coordinate space.</summary>
public readonly record struct SpatialPoint3(double X, double Y, double Z)
{
    public bool IsFinite => double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z);
}

/// <summary>Positive axis-aligned dimensions in the dungeon's three-dimensional coordinate space.</summary>
public readonly record struct SpatialExtent3(double Width, double Height, double Depth)
{
    public bool IsValid => double.IsFinite(Width) && double.IsFinite(Height)
        && double.IsFinite(Depth) && Width > 0 && Height > 0 && Depth > 0;
}

/// <summary>A continuous place or volume in the world, independent of rooms, floors, or presentation partitions.</summary>
public sealed record SpatialRegion(
    string Id,
    string Name,
    SpatialPoint3 Origin,
    SpatialExtent3 Extent,
    double FloorElevation,
    string FormationReason)
{
    public bool IsValid => !string.IsNullOrWhiteSpace(Id)
        && !string.IsNullOrWhiteSpace(Name)
        && Origin.IsFinite
        && Extent.IsValid
        && double.IsFinite(FloorElevation)
        && !string.IsNullOrWhiteSpace(FormationReason);
}

/// <summary>A semantic feature of space, such as a chasm, shaft, ledge, bridge, gate, or collapse.</summary>
/// <remarks>Feature kinds remain open strings so content can extend the domain without changing an enum.</remarks>
public sealed record SpatialFeature(
    string Id,
    string Kind,
    string Name,
    string? RegionId,
    bool IsOperational,
    string FormationReason)
{
    public bool IsValid => !string.IsNullOrWhiteSpace(Id)
        && !string.IsNullOrWhiteSpace(Kind)
        && !string.IsNullOrWhiteSpace(Name)
        && !string.IsNullOrWhiteSpace(RegionId) || RegionId is null
        && !string.IsNullOrWhiteSpace(FormationReason);
}

/// <summary>
/// A spatial adjacency between two regions. Adjacency is retained even when a required crossing feature
/// is damaged, closed, or otherwise unavailable.
/// </summary>
public sealed record SpatialConnection(
    string Id,
    string FromRegionId,
    string ToRegionId,
    string TraversalMode,
    string? RequiredFeatureId = null)
{
    public bool IsValid => !string.IsNullOrWhiteSpace(Id)
        && !string.IsNullOrWhiteSpace(FromRegionId)
        && !string.IsNullOrWhiteSpace(ToRegionId)
        && !string.Equals(FromRegionId, ToRegionId, StringComparison.Ordinal)
        && !string.IsNullOrWhiteSpace(TraversalMode);
}

/// <summary>
/// Immutable spatial facts and mutable-world snapshots for a continuous underground environment.
/// Geometry, adjacency, and traversal availability are represented separately.
/// </summary>
public sealed class SpatialTopology
{
    public SpatialTopology(
        IEnumerable<SpatialRegion> regions,
        IEnumerable<SpatialFeature>? features = null,
        IEnumerable<SpatialConnection>? connections = null)
    {
        ArgumentNullException.ThrowIfNull(regions);
        Regions = Array.AsReadOnly(regions.ToArray());
        Features = Array.AsReadOnly((features ?? []).ToArray());
        Connections = Array.AsReadOnly((connections ?? []).ToArray());

        if (Regions.Any(region => region is null || !region.IsValid))
            throw new ArgumentException("Every spatial region must have valid identity, geometry, elevation, and provenance.", nameof(regions));
        if (Features.Any(feature => feature is null || !feature.IsValid))
            throw new ArgumentException("Every spatial feature must have valid identity, kind, and provenance.", nameof(features));
        if (Connections.Any(connection => connection is null || !connection.IsValid))
            throw new ArgumentException("Every spatial connection must have valid endpoints and a traversal mode.", nameof(connections));

        EnsureUnique(Regions.Select(item => item.Id), nameof(regions));
        EnsureUnique(Features.Select(item => item.Id), nameof(features));
        EnsureUnique(Connections.Select(item => item.Id), nameof(connections));

        var regionIds = Regions.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        var featureIds = Features.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
        if (Features.Any(feature => feature.RegionId is not null && !regionIds.Contains(feature.RegionId)))
            throw new ArgumentException("A spatial feature must reference an existing region when a region is specified.", nameof(features));
        if (Connections.Any(connection => !regionIds.Contains(connection.FromRegionId)
            || !regionIds.Contains(connection.ToRegionId)))
            throw new ArgumentException("Every spatial connection must reference existing regions.", nameof(connections));
        if (Connections.Any(connection => connection.RequiredFeatureId is not null
            && !featureIds.Contains(connection.RequiredFeatureId)))
            throw new ArgumentException("A connection's required crossing feature must exist.", nameof(connections));
    }

    public IReadOnlyList<SpatialRegion> Regions { get; }
    public IReadOnlyList<SpatialFeature> Features { get; }
    public IReadOnlyList<SpatialConnection> Connections { get; }

    /// <summary>Returns whether the connection currently permits traversal; spatial adjacency is never removed.</summary>
    public bool CanTraverse(string connectionId)
    {
        var connection = Connections.FirstOrDefault(item => item.Id == connectionId)
            ?? throw new KeyNotFoundException($"Unknown spatial connection '{connectionId}'.");
        if (connection.RequiredFeatureId is null)
            return true;

        return Features.Single(feature => feature.Id == connection.RequiredFeatureId).IsOperational;
    }

    /// <summary>Creates a new topology snapshot with one feature's operational state changed.</summary>
    public SpatialTopology WithFeatureOperationalState(string featureId, bool isOperational)
    {
        var found = false;
        var updated = Features.Select(feature =>
        {
            if (!string.Equals(feature.Id, featureId, StringComparison.Ordinal))
                return feature;
            found = true;
            return feature with { IsOperational = isOperational };
        }).ToArray();

        if (!found)
            throw new KeyNotFoundException($"Unknown spatial feature '{featureId}'.");
        return new SpatialTopology(Regions, updated, Connections);
    }

    private static void EnsureUnique(IEnumerable<string> values, string parameterName)
    {
        var materialized = values.ToArray();
        if (materialized.Distinct(StringComparer.Ordinal).Count() != materialized.Length)
            throw new ArgumentException("Spatial identifiers must be unique within their collection.", parameterName);
    }
}
