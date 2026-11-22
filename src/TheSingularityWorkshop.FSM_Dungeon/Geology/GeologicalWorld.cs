namespace TheSingularityWorkshop.FSM_Dungeon.Geology;

/// <summary>
/// Geological interpretation and material occurrences bound to an actual underground spatial topology.
/// Material definitions remain external; this aggregate owns their spatial context.
/// </summary>
public sealed class GeologicalWorld
{
    public GeologicalWorld(
        SpatialTopology topology,
        IEnumerable<GeologicalProfile> profiles,
        IEnumerable<MaterialOccurrence>? occurrences = null)
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

        var regionById = topology.Regions.ToDictionary(region => region.Id, StringComparer.Ordinal);
        var missingRegions = materialized
            .Where(profile => !regionById.ContainsKey(profile.RegionId))
            .Select(profile => profile.RegionId)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (missingRegions.Length > 0)
            throw new ArgumentException(
                $"Geological profiles reference unknown spatial regions: {string.Join(", ", missingRegions)}.",
                nameof(profiles));

        var materialOccurrences = (occurrences ?? []).ToArray();
        if (materialOccurrences.Any(occurrence => occurrence is null))
            throw new ArgumentException("A geological world cannot contain null material occurrences.", nameof(occurrences));
        if (materialOccurrences.Select(occurrence => occurrence.Id).Distinct(StringComparer.Ordinal).Count()
            != materialOccurrences.Length)
            throw new ArgumentException("Material occurrence identifiers must be unique.", nameof(occurrences));

        foreach (var occurrence in materialOccurrences)
        {
            if (!regionById.TryGetValue(occurrence.RegionId, out var region))
                throw new ArgumentException(
                    $"Material occurrence '{occurrence.Id}' references unknown region '{occurrence.RegionId}'.",
                    nameof(occurrences));
            if (!occurrence.Bounds.IsContainedBy(region))
                throw new ArgumentException(
                    $"Material occurrence '{occurrence.Id}' extends beyond spatial region '{region.Id}'.",
                    nameof(occurrences));
        }

        Profiles = Array.AsReadOnly(materialized);
        Occurrences = Array.AsReadOnly(materialOccurrences);
    }

    public SpatialTopology Topology { get; }
    public IReadOnlyList<GeologicalProfile> Profiles { get; }
    public IReadOnlyList<MaterialOccurrence> Occurrences { get; }

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

    /// <summary>Returns all bounded material occurrences within a region.</summary>
    public IReadOnlyList<MaterialOccurrence> FindOccurrences(string regionId)
    {
        if (string.IsNullOrWhiteSpace(regionId))
            throw new ArgumentException("A region identifier is required.", nameof(regionId));
        return Array.AsReadOnly(Occurrences
            .Where(occurrence => string.Equals(occurrence.RegionId, regionId, StringComparison.Ordinal))
            .ToArray());
    }

    /// <summary>Returns occurrences of a material identity within a region.</summary>
    public IReadOnlyList<MaterialOccurrence> FindOccurrences(string regionId, MaterialReference material)
    {
        ArgumentNullException.ThrowIfNull(material);
        return Array.AsReadOnly(FindOccurrences(regionId)
            .Where(occurrence => string.Equals(occurrence.Material.Domain, material.Domain, StringComparison.Ordinal)
                && string.Equals(occurrence.Material.Id, material.Id, StringComparison.Ordinal))
            .ToArray());
    }
}
