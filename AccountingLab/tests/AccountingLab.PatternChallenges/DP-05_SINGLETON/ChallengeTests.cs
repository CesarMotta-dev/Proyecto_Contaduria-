using Xunit;
using AccountingLab.Domain.Challenges.DP05Singleton;

namespace AccountingLab.PatternChallenges.DP05Singleton;

[Trait("ChallengeId", "DP-05")]
public sealed class DP05SingletonTests
{
    [Fact]
    public void Repeated_acquisitions_share_identity_inside_this_process()
    {
        var service = new GlobalRegistryChallenge();
        var first = service.Acquire();
        Assert.Same(first, service.Acquire());
        Assert.Same(first, new GlobalRegistryChallenge().Acquire());
    }
    [Fact]
    public async Task Concurrent_acquisitions_observe_one_identity()
    {
        var results = await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => Task.Run(() => new GlobalRegistryChallenge().Acquire())));
        Assert.All(results, item => Assert.Same(results[0], item));
    }
    [Fact]
    public void Registry_is_explicit_about_immutable_global_scope_and_precision()
    {
        var registry = new GlobalRegistryChallenge().Acquire();
        Assert.Equal("DEMO-1", registry.Version);
        Assert.Equal(4, registry.MonetaryScale);
        Assert.Equal("process", registry.Scope);
        var alternativeValue = registry with { MonetaryScale = 2 };
        Assert.Equal(4, registry.MonetaryScale);
        Assert.NotSame(registry, alternativeValue);
    }
}

