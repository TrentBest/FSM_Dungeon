namespace TheSingularityWorkshop.FSM_Dungeon.Geology;

/// <summary>
/// Geological interpretation bound to an actual underground spatial topology.
/// This aggregate validates region references while keeping material definitions external.
/// </summary>
public sealed class GeologicalWorld
{
    public GeologicalWorld(SpatialTopology topology, IEnumerable<GeologicalProfile> profiles)
    {
        Topology = topology ?? throw new ArgumentNullException(nameof(topology));
        ArgumentNullException.ThrowIfNull(profiles);

        var materialized = profiles.ToArray();
        if (materialized.Length == 0)
            throw new ArgumentException("A geological world must contain at least one profile.", nameof(profiles));
        if (materialized.Any(profile => profile is null))
            throw new ArgumentException("A geological world cannot contain null profiles.", nameof(profiles));
        if (materialized.Select(profile => profile.Id).Distinct(StringComparer.Ordinal).Count() != materialized.Length)
            throw new ArgumentException("Geological profile identifiers must be unique.", nameof(profiles));

        var regionIds = topology.Regions.Select(region => region.Id).ToHashSet(StringComparer.Ordinal);
        var missingRegions = materialized
            .Where(profile => !regionIds.Contains(profile.RegionId))
            .Select(profile => profile.RegionId)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (missingRegions.Length > 0)
            throw new ArgumentException(
                $"Geological profiles reference unknown spatial regions: {string.Join(", ", missingRegions)}.",
                nameof(profiles));

        Profiles = Array.AsReadOnly(materialized);
    }

    public SpatialTopology Topology { get; }
    public IReadOnlyList<GeologicalProfile> Profiles { get; }

    /// <summary>Returns the geological profile attached to a region, or null if that region is uncharacterized.</summary>
    public GeologicalProfile? GetProfile(string regionId)
    {
        if (string.IsNullOrWhiteSpace(regionId))
            throw new ArgumentException("A region identifier is required.", nameof(regionId));

        return Profiles.SingleOrDefault(profile =>
            string.Equals(profile.RegionId, regionId, StringComparison.Ordinal));
    }

    /// <summary>Resolves a material reference at a region and elevation, or null for unknown geology/gaps.</summary>
    public MaterialReference? FindMaterialAt(string regionId, double elevation)
    {
        var profile = GetProfile(regionId);
        return profile?.FindLayerAt(elevation)?.Material;
    }
}
