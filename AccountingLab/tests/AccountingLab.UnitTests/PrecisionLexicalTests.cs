using System.Globalization;
using AccountingLab.Domain.Model;
using Xunit;

namespace AccountingLab.UnitTests;

public sealed class PrecisionLexicalTests
{
    [Theory]
    [InlineData("100.00000")]
    [InlineData("0.00000")]
    [InlineData("999999999999999.99990")]
    public void Money_rejects_fifth_decimal_place_even_when_final_digit_is_zero(string text)
    {
        var amount = decimal.Parse(text, CultureInfo.InvariantCulture);
        Assert.Throws<BusinessRuleException>(() => MoneyRules.Validate(amount, allowZero: true));
    }

    [Theory]
    [InlineData("100.0000")]
    [InlineData("0.0000")]
    [InlineData("999999999999999.9999")]
    public void Money_accepts_trailing_zeroes_within_four_place_limit(string text)
    {
        var amount = decimal.Parse(text, CultureInfo.InvariantCulture);
        Assert.Null(Record.Exception(() => MoneyRules.Validate(amount, allowZero: true)));
    }

    [Theory]
    [InlineData("0.1000000")]
    [InlineData("0.0000000")]
    [InlineData("1.0000000")]
    public void Configured_rate_rejects_seventh_place_even_when_final_digit_is_zero(string text)
    {
        var rule = new TaxRule
        {
            Name = "DEMO-PRECISION", Rate = decimal.Parse(text, CultureInfo.InvariantCulture),
            ValidFrom = new(2026, 1, 1), ValidTo = new(2026, 12, 31)
        };
        Assert.Throws<BusinessRuleException>(() => BasicDataRules.Validate(rule));
    }

    [Theory]
    [InlineData("0.100000")]
    [InlineData("0.000000")]
    [InlineData("1.000000")]
    public void Configured_rate_accepts_trailing_zeroes_within_six_place_limit(string text)
    {
        var rule = new TaxRule
        {
            Name = "DEMO-PRECISION", Rate = decimal.Parse(text, CultureInfo.InvariantCulture),
            ValidFrom = new(2026, 1, 1), ValidTo = new(2026, 12, 31)
        };
        Assert.Null(Record.Exception(() => BasicDataRules.Validate(rule)));
    }

    [Fact]
    public void Balanced_manual_postings_do_not_bypass_money_scale_validation()
    {
        var entry = new JournalEntry
        {
            OperationId = Guid.NewGuid(), Reference = "MANUAL-PRECISION-DEMO", Date = new(2026, 1, 1),
            Lines = new()
            {
                new() { AccountId = Guid.NewGuid(), Debit = 100.00000m },
                new() { AccountId = Guid.NewGuid(), Credit = 100.00000m }
            }
        };
        Assert.Throws<BusinessRuleException>(() => JournalRules.Validate(entry));
    }

    [Fact]
    public void Basic_invoice_draft_does_not_silently_drop_excess_zeroes()
    {
        var draft = new InvoiceDraft
        {
            Number = "FAC-PRECISION-DEMO", ThirdPartyId = Guid.NewGuid(), Amount = 100.00000m,
            IssueDate = new(2026, 1, 1), DueDate = new(2026, 1, 31)
        };
        Assert.Throws<BusinessRuleException>(() => BasicDataRules.Validate(draft));
    }
}

