using Xunit;
using AccountingLab.Application.Challenges.DP15Iterator;

namespace AccountingLab.PatternChallenges.DP15Iterator;

[Trait("ChallengeId", "DP-15")]
public sealed class DP15IteratorTests
{
    [Fact]
    public void Visits_all_records_across_pages_in_stable_order()
    {
        var first = new LedgerRecord("A-1", new(2026, 1, 1), 1m, 0m);
        var second = new LedgerRecord("A-2", new(2026, 1, 2), 0m, 1m);
        var cursor = new LedgerTraversalChallenge().Open(new[] { new LedgerPage(new[] { first }), new LedgerPage(Array.Empty<LedgerRecord>()), new LedgerPage(new[] { second }) });
        var visited = new List<string>();
        while (cursor.MoveNext()) visited.Add(cursor.Current.EntryCode);
        Assert.Equal(new[] { "A-1", "A-2" }, visited);
    }
    [Fact]
    public void Current_is_invalid_outside_a_valid_position()
    {
        var cursor = new LedgerTraversalChallenge().Open(new[] { new LedgerPage(new[] { new LedgerRecord("A-1", new(2026, 1, 1), 1m, 0m) }) });
        Assert.Throws<InvalidOperationException>(() => cursor.Current);
        Assert.True(cursor.MoveNext());
        Assert.False(cursor.MoveNext());
        Assert.Throws<InvalidOperationException>(() => cursor.Current);
    }
    [Fact]
    public void Empty_ledger_terminates_without_inventing_records()
    {
        var cursor = new LedgerTraversalChallenge().Open(Array.Empty<LedgerPage>());
        Assert.False(cursor.MoveNext());
        Assert.False(cursor.MoveNext());
    }
}

