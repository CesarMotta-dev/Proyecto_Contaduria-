using Xunit;
using AccountingLab.Domain.Challenges.DP01FactoryMethod;

namespace AccountingLab.PatternChallenges.DP01FactoryMethod;

[Trait("ChallengeId", "DP-01")]
public sealed class DP01FactoryMethodTests
{
    [Fact]
    public void Creates_requested_voucher_preserving_accounting_values()
    {
        var company = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var result = new VoucherCreationChallenge().Create(new(company, "ingreso", 125.3750m, "COP"));
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(company, result.CompanyId);
        Assert.Equal("ingreso", result.Kind);
        Assert.Equal(125.3750m, result.Amount);
        Assert.Equal("COP", result.Currency);
    }
    [Fact]
    public void Rejects_unsupported_voucher_without_creating_document()
    {
        Assert.Throws<ArgumentException>(() => new VoucherCreationChallenge().Create(new(Guid.NewGuid(), "desconocido", 10m, "COP")));
    }
    [Fact]
    public void Supports_each_required_voucher_kind_at_zero_amount()
    {
        foreach (var kind in new[] { "ingreso", "egreso", "ajuste", "traslado" })
        {
            var result = new VoucherCreationChallenge().Create(new(Guid.NewGuid(), kind, 0m, "COP"));
            Assert.Equal(kind, result.Kind);
            Assert.Equal(0m, result.Amount);
        }
    }
}

