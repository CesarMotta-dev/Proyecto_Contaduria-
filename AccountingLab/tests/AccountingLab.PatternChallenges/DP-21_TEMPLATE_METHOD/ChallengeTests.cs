using Xunit;
using AccountingLab.Application.Challenges.DP21TemplateMethod;

namespace AccountingLab.PatternChallenges.DP21TemplateMethod;

[Trait("ChallengeId", "DP-21")]
public sealed class DP21TemplateMethodTests
{
    [Fact]
    public void Exact_mode_matches_equal_movements_and_reports_unmatched_items()
    {
        var result = new BankReconciliationChallenge().Reconcile(new("exact",
            new[] { new BankStatementItem("B-1", "R-1", new(2026, 1, 1), 50m), new BankStatementItem("B-2", "R-2", new(2026, 1, 2), 10m) },
            new[] { new ReconciliationLedgerItem("L-1", "R-1", new(2026, 1, 1), 50m) }, 0m));
        var match = Assert.Single(result.Matches);
        Assert.Equal("B-1", match.BankItemId);
        Assert.Equal("L-1", match.LedgerItemId);
        Assert.Equal(0m, match.Difference);
        Assert.Equal(new[] { "B-2" }, result.UnmatchedBankItems);
        Assert.Empty(result.UnmatchedLedgerItems);
    }
    [Fact]
    public void Negative_tolerance_is_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BankReconciliationChallenge().Reconcile(new("tolerance", Array.Empty<BankStatementItem>(), Array.Empty<ReconciliationLedgerItem>(), -0.0001m)));
    }
    [Fact]
    public void Tolerance_boundary_is_inclusive_without_reusing_a_movement()
    {
        var result = new BankReconciliationChallenge().Reconcile(new("tolerance",
            new[] { new BankStatementItem("B-1", "R-1", new(2026, 1, 1), 50.0001m), new BankStatementItem("B-2", "R-1", new(2026, 1, 1), 50.0001m) },
            new[] { new ReconciliationLedgerItem("L-1", "R-1", new(2026, 1, 1), 50m) }, 0.0001m));
        Assert.Single(result.Matches);
        Assert.Equal(0.0001m, result.Matches[0].Difference);
        Assert.Single(result.UnmatchedBankItems);
        Assert.Empty(result.UnmatchedLedgerItems);
    }
}

