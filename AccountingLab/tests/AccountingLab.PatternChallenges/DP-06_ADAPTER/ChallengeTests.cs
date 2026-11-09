using Xunit;
using AccountingLab.Infrastructure.Challenges.DP06Adapter;

namespace AccountingLab.PatternChallenges.DP06Adapter;

[Trait("ChallengeId", "DP-06")]
public sealed class DP06AdapterTests
{
    [Fact]
    public void Imports_first_fictional_bank_preserving_its_decimal_values()
    {
        const string json = "[{\"id\":\"A-1\",\"date\":\"2026-01-12\",\"description\":\"Cobro demo\",\"debit\":\"0.0000\",\"credit\":\"25.1250\",\"currency\":\"COP\"}]";
        var item = Assert.Single(new BankImportChallenge().Import(new("BancoA", json)));
        Assert.Equal("A-1", item.ExternalId);
        Assert.Equal(new DateOnly(2026, 1, 12), item.Date);
        Assert.Equal(0m, item.Debit);
        Assert.Equal(25.1250m, item.Credit);
    }
    [Fact]
    public void Rejects_malformed_provider_payload()
    {
        Assert.Throws<FormatException>(() => new BankImportChallenge().Import(new("BancoA", "no es JSON")));
    }
    [Fact]
    public void Imports_second_bank_contract_with_signed_and_zero_amounts()
    {
        const string json = "[{\"reference\":\"B-1\",\"bookedOn\":\"2026-01-12\",\"concept\":\"Pago demo\",\"signedAmount\":\"-10.1250\",\"isoCurrency\":\"COP\"},{\"reference\":\"B-2\",\"bookedOn\":\"2026-01-13\",\"concept\":\"Sin importe\",\"signedAmount\":\"0.0000\",\"isoCurrency\":\"COP\"}]";
        var result = new BankImportChallenge().Import(new("BancoB", json));
        Assert.Equal(2, result.Count);
        Assert.Equal(10.1250m, result[0].Debit);
        Assert.Equal(0m, result[0].Credit);
        Assert.Equal(0m, result[1].Debit);
        Assert.Equal(0m, result[1].Credit);
    }
}

