namespace TheSingularityWorkshop.FSM_Dungeon;

/// <summary>Initial water state and geometry for one hydraulic chamber.</summary>
public sealed record HydraulicChamber
{
    public HydraulicChamber(int chamberIndex, double floorElevationMeters, double floorAreaSquareMeters,
        double initialWaterVolumeCubicMeters, double maximumWaterDepthMeters, double inflowCubicMetersPerSecond = 0)
    {
        if (chamberIndex < 0) throw new ArgumentOutOfRangeException(nameof(chamberIndex));
        RequireFiniteNonNegative(floorAreaSquareMeters, nameof(floorAreaSquareMeters), allowZero: false);
        RequireFiniteNonNegative(initialWaterVolumeCubicMeters, nameof(initialWaterVolumeCubicMeters));
        RequireFiniteNonNegative(maximumWaterDepthMeters, nameof(maximumWaterDepthMeters), allowZero: false);
        RequireFiniteNonNegative(inflowCubicMetersPerSecond, nameof(inflowCubicMetersPerSecond));
        if (!double.IsFinite(floorElevationMeters)) throw new ArgumentOutOfRangeException(nameof(floorElevationMeters));
        if (initialWaterVolumeCubicMeters > floorAreaSquareMeters * maximumWaterDepthMeters)
            throw new ArgumentException("Initial water volume exceeds chamber capacity.", nameof(initialWaterVolumeCubicMeters));

        ChamberIndex = chamberIndex;
        FloorElevationMeters = floorElevationMeters;
        FloorAreaSquareMeters = floorAreaSquareMeters;
        InitialWaterVolumeCubicMeters = initialWaterVolumeCubicMeters;
        MaximumWaterDepthMeters = maximumWaterDepthMeters;
        InflowCubicMetersPerSecond = inflowCubicMetersPerSecond;
    }

    public int ChamberIndex { get; }
    public double FloorElevationMeters { get; }
    public double FloorAreaSquareMeters { get; }
    public double InitialWaterVolumeCubicMeters { get; }
    public double MaximumWaterDepthMeters { get; }
    public double InflowCubicMetersPerSecond { get; }

    private static void RequireFiniteNonNegative(double value, string name, bool allowZero = true)
    {
        if (!double.IsFinite(value) || (allowZero ? value < 0 : value <= 0))
            throw new ArgumentOutOfRangeException(name);
    }
}

/// <summary>A hydraulic connection mapped to an existing dungeon passage.</summary>
public sealed record HydraulicPassage
{
    public HydraulicPassage(int fromChamberIndex, int toChamberIndex, double invertElevationMeters,
        double maximumFlowCubicMetersPerSecond, double conductanceCubicMetersPerSecondPerMeter,
        double blockageFraction = 0)
    {
        if (fromChamberIndex < 0) throw new ArgumentOutOfRangeException(nameof(fromChamberIndex));
        if (toChamberIndex < 0 || toChamberIndex == fromChamberIndex)
            throw new ArgumentOutOfRangeException(nameof(toChamberIndex));
        if (!double.IsFinite(invertElevationMeters)) throw new ArgumentOutOfRangeException(nameof(invertElevationMeters));
        if (!double.IsFinite(maximumFlowCubicMetersPerSecond) || maximumFlowCubicMetersPerSecond <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumFlowCubicMetersPerSecond));
        if (!double.IsFinite(conductanceCubicMetersPerSecondPerMeter) || conductanceCubicMetersPerSecondPerMeter <= 0)
            throw new ArgumentOutOfRangeException(nameof(conductanceCubicMetersPerSecondPerMeter));
        if (!double.IsFinite(blockageFraction) || blockageFraction is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(blockageFraction));

        FromChamberIndex = fromChamberIndex;
        ToChamberIndex = toChamberIndex;
        InvertElevationMeters = invertElevationMeters;
        MaximumFlowCubicMetersPerSecond = maximumFlowCubicMetersPerSecond;
        ConductanceCubicMetersPerSecondPerMeter = conductanceCubicMetersPerSecondPerMeter;
        BlockageFraction = blockageFraction;
    }

    public int FromChamberIndex { get; }
    public int ToChamberIndex { get; }
    public double InvertElevationMeters { get; }
    public double MaximumFlowCubicMetersPerSecond { get; }
    public double ConductanceCubicMetersPerSecondPerMeter { get; }
    public double BlockageFraction { get; }
}

/// <summary>Validated, deterministic hydraulic inputs for a subset of a dungeon's passages.</summary>
public sealed class HydraulicNetworkDefinition
{
    public HydraulicNetworkDefinition(IEnumerable<HydraulicChamber> chambers, IEnumerable<HydraulicPassage> passages)
    {
        ArgumentNullException.ThrowIfNull(chambers);
        ArgumentNullException.ThrowIfNull(passages);
        Chambers = Array.AsReadOnly(chambers.ToArray());
        Passages = Array.AsReadOnly(passages.ToArray());
        if (Chambers.Count == 0) throw new ArgumentException("At least one hydraulic chamber is required.", nameof(chambers));
        if (Chambers.Select(x => x.ChamberIndex).Distinct().Count() != Chambers.Count)
            throw new ArgumentException("Hydraulic chamber indices must be unique.", nameof(chambers));
        var ids = Chambers.Select(x => x.ChamberIndex).ToHashSet();
        if (Passages.Any(x => !ids.Contains(x.FromChamberIndex) || !ids.Contains(x.ToChamberIndex)))
            throw new ArgumentException("Every hydraulic passage must reference declared chambers.", nameof(passages));
        var edges = Passages.Select(x => x.FromChamberIndex < x.ToChamberIndex
            ? (x.FromChamberIndex, x.ToChamberIndex) : (x.ToChamberIndex, x.FromChamberIndex));
        if (edges.Distinct().Count() != Passages.Count)
            throw new ArgumentException("A hydraulic chamber pair may have only one hydraulic passage.", nameof(passages));
    }

    public IReadOnlyList<HydraulicChamber> Chambers { get; }
    public IReadOnlyList<HydraulicPassage> Passages { get; }
}
