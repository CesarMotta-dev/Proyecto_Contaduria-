using System.Globalization;
using System.Text;
using AccountingLab.Api.Transport;
using AccountingLab.Application.Contracts;
using AccountingLab.Application.Services;
using AccountingLab.Domain.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountingLab.Api.Controllers;

[ApiController, Route("api"), Authorize, ServiceFilter(typeof(TenantAccessFilter))]
public sealed class WorkspaceController(CatalogService service, TenantContext tenant) : ControllerBase
{
    private Task<PagedResult<T>> List<T>(int page, int pageSize, string? search, CancellationToken ct, string? kind = null, DateOnly? from = null, DateOnly? to = null) where T : class, ITenantEntity =>
        service.ListAsync<T>(tenant.CompanyId, new(page, pageSize, search, kind, from, to), ct);
    private Task<T> Add<T>(T value, CancellationToken ct) where T : class, ITenantEntity => service.AddAsync(tenant.CompanyId, tenant.UserId, value, ct);

    [HttpGet("accounts")]
    public Task<PagedResult<Account>> Accounts(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default) => List<Account>(page, pageSize, search, ct);
    [HttpPost("accounts")] public Task<Account> CreateAccount(Account input, CancellationToken ct) => Add(input, ct);
    [HttpGet("periods")]
    public Task<PagedResult<AccountingPeriod>> Periods(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default) => List<AccountingPeriod>(page, pageSize, search, ct);
    [HttpPost("periods")] public Task<AccountingPeriod> CreatePeriod(AccountingPeriod input, CancellationToken ct) => Add(input, ct);
    [HttpGet("third-parties")]
    public Task<PagedResult<ThirdParty>> Parties(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default) => List<ThirdParty>(page, pageSize, search, ct);
    [HttpPost("third-parties")] public Task<ThirdParty> CreateParty(ThirdParty input, CancellationToken ct) => Add(input, ct);
    [HttpGet("cost-centers")]
    public Task<PagedResult<CostCenter>> CostCenters(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default) => List<CostCenter>(page, pageSize, search, ct);
    [HttpPost("cost-centers")] public Task<CostCenter> CreateCostCenter(CostCenter input, CancellationToken ct) => Add(input, ct);
    [HttpGet("tax-rules")]
    public Task<PagedResult<TaxRule>> Taxes(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default) => List<TaxRule>(page, pageSize, search, ct);
    [HttpPost("tax-rules")] public Task<TaxRule> CreateTax(TaxRule input, CancellationToken ct) => Add(input, ct);
    [HttpGet("receivables")]
    public Task<PagedResult<Obligation>> Receivables(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default) => List<Obligation>(page, pageSize, search, ct, "Receivable");
    [HttpPost("receivables")]
    public Task<Obligation> CreateReceivable(Obligation input, CancellationToken ct) { input.Kind = "Receivable"; return Add(input, ct); }
    [HttpGet("payables")]
    public Task<PagedResult<Obligation>> Payables(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default) => List<Obligation>(page, pageSize, search, ct, "Payable");
    [HttpPost("payables")]
    public Task<Obligation> CreatePayable(Obligation input, CancellationToken ct) { input.Kind = "Payable"; return Add(input, ct); }
    [HttpGet("payments")]
    public Task<PagedResult<PaymentDraft>> Payments(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default) => List<PaymentDraft>(page, pageSize, search, ct);
    [HttpPost("payments")] public Task<PaymentDraft> CreatePayment(PaymentDraft input, CancellationToken ct) => Add(input, ct);
    [HttpGet("invoices")]
    public Task<PagedResult<InvoiceDraft>> Invoices(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default) => List<InvoiceDraft>(page, pageSize, search, ct);
    [HttpPost("invoices")] public Task<InvoiceDraft> CreateInvoice(InvoiceDraft input, CancellationToken ct) => Add(input, ct);
    [HttpPut("invoices/{id:guid}")]
    public Task<InvoiceDraft> EditInvoice(Guid id, InvoiceDraft input, CancellationToken ct) => service.UpdateAsync(tenant.CompanyId, tenant.UserId, id, input, ct);
    [HttpGet("bank-statements")]
    public Task<PagedResult<BankStatement>> Statements(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default) => List<BankStatement>(page, pageSize, search, ct);
    [HttpPost("bank-statements")] public Task<BankStatement> CreateStatement(BankStatement input, CancellationToken ct) => Add(input, ct);
    [HttpGet("bank-reconciliation")]
    public Task<PagedResult<ReconciliationDraft>> Reconciliations(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default) => List<ReconciliationDraft>(page, pageSize, search, ct);
    [HttpPost("bank-reconciliation")] public Task<ReconciliationDraft> CreateReconciliation(ReconciliationDraft input, CancellationToken ct) => Add(input, ct);
    [HttpGet("budgets")]
    public Task<PagedResult<BudgetDraft>> Budgets(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default) => List<BudgetDraft>(page, pageSize, search, ct);
    [HttpPost("budgets")] public Task<BudgetDraft> CreateBudget(BudgetDraft input, CancellationToken ct) => Add(input, ct);
    [HttpPut("budgets/{id:guid}")]
    public Task<BudgetDraft> EditBudget(Guid id, BudgetDraft input, CancellationToken ct) => service.UpdateAsync(tenant.CompanyId, tenant.UserId, id, input, ct);
    [HttpGet("approvals")]
    public Task<PagedResult<ApprovalDraft>> Approvals(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default) => List<ApprovalDraft>(page, pageSize, search, ct);
    [HttpPost("approvals")] public Task<ApprovalDraft> CreateApproval(ApprovalDraft input, CancellationToken ct) => Add(input, ct);
    [HttpGet("notifications")]
    public Task<PagedResult<Notice>> Notifications(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default) => List<Notice>(page, pageSize, search, ct);
    [HttpGet("audit")]
    public Task<PagedResult<AuditEvent>> Audit(int page = 1, int pageSize = 20, string? search = null, CancellationToken ct = default) => List<AuditEvent>(page, pageSize, search, ct);
    [HttpGet("journal-entries")]
    public Task<PagedResult<JournalEntry>> Entries(int page = 1, int pageSize = 20, string? search = null, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default) => List<JournalEntry>(page, pageSize, search, ct, from: from, to: to);
    [HttpGet("journal-entries/{id:guid}")]
    public Task<JournalEntry> Entry(Guid id, CancellationToken ct) => service.GetAsync<JournalEntry>(tenant.CompanyId, id, ct);
    [HttpPost("journal-entries")]
    public async Task<IActionResult> Post(JournalInput input, CancellationToken ct)
    {
        var result = await service.PostJournalAsync(tenant.CompanyId, tenant.UserId, input, ct);
        if (result.Replayed) Response.Headers["Idempotent-Replay"] = "true";
        return StatusCode(result.Replayed ? 200 : 201, result.Entry);
    }
    [HttpGet("ledger")]
    public Task<PagedResult<LedgerRow>> Ledger(Guid? accountId, DateOnly? from, DateOnly? to, int page = 1, int pageSize = 20, CancellationToken ct = default) => service.LedgerAsync(tenant.CompanyId, accountId, new(page, pageSize, From: from, To: to), ct);
    [HttpGet("reports/summary")]
    public Task<Summary> Summary(CancellationToken ct) => service.SummaryAsync(tenant.CompanyId, ct);
    [HttpGet("exports/accounts")]
    public async Task<IActionResult> ExportAccounts(CancellationToken ct)
    {
        var all = new StringBuilder("code,name\r\n"); var page = 1; PagedResult<Account> batch;
        do { batch = await List<Account>(page++, 100, null, ct); foreach (var x in batch.Items) all.Append(Csv(x.Code)).Append(',').Append(Csv(x.Name)).Append("\r\n"); } while ((page - 1) * 100 < batch.Total);
        return File(Encoding.UTF8.GetBytes(all.ToString()), "text/csv; charset=utf-8", "cuentas.csv");
    }
    [HttpGet("exports/journal-entries")]
    public async Task<IActionResult> ExportEntries(CancellationToken ct)
    {
        var all = new StringBuilder("date,reference,accountId,debit,credit\r\n"); var page = 1; PagedResult<JournalEntry> batch;
        do { batch = await List<JournalEntry>(page++, 100, null, ct); foreach (var entry in batch.Items) foreach (var line in entry.Lines)
            all.Append(entry.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append(',').Append(Csv(entry.Reference)).Append(',').Append(line.AccountId).Append(',').Append(line.Debit.ToString(CultureInfo.InvariantCulture)).Append(',').Append(line.Credit.ToString(CultureInfo.InvariantCulture)).Append("\r\n"); } while ((page - 1) * 100 < batch.Total);
        return File(Encoding.UTF8.GetBytes(all.ToString()), "text/csv; charset=utf-8", "diario.csv");
    }
    private static string Csv(string value)
    {
        var safe = value.Length > 0 && "=+-@\t\r".Contains(value[0]) ? "'" + value : value;
        return "\"" + safe.Replace("\"", "\"\"") + "\"";
    }
}
