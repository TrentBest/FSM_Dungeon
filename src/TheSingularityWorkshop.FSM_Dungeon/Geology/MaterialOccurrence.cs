namespace TheSingularityWorkshop.FSM_Dungeon.Geology;

/// <summary>
/// An axis-aligned volume in the same coordinate system as its containing spatial region.
/// Minimum and maximum coordinates are inclusive.
/// </summary>
public sealed record SpatialBounds3
{
    public SpatialBounds3(SpatialPoint3 minimum, SpatialPoint3 maximum)
    {
        if (!minimum.IsFinite || !maximum.IsFinite)
            throw new ArgumentException("Spatial bounds must use finite coordinates.");
        if (maximum.X <= minimum.X || maximum.Y <= minimum.Y || maximum.Z <= minimum.Z)
            throw new ArgumentException("Spatial bounds must have positive size on every axis.");

        Minimum = minimum;
        Maximum = maximum;
    }

    public SpatialPoint3 Minimum { get; }
    public SpatialPoint3 Maximum { get; }

    /// <summary>Returns whether a point lies inside or on this occurrence's bounds.</summary>
    public bool Contains(SpatialPoint3 point)
    {
        if (!point.IsFinite)
            throw new ArgumentOutOfRangeException(nameof(point), "Query coordinates must be finite.");
        return point.X >= Minimum.X && point.X <= Maximum.X
            && point.Y >= Minimum.Y && point.Y <= Maximum.Y
            && point.Z >= Minimum.Z && point.Z <= Maximum.Z;
    }

    /// <summary>Returns whether two bounds overlap or touch on all three axes.</summary>
    public bool Intersects(SpatialBounds3 other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Minimum.X <= other.Maximum.X && Maximum.X >= other.Minimum.X
            && Minimum.Y <= other.Maximum.Y && Maximum.Y >= other.Minimum.Y
            && Minimum.Z <= other.Maximum.Z && Maximum.Z >= other.Minimum.Z;
    }

    public bool IsContainedBy(SpatialRegion region)
    {
        ArgumentNullException.ThrowIfNull(region);
        var regionMaximum = new SpatialPoint3(
            region.Origin.X + region.Extent.Width,
            region.Origin.Y + region.Extent.Height,
            region.Origin.Z + region.Extent.Depth);

        return Minimum.X >= region.Origin.X && Minimum.Y >= region.Origin.Y
            && Minimum.Z >= region.Origin.Z && Maximum.X <= regionMaximum.X
            && Maximum.Y <= regionMaximum.Y && Maximum.Z <= regionMaximum.Z;
    }
}

/// <summary>A survey observation estimating the quantity of material in an occurrence.</summary>
/// <remarks>This is an estimate with an explicit unit and method, not a reserve or economic-viability claim.</remarks>
public sealed record MaterialQuantityEstimate
{
    public MaterialQuantityEstimate(double value, string unit, string method, double confidence)
    {
        if (!double.IsFinite(value) || value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Quantity must be finite and non-negative.");
        Value = value;
        Unit = Required(unit, nameof(unit));
        Method = Required(method, nameof(method));
        Confidence = ValidConfidence(confidence);
    }

    public double Value { get; }
    public string Unit { get; }
    public string Method { get; }
    public double Confidence { get; }

    private static double ValidConfidence(double value) =>
        double.IsFinite(value) && value >= 0 && value <= 1
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), "Confidence must be between zero and one.");

    private static string Required(string value, string parameterName) =>
        !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw new ArgumentException("A meaningful value is required.", parameterName);
}

/// <summary>A measured or interpreted material grade, with its basis and method kept explicit.</summary>
/// <remarks>Grade units are open strings (for example percent, ppm, or grams per tonne).</remarks>
public sealed record MaterialGradeObservation
{
    public MaterialGradeObservation(double value, string unit, string basis, string method, double confidence)
    {
        if (!double.IsFinite(value) || value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Grade must be finite and non-negative.");
        Value = value;
        Unit = Required(unit, nameof(unit));
        Basis = Required(basis, nameof(basis));
        Method = Required(method, nameof(method));
        if (!double.IsFinite(confidence) || confidence < 0 || confidence > 1)
            throw new ArgumentOutOfRangeException(nameof(confidence), "Confidence must be between zero and one.");
        Confidence = confidence;
    }

    public double Value { get; }
    public string Unit { get; }
    public string Basis { get; }
    public string Method { get; }
    public double Confidence { get; }

    private static string Required(string value, string parameterName) =>
        !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw new ArgumentException("A meaningful value is required.", parameterName);
}

/// <summary>
/// A spatially bounded occurrence of one externally defined material, such as a vein, seam, lens, or pocket.
/// Occurrence geometry and survey observations belong here, not on the material definition.
/// </summary>
public sealed class MaterialOccurrence
{
    public MaterialOccurrence(
        string id,
        string name,
        string occurrenceType,
        string regionId,
        MaterialReference material,
        SpatialBounds3 bounds,
        string formationContext,
        IEnumerable<MaterialGradeObservation>? gradeObservations = null,
        IEnumerable<MaterialQuantityEstimate>? quantityEstimates = null)
    {
        Id = Required(id, nameof(id));
        Name = Required(name, nameof(name));
        OccurrenceType = Required(occurrenceType, nameof(occurrenceType));
        RegionId = Required(regionId, nameof(regionId));
        Material = material ?? throw new ArgumentNullException(nameof(material));
        Bounds = bounds ?? throw new ArgumentNullException(nameof(bounds));
        FormationContext = Required(formationContext, nameof(formationContext));

        var grades = (gradeObservations ?? []).ToArray();
        var quantities = (quantityEstimates ?? []).ToArray();
        if (grades.Any(item => item is null))
            throw new ArgumentException("Grade observations cannot contain null entries.", nameof(gradeObservations));
        if (quantities.Any(item => item is null))
            throw new ArgumentException("Quantity estimates cannot contain null entries.", nameof(quantityEstimates));

        GradeObservations = Array.AsReadOnly(grades);
        QuantityEstimates = Array.AsReadOnly(quantities);
    }

    public string Id { get; }
    public string Name { get; }
    public string OccurrenceType { get; }
    public string RegionId { get; }
    public MaterialReference Material { get; }
    public SpatialBounds3 Bounds { get; }
    public string FormationContext { get; }
    public IReadOnlyList<MaterialGradeObservation> GradeObservations { get; }
    public IReadOnlyList<MaterialQuantityEstimate> QuantityEstimates { get; }

    private static string Required(string value, string parameterName) =>
        !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw new ArgumentException("A meaningful value is required.", parameterName);
}
