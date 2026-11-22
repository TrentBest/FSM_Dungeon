using Xunit;
using TheSingularityWorkshop.FSM_Dungeon.MicroBundle;
using TheSingularityWorkshop.MicroBundleDomain;

namespace TheSingularityWorkshop.FSM_Dungeon.Tests;

public sealed class DungeonMicroBundleTests
{
    [Fact]
    public void Descriptor_IdentifiesUndergroundEnvironmentCapability()
    {
        var bundle = new DungeonMicroBundle();

        Assert.Equal(DungeonMicroBundleIdentity.Id, bundle.Descriptor.Id);
        Assert.Equal("0.1.0-alpha.1", bundle.Descriptor.Version);
        Assert.Contains(bundle.Descriptor.Providers,
            provider => provider.Id == "fsm-dungeon.underground-environment");
    }

    [Fact]
    public void Bundle_HasNoInhabitantOrCommunicationDependencies()
    {
        var bundle = new DungeonMicroBundle();

        Assert.Empty(bundle.Dependencies);
        Assert.Empty(bundle.Descriptor.Dependencies);
    }

    [Fact]
    public void Load_IsHostNeutralAndSideEffectFree()
    {
        var bundle = new DungeonMicroBundle();

        bundle.Load(new TestLoadContext());

        Assert.False(bundle.Arbitrate(new TestArbitrationContext(), 0));
    }

    private sealed class TestLoadContext : IMicroBundleLoadContext
    {
        public ulong RuntimeId => 1;

        public bool TryGetConfiguration(ulong bundleId, out ReadOnlyMemory<byte> configuration)
        {
            configuration = ReadOnlyMemory<byte>.Empty;
            return false;
        }
    }

    private sealed class TestArbitrationContext : IMicroBundleArbitrationContext
    {
        public ulong RuntimeId => 1;
        public IReadOnlyList<IMicroBundle> Bundles => Array.Empty<IMicroBundle>();
        public object? ExperienceContext => null;
    }
}
