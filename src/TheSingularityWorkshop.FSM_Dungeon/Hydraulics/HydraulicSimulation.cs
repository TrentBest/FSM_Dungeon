using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.FSM_Dungeon;

public sealed record HydraulicChamberState(int ChamberIndex, double WaterVolumeCubicMeters,
    double WaterSurfaceElevationMeters, double InflowCubicMetersPerSecond);

public sealed record HydraulicTransfer(int FromChamberIndex, int ToChamberIndex,
    double VolumeCubicMeters, double FlowRateCubicMetersPerSecond);

public sealed class HydraulicSnapshot
{
    internal HydraulicSnapshot(long tick, double elapsedSeconds, IEnumerable<HydraulicChamberState> chambers,
        IEnumerable<HydraulicTransfer> transfers)
    {
        Tick = tick;
        ElapsedSeconds = elapsedSeconds;
        Chambers = new ReadOnlyCollection<HydraulicChamberState>(chambers.ToArray());
        Transfers = new ReadOnlyCollection<HydraulicTransfer>(transfers.ToArray());
    }
    public long Tick { get; }
    public double ElapsedSeconds { get; }
    public IReadOnlyList<HydraulicChamberState> Chambers { get; }
    public IReadOnlyList<HydraulicTransfer> Transfers { get; }
}

/// <summary>
/// Deterministic, conservative first-order hydraulic solver. Flow is proportional to head difference,
/// bounded by passage capacity, reduced by blockage, and limited to available source water.
/// </summary>
public sealed class HydraulicSimulation
{
    private readonly HydraulicNetworkDefinition _definition;
    private readonly Dictionary<int, double> _volumes;
    private long _tick;
    private double _elapsedSeconds;

    public HydraulicSimulation(HydraulicNetworkDefinition definition)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        _volumes = definition.Chambers.ToDictionary(x => x.ChamberIndex, x => x.InitialWaterVolumeCubicMeters);
    }

    public HydraulicSnapshot Current => CreateSnapshot([]);

    public HydraulicSnapshot Step(double durationSeconds)
    {
        if (!double.IsFinite(durationSeconds) || durationSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds));

        var chambers = _definition.Chambers.ToDictionary(x => x.ChamberIndex);
        var working = new Dictionary<int, double>(_volumes);
        foreach (var chamber in _definition.Chambers)
        {
            var capacity = chamber.FloorAreaSquareMeters * chamber.MaximumWaterDepthMeters;
            working[chamber.ChamberIndex] = Math.Min(capacity,
                working[chamber.ChamberIndex] + chamber.InflowCubicMetersPerSecond * durationSeconds);
        }

        var proposals = new List<(int From, int To, double Volume)>();
        foreach (var passage in _definition.Passages)
        {
            var from = chambers[passage.FromChamberIndex];
            var to = chambers[passage.ToChamberIndex];
            var fromSurface = from.FloorElevationMeters + working[from.ChamberIndex] / from.FloorAreaSquareMeters;
            var toSurface = to.FloorElevationMeters + working[to.ChamberIndex] / to.FloorAreaSquareMeters;
            var difference = fromSurface - toSurface;
            if (Math.Abs(difference) <= 1e-12 || passage.BlockageFraction >= 1) continue;

            var source = difference > 0 ? from : to;
            var destination = difference > 0 ? to : from;
            var sourceSurface = difference > 0 ? fromSurface : toSurface;
            var destinationSurface = difference > 0 ? toSurface : fromSurface;
            if (sourceSurface <= passage.InvertElevationMeters) continue;

            var effectiveCapacity = passage.MaximumFlowCubicMetersPerSecond * (1 - passage.BlockageFraction);
            var rate = Math.Min(effectiveCapacity,
                passage.ConductanceCubicMetersPerSecondPerMeter * (sourceSurface - destinationSurface));
            var volume = Math.Min(rate * durationSeconds, working[source.ChamberIndex]);
            if (volume > 0) proposals.Add((source.ChamberIndex, destination.ChamberIndex, volume));
        }

        // Simultaneous proposals are scaled per source so a node with many outlets cannot overdraw.
        var outgoing = proposals.GroupBy(x => x.From).ToDictionary(g => g.Key, g => g.Sum(x => x.Volume));
        var scale = outgoing.ToDictionary(x => x.Key,
            x => x.Value > working[x.Key] && x.Value > 0 ? working[x.Key] / x.Value : 1d);
        var transfers = new List<HydraulicTransfer>(proposals.Count);
        foreach (var proposal in proposals)
        {
            var moved = proposal.Volume * scale[proposal.From];
            working[proposal.From] -= moved;
            working[proposal.To] += moved;
            transfers.Add(new HydraulicTransfer(proposal.From, proposal.To, moved, moved / durationSeconds));
        }

        foreach (var chamber in _definition.Chambers)
            working[chamber.ChamberIndex] = Math.Min(working[chamber.ChamberIndex],
                chamber.FloorAreaSquareMeters * chamber.MaximumWaterDepthMeters);

        _volumes.Clear();
        foreach (var pair in working) _volumes.Add(pair.Key, pair.Value);
        _tick++;
        _elapsedSeconds += durationSeconds;
        return CreateSnapshot(transfers);
    }

    private HydraulicSnapshot CreateSnapshot(IEnumerable<HydraulicTransfer> transfers)
    {
        var states = _definition.Chambers.Select(chamber =>
        {
            var volume = _volumes[chamber.ChamberIndex];
            return new HydraulicChamberState(chamber.ChamberIndex, volume,
                chamber.FloorElevationMeters + volume / chamber.FloorAreaSquareMeters,
                chamber.InflowCubicMetersPerSecond);
        });
        return new HydraulicSnapshot(_tick, _elapsedSeconds, states, transfers);
    }
}
