namespace TheSingularityWorkshop.FSM_Dungeon.Geology;

/// <summary>
/// An opaque reference to a material definition owned by another domain.
/// FSM_Dungeon records where a material occurs; it does not redefine the material.
/// </summary>
public sealed record MaterialReference
{
    public MaterialReference(string domain, string id)
    {
        Domain = Required(domain, nameof(domain));
        Id = Required(id, nameof(id));
    }

    public string Domain { get; }
    public string Id { get; }

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A meaningful value is required.", parameterName);
        return value.Trim();
    }
}

/// <summary>
/// A geological stratum in a profile. Elevations use the profile's declared vertical unit and datum.
/// </summary>
public sealed record GeologicalLayer
{
    public GeologicalLayer(
        string id,
        string name,
        MaterialReference material,
        double topElevation,
        double bottomElevation,
        string formationProcess,
        string relativeAge)
    {
        Id = Required(id, nameof(id));
        Name = Required(name, nameof(name));
        Material = material ?? throw new ArgumentNullException(nameof(material));
        if (!double.IsFinite(topElevation))
            throw new ArgumentOutOfRangeException(nameof(topElevation));
        if (!double.IsFinite(bottomElevation))
            throw new ArgumentOutOfRangeException(nameof(bottomElevation));
        if (topElevation <= bottomElevation)
            throw new ArgumentException("A geological layer must have positive vertical thickness.");
        TopElevation = topElevation;
        BottomElevation = bottomElevation;
        FormationProcess = Required(formationProcess, nameof(formationProcess));
        RelativeAge = Required(relativeAge, nameof(relativeAge));
    }

    public string Id { get; }
    public string Name { get; }
    public MaterialReference Material { get; }
    public double TopElevation { get; }
    public double BottomElevation { get; }
    public double Thickness => TopElevation - BottomElevation;
    public string FormationProcess { get; }
    public string RelativeAge { get; }

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A meaningful value is required.", parameterName);
        return value.Trim();
    }
}

/// <summary>
/// An ordered vertical geological section at a location in an underground world.
/// Layers are ordered from highest to lowest elevation. Gaps are permitted to represent
/// unconformities, erosion, or intervals that have not been characterized.
/// </summary>
public sealed class GeologicalProfile
{
    public GeologicalProfile(
        string id,
        string regionId,
        string verticalUnit,
        string datum,
        IEnumerable<GeologicalLayer> layers)
    {
        Id = Required(id, nameof(id));
        RegionId = Required(regionId, nameof(regionId));
        VerticalUnit = Required(verticalUnit, nameof(verticalUnit));
        Datum = Required(datum, nameof(datum));
        ArgumentNullException.ThrowIfNull(layers);

        var materialized = layers.ToArray();
        if (materialized.Length == 0)
            throw new ArgumentException("A geological profile must contain at least one layer.", nameof(layers));
        if (materialized.Any(layer => layer is null))
            throw new ArgumentException("A geological profile cannot contain null layers.", nameof(layers));
        if (materialized.Select(layer => layer.Id).Distinct(StringComparer.Ordinal).Count() != materialized.Length)
            throw new ArgumentException("Geological layer identifiers must be unique within a profile.", nameof(layers));

        for (var index = 1; index < materialized.Length; index++)
        {
            if (materialized[index].TopElevation > materialized[index - 1].BottomElevation)
                throw new ArgumentException(
                    "Layers must be ordered from highest to lowest elevation and cannot overlap.",
                    nameof(layers));
        }

        Layers = Array.AsReadOnly(materialized);
    }

    public string Id { get; }
    public string RegionId { get; }
    public string VerticalUnit { get; }
    public string Datum { get; }
    public IReadOnlyList<GeologicalLayer> Layers { get; }

    /// <summary>Finds the layer containing an elevation, or null when it lies in a gap/outside the profile.</summary>
    public GeologicalLayer? FindLayerAt(double elevation)
    {
        if (!double.IsFinite(elevation))
            throw new ArgumentOutOfRangeException(nameof(elevation));

        return Layers.FirstOrDefault(layer =>
            elevation <= layer.TopElevation && elevation >= layer.BottomElevation);
    }

    private static string Required(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A meaningful value is required.", parameterName);
        return value.Trim();
    }
}
