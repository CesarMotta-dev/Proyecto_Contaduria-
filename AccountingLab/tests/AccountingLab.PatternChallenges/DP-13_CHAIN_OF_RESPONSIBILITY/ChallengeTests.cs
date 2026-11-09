using Xunit;
using AccountingLab.Domain.Challenges.DP13ChainOfResponsibility;

namespace AccountingLab.PatternChallenges.DP13ChainOfResponsibility;

[Trait("ChallengeId", "DP-13")]
public sealed class DP13ChainOfResponsibilityTests
{
    [Fact]
    public void Approves_request_using_configured_order_of_responsibility()
    {
        var checks = new[] { "politica", "fondos", "limite" };
        var result = new PaymentApprovalChallenge().Evaluate(new(50m, 100m, true, 75m, checks));
        Assert.True(result.Approved);
        Assert.Null(result.RejectedBy);
        Assert.Equal(checks, result.VisitedChecks);
    }
    [Fact]
    public void Stops_at_first_business_rejection()
    {
        var result = new PaymentApprovalChallenge().Evaluate(new(101m, 100m, true, 200m, new[] { "fondos", "politica", "limite" }));
        Assert.False(result.Approved);
        Assert.Equal("fondos", result.RejectedBy);
        Assert.Equal(new[] { "fondos" }, result.VisitedChecks);
    }
    [Fact]
    public void Exact_funds_and_authorization_boundary_are_admissible()
    {
        var result = new PaymentApprovalChallenge().Evaluate(new(100m, 100m, true, 100m, new[] { "limite", "fondos", "politica" }));
        Assert.True(result.Approved);
        Assert.Equal(new[] { "limite", "fondos", "politica" }, result.VisitedChecks);
    }
}

