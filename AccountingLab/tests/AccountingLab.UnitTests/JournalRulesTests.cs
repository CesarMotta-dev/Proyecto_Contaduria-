using AccountingLab.Domain.Model;
using Xunit;

namespace AccountingLab.UnitTests;

public sealed class JournalRulesTests
{
    private static JournalEntry BalancedEntry() => new()
    {
        CompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        OperationId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        Reference = "MANUAL-DEMO-001",
        Date = new(2026, 1, 15),
        Lines = new()
        {
            new() { AccountId = Guid.Parse("44444444-4444-4444-4444-444444444444"), Debit = 25.1250m },
            new() { AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), Credit = 25.1250m }
        }
    };

    [Fact]
    public void Accepts_balanced_manual_entry_with_exact_decimal_amounts()
    {
        var entry = BalancedEntry();
        Assert.Null(Record.Exception(() => JournalRules.Validate(entry)));
        Assert.Equal(25.1250m, entry.Lines.Sum(x => x.Debit));
        Assert.Equal(entry.Lines.Sum(x => x.Debit), entry.Lines.Sum(x => x.Credit));
    }

    [Fact]
    public void Supports_multiple_debit_lines_balanced_by_one_credit()
    {
        var entry = BalancedEntry();
        entry.Lines[0].Debit = 20m;
        entry.Lines.Add(new() { AccountId = Guid.NewGuid(), Debit = 5.1250m });
        Assert.Null(Record.Exception(() => JournalRules.Validate(entry)));
    }

    [Fact]
    public void Rejects_smallest_supported_difference_between_debits_and_credits()
    {
        var entry = BalancedEntry();
        entry.Lines[1].Credit += 0.0001m;
        Assert.Throws<BusinessRuleException>(() => JournalRules.Validate(entry));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Entry_requires_at_least_two_lines(int count)
    {
        var entry = BalancedEntry();
        entry.Lines = entry.Lines.Take(count).ToList();
        Assert.Throws<BusinessRuleException>(() => JournalRules.Validate(entry));
    }

    [Fact]
    public void Every_posting_line_requires_an_account_identity()
    {
        var entry = BalancedEntry();
        entry.Lines[0].AccountId = Guid.Empty;
        Assert.Throws<BusinessRuleException>(() => JournalRules.Validate(entry));
    }

    [Fact]
    public void One_line_cannot_have_both_debit_and_credit()
    {
        var entry = BalancedEntry();
        entry.Lines[0].Credit = 1m;
        Assert.Throws<BusinessRuleException>(() => JournalRules.Validate(entry));
    }

    [Fact]
    public void Zero_value_line_cannot_make_an_empty_entry_appear_valid()
    {
        var entry = BalancedEntry();
        entry.Lines.ForEach(line => { line.Debit = 0m; line.Credit = 0m; });
        Assert.Throws<BusinessRuleException>(() => JournalRules.Validate(entry));
    }

    [Fact]
    public void Negative_posting_amount_is_rejected()
    {
        var entry = BalancedEntry();
        entry.Lines[0].Debit = -25.1250m;
        Assert.Throws<BusinessRuleException>(() => JournalRules.Validate(entry));
    }

    [Theory]
    [InlineData("operation")]
    [InlineData("reference")]
    [InlineData("date")]
    public void Entry_requires_operation_reference_and_accounting_date(string missing)
    {
        var entry = BalancedEntry();
        if (missing == "operation") entry.OperationId = Guid.Empty;
        if (missing == "reference") entry.Reference = " ";
        if (missing == "date") entry.Date = default;
        Assert.Throws<BusinessRuleException>(() => JournalRules.Validate(entry));
    }

    [Fact]
    public void Balanced_values_with_excess_precision_are_still_rejected()
    {
        var entry = BalancedEntry();
        entry.Lines[0].Debit = 25.12501m;
        entry.Lines[1].Credit = 25.12501m;
        Assert.Throws<BusinessRuleException>(() => JournalRules.Validate(entry));
    }
}

