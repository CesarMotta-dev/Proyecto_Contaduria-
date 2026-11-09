using Xunit;
using AccountingLab.Domain.Challenges.DP02AbstractFactory;

namespace AccountingLab.PatternChallenges.DP02AbstractFactory;

[Trait("ChallengeId", "DP-02")]
public sealed class DP02AbstractFactoryTests
{
    [Fact]
    public void Produces_compatible_services_for_one_company_profile()
    {
        var result = new AccountingProfileChallenge().CreateFamily(new("comercial", "DEMO-A"));
        Assert.Equal("comercial", result.Profile);
        Assert.Equal("DEMO-A", result.FictionalRegime);
        Assert.Equal(new[] { "calculo", "validacion", "reporte" }, result.Members.Select(x => x.Role));
        Assert.Single(result.Members.Select(x => x.CompatibilityKey).Distinct());
    }
    [Fact]
    public void Rejects_unknown_company_profile()
    {
        Assert.Throws<ArgumentException>(() => new AccountingProfileChallenge().CreateFamily(new("inexistente", "DEMO-A")));
    }
    [Fact]
    public void Different_profiles_do_not_mix_compatibility_keys()
    {
        var service = new AccountingProfileChallenge();
        var commercial = service.CreateFamily(new("comercial", "DEMO-A"));
        var services = service.CreateFamily(new("servicios", "DEMO-A"));
        Assert.NotEqual(commercial.Members[0].CompatibilityKey, services.Members[0].CompatibilityKey);
        Assert.All(services.Members, x => Assert.Equal(services.Members[0].CompatibilityKey, x.CompatibilityKey));
    }
}

