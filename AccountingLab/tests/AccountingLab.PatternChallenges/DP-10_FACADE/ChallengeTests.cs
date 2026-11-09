using Xunit;
using AccountingLab.Application.Challenges.DP10Facade;

namespace AccountingLab.PatternChallenges.DP10Facade;

[Trait("ChallengeId", "DP-10")]
public sealed class DP10FacadeTests
{
    [Fact]
    public void Closes_consistent_period_with_consolidated_totals()
    {
        var company = Guid.NewGuid();
        var result = new PeriodClosingChallenge().Close(new(company, new(2026, 1, 1), new(2026, 1, 31),
            new[] { new ClosingAccountBalance("1105", 100m, 0m), new ClosingAccountBalance("3105", 0m, 100m) }, 0));
        Assert.True(result.Closed);
        Assert.Equal(company, result.CompanyId);
        Assert.Equal(100m, result.TotalDebit);
        Assert.Equal(result.TotalDebit, result.TotalCredit);
    }
    [Fact]
    public void Rejects_period_with_unposted_documents()
    {
        Assert.Throws<InvalidOperationException>(() => new PeriodClosingChallenge().Close(new(Guid.NewGuid(), new(2026, 1, 1), new(2026, 1, 31), Array.Empty<ClosingAccountBalance>(), 1)));
    }
    [Fact]
    public void Rejects_smallest_supported_decimal_imbalance()
    {
        var balances = new[] { new ClosingAccountBalance("1105", 0.0001m, 0m) };
        Assert.Throws<InvalidOperationException>(() => new PeriodClosingChallenge().Close(new(Guid.NewGuid(), new(2026, 1, 1), new(2026, 1, 31), balances, 0)));
    }
}

