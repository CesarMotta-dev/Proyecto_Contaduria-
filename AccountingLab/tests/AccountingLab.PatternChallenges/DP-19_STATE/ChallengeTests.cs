using Xunit;
using AccountingLab.Domain.Challenges.DP19State;

namespace AccountingLab.PatternChallenges.DP19State;

[Trait("ChallengeId", "DP-19")]
public sealed class DP19StateTests
{
    [Fact]
    public void Draft_emission_and_payment_follow_their_valid_states()
    {
        var service = new InvoiceLifecycleChallenge();
        var emitted = service.Transition(new("draft", "emit", 100m, new(2026, 1, 31), new(2026, 1, 1)));
        Assert.Equal("issued", emitted.State);
        Assert.True(emitted.PaymentAllowed);
        var paid = service.Transition(new("issued", "pay", 0m, new(2026, 1, 31), new(2026, 1, 2)));
        Assert.Equal("paid", paid.State);
        Assert.False(paid.PaymentAllowed);
    }
    [Fact]
    public void Already_paid_invoice_cannot_be_emitted_again()
    {
        Assert.Throws<InvalidOperationException>(() => new InvoiceLifecycleChallenge().Transition(new("paid", "emit", 0m, new(2026, 1, 31), new(2026, 1, 2))));
    }
    [Fact]
    public void Due_date_boundary_distinguishes_issued_and_overdue()
    {
        var service = new InvoiceLifecycleChallenge();
        var onDueDate = service.Transition(new("issued", "mark-overdue", 100m, new(2026, 1, 31), new(2026, 1, 31)));
        Assert.Equal("issued", onDueDate.State);
        var afterDueDate = service.Transition(new("issued", "mark-overdue", 100m, new(2026, 1, 31), new(2026, 2, 1)));
        Assert.Equal("overdue", afterDueDate.State);
        Assert.True(afterDueDate.PaymentAllowed);
    }
}

