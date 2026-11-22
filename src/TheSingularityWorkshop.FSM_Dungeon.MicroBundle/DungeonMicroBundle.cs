using TheSingularityWorkshop.MicroBundleDomain;

namespace TheSingularityWorkshop.FSM_Dungeon.MicroBundle;

/// <summary>
/// Stable identity shared by the dungeon capability and SURPS rulebook compositions.
/// The numeric value is the FNV-1a 64-bit hash of the canonical identity string.
/// </summary>
public static class DungeonMicroBundleIdentity
{
    public const string CanonicalId = "TheSingularityWorkshop.FSM_Dungeon";
    public const ulong Id = 0xC0C3B15E31815313UL;
}

/// <summary>
/// Exposes FSM_Dungeon as a host-neutral underground-environment MicroBundle.
/// </summary>
/// <remarks>
/// The bundle identifies and exposes the domain capability. The current MicroBundle
/// load contract has no domain-service registration API, so loading is intentionally
/// side-effect-free; hosts consume the referenced FSM_Dungeon domain assembly through
/// their resolved composition rather than having Dungeon depend on a host.
/// </remarks>
public sealed class DungeonMicroBundle : IMicroBundle
{
    public MicroBundleDescriptor Descriptor { get; } = new(
        DungeonMicroBundleIdentity.Id,
        "0.1.0-alpha.1",
        providers:
        [
            new MicroBundleProvider("fsm-dungeon.underground-environment")
        ]);

    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies { get; } =
        Array.Empty<MicroBundleDependencyRequest>();

    public void Load(IMicroBundleLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
    }

    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentOutOfRangeException.ThrowIfNegative(roundIndex);
        return false;
    }
}
