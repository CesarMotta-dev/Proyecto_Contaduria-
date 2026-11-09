namespace AccountingLab.Domain.Model;

public interface ITenantEntity
{
    Guid Id { get; set; }
    Guid CompanyId { get; set; }
}

public sealed class Company
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string TaxId { get; set; } = "";
    public string Currency { get; set; } = "COP";
}
public sealed class LabUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
}
public sealed class Membership
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = "Consulta";
}
public sealed class Account : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsGroup { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsActive { get; set; } = true;
}
public sealed class AccountingPeriod : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = "";
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = "Open";
}
public sealed class ThirdParty : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = "";
    public string Document { get; set; } = "";
    public string Email { get; set; } = "";
}
public sealed class CostCenter : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}
public sealed class TaxRule : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = "";
    public decimal Rate { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
}
public sealed class Obligation : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid ThirdPartyId { get; set; }
    public string Kind { get; set; } = "Receivable";
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = "Open";
}
public sealed class PaymentDraft : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid ThirdPartyId { get; set; }
    public string Reference { get; set; } = "";
    public string Direction { get; set; } = "Payment";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Draft";
}
public sealed class InvoiceDraft : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid ThirdPartyId { get; set; }
    public string Number { get; set; } = "";
    public decimal Amount { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = "Draft";
}
public sealed class BankStatement : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public string Reference { get; set; } = "";
    public DateOnly BookingDate { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
}
public sealed class ReconciliationDraft : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = "";
    public DateOnly StatementDate { get; set; }
    public string Status { get; set; } = "Draft";
}
public sealed class BudgetDraft : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = "";
    public int Year { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Draft";
}
public sealed class ApprovalDraft : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public string Reference { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Draft";
}
public sealed class Notice : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public bool IsDemo { get; set; }
}
public sealed class AuditEvent : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    public Guid ResourceId { get; set; }
    public string Action { get; set; } = "";
    public string Resource { get; set; } = "";
    public DateTime OccurredAt { get; set; }
}
public sealed class JournalEntry : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid OperationId { get; set; }
    public string RequestHash { get; set; } = "";
    public string Reference { get; set; } = "";
    public DateOnly Date { get; set; }
    public string Status { get; set; } = "Posted";
    public List<JournalLine> Lines { get; set; } = [];
}
public sealed class JournalLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EntryId { get; set; }
    public Guid AccountId { get; set; }
    public Guid? CostCenterId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}
