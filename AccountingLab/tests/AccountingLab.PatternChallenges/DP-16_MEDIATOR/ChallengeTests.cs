using Xunit;
using AccountingLab.Domain.Challenges.DP16Mediator;

namespace AccountingLab.PatternChallenges.DP16Mediator;

[Trait("ChallengeId", "DP-16")]
public sealed class DP16MediatorTests
{
    [Fact]
    public void Coordinates_required_components_once_for_valid_invoice()
    {
        var invoice = Guid.NewGuid();
        var result = new InvoiceCoordinationChallenge().Process(new(invoice, 100m, true, new[] { "validacion", "contabilizacion", "publicacion" }));
        Assert.True(result.Published);
        Assert.Equal(invoice, result.InvoiceId);
        Assert.Equal(new[] { "validacion", "contabilizacion", "publicacion" }, result.ParticipantsInvoked);
        Assert.Equal(3, result.ParticipantsInvoked.Distinct().Count());
    }
    [Fact]
    public void Invalid_invoice_is_not_accounted_or_published()
    {
        var result = new InvoiceCoordinationChallenge().Process(new(Guid.NewGuid(), 100m, false, new[] { "validacion", "contabilizacion", "publicacion" }));
        Assert.False(result.Published);
        Assert.Equal(new[] { "validacion" }, result.ParticipantsInvoked);
    }
    [Fact]
    public void Duplicate_participant_registration_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => new InvoiceCoordinationChallenge().Process(new(Guid.NewGuid(), 0m, true, new[] { "validacion", "validacion", "publicacion" })));
    }
}

