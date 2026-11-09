using Xunit;
using AccountingLab.Domain.Challenges.DP08Composite;

namespace AccountingLab.PatternChallenges.DP08Composite;

[Trait("ChallengeId", "DP-08")]
public sealed class DP08CompositeTests
{
    [Fact]
    public void Returns_balance_for_nested_groups_and_individual_accounts()
    {
        var cash = new AccountHierarchy("1105", true, 25m, Array.Empty<AccountHierarchy>());
        var bank = new AccountHierarchy("1110", true, 75.1250m, Array.Empty<AccountHierarchy>());
        var current = new AccountHierarchy("11", false, 0m, new[] { cash, bank });
        var assets = new AccountHierarchy("1", false, 0m, new[] { current });
        var service = new AccountTreeChallenge();
        Assert.Equal(100.1250m, service.Total(assets));
        Assert.Equal(25m, service.Total(cash));
    }
    [Fact]
    public void Rejects_posting_account_containing_child_accounts()
    {
        var child = new AccountHierarchy("110505", true, 10m, Array.Empty<AccountHierarchy>());
        Assert.Throws<ArgumentException>(() => new AccountTreeChallenge().Total(new("1105", true, 5m, new[] { child })));
    }
    [Fact]
    public void Empty_group_has_zero_balance()
    {
        Assert.Equal(0m, new AccountTreeChallenge().Total(new("1", false, 0m, Array.Empty<AccountHierarchy>())));
    }
}

