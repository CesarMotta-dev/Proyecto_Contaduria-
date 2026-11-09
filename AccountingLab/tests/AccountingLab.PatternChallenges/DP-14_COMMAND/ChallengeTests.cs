using Xunit;
using AccountingLab.Domain.Challenges.DP14Command;

namespace AccountingLab.PatternChallenges.DP14Command;

[Trait("ChallengeId", "DP-14")]
public sealed class DP14CommandTests
{
    [Fact]
    public void Application_and_reversal_keep_explicit_links_to_their_original_targets()
    {
        var company = Guid.NewGuid();
        var target = Guid.NewGuid();
        var operation = Guid.NewGuid();
        var service = new IdentifiedOperationChallenge();
        var payment = service.Execute(new(company, operation, "apply-payment", target, 25m), new(100m, true, Array.Empty<AccountingOperationReceipt>()));
        Assert.Equal(target, payment.TargetId);
        Assert.Equal(operation, payment.OperationId);
        Assert.Equal(25m, payment.AppliedAmount);
        Assert.Equal(75m, payment.RemainingAmount);
        Assert.NotEqual(Guid.Empty, payment.LinkedJournalEntryId);
        var reversal = service.Execute(new(company, Guid.NewGuid(), "reverse-voucher", target, 100m), new(0m, true, Array.Empty<AccountingOperationReceipt>()));
        Assert.Equal(target, reversal.TargetId);
        Assert.Equal("reverse-voucher", reversal.Kind);
        Assert.NotEqual(Guid.Empty, reversal.LinkedJournalEntryId);
    }
    [Fact]
    public void Repeated_operation_reuses_previous_effect_instead_of_creating_another_entry()
    {
        var target = Guid.NewGuid();
        var operation = Guid.NewGuid();
        var previous = new AccountingOperationReceipt(operation, target, "apply-payment", 25m, 75m, Guid.NewGuid(), false);
        var result = new IdentifiedOperationChallenge().Execute(new(Guid.NewGuid(), operation, "apply-payment", target, 25m), new(75m, true, new[] { previous }));
        Assert.True(result.Replayed);
        Assert.Equal(previous.LinkedJournalEntryId, result.LinkedJournalEntryId);
        Assert.Equal(75m, result.RemainingAmount);
    }
    [Fact]
    public void Draft_voucher_cannot_be_reversed_as_a_published_accounting_fact()
    {
        Assert.Throws<InvalidOperationException>(() => new IdentifiedOperationChallenge().Execute(
            new(Guid.NewGuid(), Guid.NewGuid(), "reverse-voucher", Guid.NewGuid(), 100m),
            new(0m, false, Array.Empty<AccountingOperationReceipt>())));
    }
}

