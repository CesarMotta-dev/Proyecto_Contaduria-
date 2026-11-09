using AccountingLab.Api.Transport;
using AccountingLab.Domain.Education;
using AccountingLab.Domain.Model;
using AccountingLab.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using D01 = AccountingLab.Domain.Challenges.DP01FactoryMethod;
using D02 = AccountingLab.Domain.Challenges.DP02AbstractFactory;
using D03 = AccountingLab.Application.Challenges.DP03Builder;
using D04 = AccountingLab.Domain.Challenges.DP04Prototype;
using D05 = AccountingLab.Domain.Challenges.DP05Singleton;
using D06 = AccountingLab.Infrastructure.Challenges.DP06Adapter;
using D07 = AccountingLab.Application.Challenges.DP07Bridge;
using D08 = AccountingLab.Domain.Challenges.DP08Composite;
using D09 = AccountingLab.Application.Challenges.DP09Decorator;
using D10 = AccountingLab.Application.Challenges.DP10Facade;
using D11 = AccountingLab.Domain.Challenges.DP11Flyweight;
using D12 = AccountingLab.Application.Challenges.DP12Proxy;
using D13 = AccountingLab.Domain.Challenges.DP13ChainOfResponsibility;
using D14 = AccountingLab.Domain.Challenges.DP14Command;
using D15 = AccountingLab.Application.Challenges.DP15Iterator;
using D16 = AccountingLab.Domain.Challenges.DP16Mediator;
using D17 = AccountingLab.Domain.Challenges.DP17Memento;
using D18 = AccountingLab.Domain.Challenges.DP18Observer;
using D19 = AccountingLab.Domain.Challenges.DP19State;
using D20 = AccountingLab.Domain.Challenges.DP20Strategy;
using D21 = AccountingLab.Application.Challenges.DP21TemplateMethod;
using D22 = AccountingLab.Domain.Challenges.DP22Visitor;

namespace AccountingLab.Api.Controllers;

public sealed record IdentifiedOperationInput(D14.AccountingOperationRequest Request, D14.AccountingOperationContext Context);

[ApiController, Route("api"), Authorize, ServiceFilter(typeof(TenantAccessFilter))]
[ProducesResponseType(typeof(ProblemDetails), 501)]
[ProducesResponseType(typeof(ProblemDetails), 400)]
[ProducesResponseType(typeof(ProblemDetails), 401)]
[ProducesResponseType(typeof(ProblemDetails), 403)]
public sealed class ChallengesController(TenantContext tenant, CatalogService service) : ControllerBase
{
    [HttpGet("challenges")]
    public IActionResult Catalog() => Ok(ChallengeCatalog.Items.Select(x => new { x.Id, x.Name, x.Family, x.Context, x.ReferenceUrl, Available = false, Verification = "NotExecuted" }));

    [HttpPost("vouchers/generate")]
    public IActionResult Voucher(D01.VoucherCreationRequest input) => Ok(new D01.VoucherCreationChallenge().Create(input with { CompanyId = tenant.CompanyId }));
    [HttpPost("companies/profile-services")]
    public IActionResult Profile(D02.AccountingProfileRequest input) => Ok(new D02.AccountingProfileChallenge().CreateFamily(input));
    [HttpPost("reports/build")]
    public IActionResult Report(D03.FinancialReportRequest input) => Ok(new D03.ReportAssemblyChallenge().Build(input));
    [HttpGet("reports/balance-sheet")]
    public IActionResult BalanceSheet(DateOnly? from, DateOnly? to) => Report(new("Balance general", from ?? new DateOnly(2026, 1, 1), to ?? new DateOnly(2026, 12, 31), ["Activo", "Pasivo", "Patrimonio"], []));
    [HttpPost("templates/copy")]
    public IActionResult CopyTemplate(D04.RecurringTemplate input) => Ok(new D04.TemplateCopyChallenge().Copy(input));
    [HttpPost("configuration/registry")]
    public IActionResult Registry() => Ok(new D05.GlobalRegistryChallenge().Acquire());
    [HttpPost("bank-statements/import")]
    public IActionResult Import(D06.BankImportRequest input) => Ok(new D06.BankImportChallenge().Import(input));
    [HttpPost("reports/render")]
    public IActionResult Render(D07.ReportPresentationRequest input) => Ok(new D07.FinancialRepresentationChallenge().Render(input));
    [HttpPost("accounts/tree-total")]
    public IActionResult Tree(D08.AccountHierarchy input) => Ok(new D08.AccountTreeChallenge().Total(input));
    [HttpPost("exports/enriched")]
    public IActionResult Enrich(D09.DocumentExportRequest input) => Ok(new D09.DocumentEnrichmentChallenge().Export(input));
    [HttpPost("accounting-periods/close")]
    public IActionResult Close(D10.ClosePeriodRequest input) => Ok(new D10.PeriodClosingChallenge().Close(input with { CompanyId = tenant.CompanyId }));
    [HttpPost("movements/classifications")]
    public IActionResult Classifications(IReadOnlyList<D11.MovementClassificationInput> input) => Ok(new D11.SharedClassificationChallenge().Prepare(input));
    [HttpPost("reports/confidential")]
    public async Task<IActionResult> Confidential(D12.SensitiveReportRequest input, CancellationToken ct = default)
    {
        if (tenant.Role != "Administrador") throw new AccessDeniedException("El informe confidencial requiere rol Administrador.");
        var summary = await service.SummaryAsync(tenant.CompanyId, ct);
        return Ok(new D12.SensitiveReportChallenge().Get(input with { CompanyId = tenant.CompanyId, MayRead = true },
            () => new D12.SensitiveFinancialReport(tenant.CompanyId, input.ReportKey, summary.Debits)));
    }
    [HttpPost("payments/approve")]
    public IActionResult Approve(D13.PaymentApprovalRequest input) => Ok(new D13.PaymentApprovalChallenge().Evaluate(input));
    [HttpPost("operations/execute")]
    public IActionResult ExecuteOperation(IdentifiedOperationInput input) => Ok(new D14.IdentifiedOperationChallenge().Execute(input.Request with { CompanyId = tenant.CompanyId }, input.Context));
    [HttpPost("ledger/traverse")]
    public IActionResult Traverse(IReadOnlyList<D15.LedgerPage> input) => Ok(new D15.LedgerTraversalChallenge().Open(input));
    [HttpPost("invoices/process")]
    public IActionResult Process(D16.InvoiceProcessingRequest input) => Ok(new D16.InvoiceCoordinationChallenge().Process(input));
    [HttpPost("budgets/snapshots")]
    public IActionResult Snapshot(D17.BudgetDraft input) => Ok(new D17.BudgetHistoryChallenge().Save(input));
    [HttpPost("notifications/publish")]
    public IActionResult Publish(D18.FinancialEvent input) => Ok(new D18.FinancialNotificationsChallenge().Publish(input));
    [HttpPost("invoices/transition")]
    public IActionResult Transition(D19.InvoiceLifecycleRequest input) => Ok(new D19.InvoiceLifecycleChallenge().Transition(input));
    [HttpPost("taxes/calculate")]
    public IActionResult Taxes(D20.TaxCalculationRequest input) => Ok(new D20.ConfigurableTaxChallenge().Calculate(input));
    [HttpPost("bank-reconciliation/reconcile")]
    public IActionResult Reconcile(D21.BankReconciliationRequest input) => Ok(new D21.BankReconciliationChallenge().Reconcile(input));
    [HttpPost("documents/analyze")]
    public IActionResult Analyze(D22.DocumentAnalysisRequest input) => Ok(new D22.DocumentAnalysisChallenge().Analyze(input));

    // Invocaciones educativas con entradas ficticias explícitas. Cada una alcanza el mismo contrato real de su módulo.
    // No son operaciones básicas ni resultados contables simulados; la capacidad sigue sin implementar.
    [HttpPost("challenges/{id}/execute")]
    public async Task<IActionResult> Demonstrate(string id)
    {
        var companyId = tenant.CompanyId;
        return id.ToUpperInvariant() switch
        {
            "DP-01" => Voucher(new(companyId, "Ingreso", 100m, "COP")),
            "DP-02" => Profile(new("Pyme", "Ficticio")),
            "DP-03" => Report(new("Informe educativo", new(2026, 1, 1), new(2026, 12, 31), ["Balance"], [])),
            "DP-04" => CopyTemplate(new D04.RecurringTemplate()),
            "DP-05" => Registry(),
            "DP-06" => Import(new("BancoA", "[]")),
            "DP-07" => Render(new("Balance", "CSV", 100m)),
            "DP-08" => Tree(new("1105", true, 100m, [])),
            "DP-09" => Enrich(new("Documento educativo", ["Audit"], new Dictionary<string, string>())),
            "DP-10" => Close(new(companyId, new(2026, 1, 1), new(2026, 12, 31), [], 0)),
            "DP-11" => Classifications([]),
            "DP-12" => await Confidential(new(companyId, true, "Balance")),
            "DP-13" => Approve(new(100m, 500m, true, 200m, ["Disponibilidad"])),
            "DP-14" => ExecuteOperation(new(new(companyId, Guid.NewGuid(), "ApplyReceipt", Guid.NewGuid(), 100m), new(200m, false, []))),
            "DP-15" => Traverse([]),
            "DP-16" => Process(new(Guid.NewGuid(), 100m, true, [])),
            "DP-17" => Snapshot(new D17.BudgetDraft { Id = Guid.NewGuid(), Name = "Borrador educativo" }),
            "DP-18" => Publish(new(Guid.NewGuid(), "PaymentRegistered", 100m)),
            "DP-19" => Transition(new("Draft", "Issue", 100m, new(2026, 12, 31), new(2026, 10, 23))),
            "DP-20" => Taxes(new(100m, "Fictional", new(2026, 10, 23), [])),
            "DP-21" => Reconcile(new("Exact", [], [], 0m)),
            "DP-22" => Analyze(new("Totals", [])),
            _ => throw new ResourceNotFoundException("No existe ese desafío académico.")
        };
    }
}
