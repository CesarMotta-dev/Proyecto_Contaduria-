using Xunit;
using AccountingLab.Domain.Challenges.DP17Memento;

namespace AccountingLab.PatternChallenges.DP17Memento;

[Trait("ChallengeId", "DP-17")]
public sealed class DP17MementoTests
{
    private static BudgetDraft Draft() => new()
    {
        Id = Guid.NewGuid(), Name = "Inicial", Lines = new() { new() { AccountCode = "5105", Amount = 100m } }
    };
    [Fact]
    public void Restores_previous_budget_draft_values()
    {
        var service = new BudgetHistoryChallenge();
        var draft = Draft();
        var snapshot = service.Save(draft);
        draft.Name = "Modificado";
        draft.Lines[0].Amount = 200m;
        var restored = service.Restore(draft, snapshot);
        Assert.Equal("Inicial", restored.Name);
        Assert.Equal(100m, restored.Lines[0].Amount);
        Assert.Equal(draft.Id, restored.Id);
    }
    [Fact]
    public void Approved_budget_cannot_create_editing_snapshot()
    {
        var draft = Draft();
        draft.Approved = true;
        Assert.Throws<InvalidOperationException>(() => new BudgetHistoryChallenge().Save(draft));
    }
    [Fact]
    public void Snapshot_remains_independent_when_nested_draft_data_changes()
    {
        var service = new BudgetHistoryChallenge();
        var draft = Draft();
        var snapshot = service.Save(draft);
        draft.Lines.Clear();
        var restored = service.Restore(draft, snapshot);
        Assert.Single(restored.Lines);
        Assert.Equal("5105", restored.Lines[0].AccountCode);
        restored.Lines[0].Amount = 500m;
        var secondRestoration = service.Restore(draft, snapshot);
        Assert.Equal(100m, secondRestoration.Lines[0].Amount);
    }
}

