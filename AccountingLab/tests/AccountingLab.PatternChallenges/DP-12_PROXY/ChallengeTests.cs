using Xunit;
using AccountingLab.Application.Challenges.DP12Proxy;

namespace AccountingLab.PatternChallenges.DP12Proxy;

[Trait("ChallengeId", "DP-12")]
public sealed class DP12ProxyTests
{
    [Fact]
    public void Authorized_query_obtains_report_from_requested_company()
    {
        var company = Guid.NewGuid();
        var reads = 0;
        var result = new SensitiveReportChallenge().Get(new(company, true, "confidencial"), () => { reads++; return new(company, "confidencial", 55.1250m); });
        Assert.Equal(company, result.CompanyId);
        Assert.Equal(55.1250m, result.ConfidentialAmount);
        Assert.Equal(1, reads);
    }
    [Fact]
    public void Denied_query_never_reads_sensitive_source()
    {
        var reads = 0;
        Assert.Throws<UnauthorizedAccessException>(() => new SensitiveReportChallenge().Get(new(Guid.NewGuid(), false, "confidencial"), () => { reads++; return new(Guid.NewGuid(), "confidencial", 100m); }));
        Assert.Equal(0, reads);
    }
    [Fact]
    public void Rejects_source_result_belonging_to_another_company()
    {
        var request = new SensitiveReportRequest(Guid.NewGuid(), true, "confidencial");
        Assert.Throws<InvalidOperationException>(() => new SensitiveReportChallenge().Get(request, () => new(Guid.NewGuid(), "confidencial", 100m)));
    }
}

