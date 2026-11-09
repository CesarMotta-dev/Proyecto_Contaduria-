using System.Globalization;
using AccountingLab.Domain.Model;
using Xunit;

namespace AccountingLab.UnitTests;

public sealed class BasicDataRulesTests
{
    [Fact]
    public void Required_text_accepts_documented_maximum_length()
    {
        Assert.Null(Record.Exception(() => BasicDataRules.Required(new string('x', 200), "Nombre")));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Required_text_rejects_missing_business_value(string value)
    {
        Assert.Throws<BusinessRuleException>(() => BasicDataRules.Required(value, "Nombre"));
    }

    [Fact]
    public void Required_text_rejects_value_longer_than_contract_limit()
    {
        Assert.Throws<BusinessRuleException>(() => BasicDataRules.Required(new string('x', 201), "Nombre"));
    }

    [Fact]
    public void Account_cannot_be_its_own_parent()
    {
        var account = new Account { Code = "1105", Name = "Caja demo" };
        account.ParentId = account.Id;
        Assert.Throws<BusinessRuleException>(() => BasicDataRules.Validate(account));
    }

    [Fact]
    public void Basic_account_can_reference_a_distinct_parent_without_solving_tree_operations()
    {
        var account = new Account { Code = "1105", Name = "Caja demo", ParentId = Guid.NewGuid() };
        Assert.Null(Record.Exception(() => BasicDataRules.Validate(account)));
    }

    [Fact]
    public void One_day_accounting_period_is_valid_when_open()
    {
        var period = new AccountingPeriod { Name = "Día demo", StartDate = new(2026, 1, 1), EndDate = new(2026, 1, 1) };
        Assert.Null(Record.Exception(() => BasicDataRules.Validate(period)));
    }

    [Theory]
    [InlineData("reversed")]
    [InlineData("missing-start")]
    [InlineData("closed")]
    public void Accounting_period_requires_valid_dates_and_initial_open_status(string invalid)
    {
        var period = new AccountingPeriod { Name = "Periodo demo", StartDate = new(2026, 1, 1), EndDate = new(2026, 1, 31) };
        if (invalid == "reversed") period.EndDate = new(2025, 12, 31);
        if (invalid == "missing-start") period.StartDate = default;
        if (invalid == "closed") period.Status = "Closed";
        Assert.Throws<BusinessRuleException>(() => BasicDataRules.Validate(period));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("0.123456")]
    public void Fictional_tax_rate_accepts_configured_six_decimal_range(string text)
    {
        var rule = new TaxRule { Name = "DEMO", Rate = decimal.Parse(text, CultureInfo.InvariantCulture), ValidFrom = new(2026, 1, 1), ValidTo = new(2026, 12, 31) };
        Assert.Null(Record.Exception(() => BasicDataRules.Validate(rule)));
    }

    [Theory]
    [InlineData("-0.000001")]
    [InlineData("1.000001")]
    [InlineData("0.1234567")]
    public void Fictional_tax_rate_rejects_range_or_precision_violations(string text)
    {
        var rule = new TaxRule { Name = "DEMO", Rate = decimal.Parse(text, CultureInfo.InvariantCulture), ValidFrom = new(2026, 1, 1) };
        Assert.Throws<BusinessRuleException>(() => BasicDataRules.Validate(rule));
    }

    [Fact]
    public void Fictional_tax_rule_can_have_open_ended_validity()
    {
        var rule = new TaxRule { Name = "DEMO", Rate = 0.125m, ValidFrom = new(2026, 1, 1), ValidTo = null };
        Assert.Null(Record.Exception(() => BasicDataRules.Validate(rule)));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Fictional_tax_rule_rejects_missing_or_inverted_validity(bool missingStart)
    {
        var rule = new TaxRule { Name = "DEMO", Rate = 0.125m, ValidFrom = new(2026, 1, 1), ValidTo = new(2025, 12, 31) };
        if (missingStart) { rule.ValidFrom = default; rule.ValidTo = null; }
        Assert.Throws<BusinessRuleException>(() => BasicDataRules.Validate(rule));
    }

    private static InvoiceDraft ValidInvoice() => new()
    {
        Number = "FAC-DEMO-001", ThirdPartyId = Guid.NewGuid(), Amount = 100m,
        IssueDate = new(2026, 1, 1), DueDate = new(2026, 1, 31)
    };

    [Fact]
    public void Basic_invoice_registration_accepts_only_a_valid_draft()
    {
        var invoice = ValidInvoice();
        Assert.Null(Record.Exception(() => BasicDataRules.Validate(invoice)));
        Assert.Equal("Draft", invoice.Status);
    }

    [Theory]
    [InlineData("Issued")]
    [InlineData("Paid")]
    [InlineData("Cancelled")]
    public void Basic_invoice_registration_does_not_perform_evaluated_lifecycle_transitions(string status)
    {
        var invoice = ValidInvoice();
        invoice.Status = status;
        Assert.Throws<BusinessRuleException>(() => BasicDataRules.Validate(invoice));
    }

    [Theory]
    [InlineData("third-party")]
    [InlineData("issue-date")]
    [InlineData("due-date")]
    public void Invoice_draft_requires_third_party_and_consistent_dates(string invalid)
    {
        var invoice = ValidInvoice();
        if (invalid == "third-party") invoice.ThirdPartyId = Guid.Empty;
        if (invalid == "issue-date") invoice.IssueDate = default;
        if (invalid == "due-date") invoice.DueDate = invoice.IssueDate.AddDays(-1);
        Assert.Throws<BusinessRuleException>(() => BasicDataRules.Validate(invoice));
    }

    [Theory]
    [InlineData("Payment")]
    [InlineData("Receipt")]
    public void Basic_payment_or_receipt_is_stored_as_draft(string direction)
    {
        var payment = new PaymentDraft { Reference = "P-DEMO", ThirdPartyId = Guid.NewGuid(), Amount = 50m, Direction = direction };
        Assert.Null(Record.Exception(() => BasicDataRules.Validate(payment)));
        Assert.Equal("Draft", payment.Status);
    }

    [Theory]
    [InlineData("direction")]
    [InlineData("status")]
    [InlineData("third-party")]
    public void Payment_draft_rejects_invalid_identity_direction_or_applied_status(string invalid)
    {
        var payment = new PaymentDraft { Reference = "P-DEMO", ThirdPartyId = Guid.NewGuid(), Amount = 50m };
        if (invalid == "direction") payment.Direction = "Unknown";
        if (invalid == "status") payment.Status = "Applied";
        if (invalid == "third-party") payment.ThirdPartyId = Guid.Empty;
        Assert.Throws<BusinessRuleException>(() => BasicDataRules.Validate(payment));
    }

    [Theory]
    [InlineData(2000)]
    [InlineData(2200)]
    public void Budget_draft_accepts_year_boundaries_and_zero_amount(int year)
    {
        var budget = new BudgetDraft { Name = "Presupuesto demo", Year = year, Amount = 0m };
        Assert.Null(Record.Exception(() => BasicDataRules.Validate(budget)));
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2201)]
    public void Budget_draft_rejects_year_outside_supported_range(int year)
    {
        var budget = new BudgetDraft { Name = "Presupuesto demo", Year = year, Amount = 0m };
        Assert.Throws<BusinessRuleException>(() => BasicDataRules.Validate(budget));
    }

    [Fact]
    public void Basic_budget_endpoint_cannot_register_an_approved_budget()
    {
        var budget = new BudgetDraft { Name = "Presupuesto demo", Year = 2026, Amount = 10m, Status = "Approved" };
        Assert.Throws<BusinessRuleException>(() => BasicDataRules.Validate(budget));
    }

    [Theory]
    [InlineData("Receivable")]
    [InlineData("Payable")]
    public void Basic_obligation_registration_accepts_supported_kinds_as_open(string kind)
    {
        var obligation = new Obligation { Description = "Obligación demo", ThirdPartyId = Guid.NewGuid(), Amount = 10m, DueDate = new(2026, 1, 31), Kind = kind };
        Assert.Null(Record.Exception(() => BasicDataRules.Validate(obligation)));
    }

    [Fact]
    public void Basic_obligation_registration_cannot_apply_payment_effects()
    {
        var obligation = new Obligation { Description = "Obligación demo", ThirdPartyId = Guid.NewGuid(), Amount = 10m, DueDate = new(2026, 1, 31), Status = "Settled" };
        Assert.Throws<BusinessRuleException>(() => BasicDataRules.Validate(obligation));
    }

    [Fact]
    public void Reconciliation_registration_cannot_claim_completed_reconciliation()
    {
        var session = new ReconciliationDraft { Name = "Sesión demo", StatementDate = new(2026, 1, 31), Status = "Completed" };
        Assert.Throws<BusinessRuleException>(() => BasicDataRules.Validate(session));
    }

    [Fact]
    public void Approval_request_registration_cannot_claim_authorized_payment()
    {
        var approval = new ApprovalDraft { Reference = "AP-DEMO", Amount = 10m, Status = "Approved" };
        Assert.Throws<BusinessRuleException>(() => BasicDataRules.Validate(approval));
    }
}

