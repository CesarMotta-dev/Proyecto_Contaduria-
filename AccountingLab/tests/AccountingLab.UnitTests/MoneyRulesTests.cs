using System.Globalization;
using AccountingLab.Domain.Model;
using Xunit;

namespace AccountingLab.UnitTests;

public sealed class MoneyRulesTests
{
    [Theory]
    [InlineData("0.0001")]
    [InlineData("1")]
    [InlineData("125.5000")]
    [InlineData("999999999999999.9999")]
    public void Accepts_positive_amounts_representable_in_decimal_19_4(string text)
    {
        var amount = decimal.Parse(text, CultureInfo.InvariantCulture);
        Assert.Null(Record.Exception(() => MoneyRules.Validate(amount)));
    }

    [Fact]
    public void Zero_is_allowed_only_when_contract_explicitly_allows_it()
    {
        Assert.Null(Record.Exception(() => MoneyRules.Validate(0m, allowZero: true)));
        Assert.Throws<BusinessRuleException>(() => MoneyRules.Validate(0m));
    }

    [Theory]
    [InlineData("-0.0001")]
    [InlineData("-1")]
    public void Negative_amount_is_rejected_even_if_zero_is_allowed(string text)
    {
        var amount = decimal.Parse(text, CultureInfo.InvariantCulture);
        Assert.Throws<BusinessRuleException>(() => MoneyRules.Validate(amount, allowZero: true));
    }

    [Fact]
    public void Rejects_first_amount_above_mysql_decimal_19_4_maximum()
    {
        Assert.Throws<BusinessRuleException>(() => MoneyRules.Validate(1000000000000000m));
    }

    [Theory]
    [InlineData("0.00001")]
    [InlineData("1.00001")]
    [InlineData("10.12345")]
    public void Extra_fractional_precision_is_rejected_instead_of_silently_rounded(string text)
    {
        var amount = decimal.Parse(text, CultureInfo.InvariantCulture);
        Assert.Throws<BusinessRuleException>(() => MoneyRules.Validate(amount));
    }
}

