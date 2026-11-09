using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AccountingLab.IntegrationTests;

[Collection(MySqlApiCollection.Name)]
public sealed class ConflictResponsesTests(MySqlApiFixture fixture)
{
    [Fact]
    public async Task Duplicate_company_tax_id_returns_conflict_without_creating_company_or_membership()
    {
        using var workspace = await fixture.CreateWorkspaceAsync();
        var original = await fixture.WithDatabaseAsync(db => db.Companies.AsNoTracking()
            .Where(company => company.Id == workspace.CompanyId)
            .Select(company => new { company.Id, company.Name, company.TaxId, company.Currency })
            .SingleAsync());
        var before = await fixture.WithDatabaseAsync(async db => new
        {
            Companies = await db.Companies.CountAsync(),
            Memberships = await db.Memberships.CountAsync()
        });

        using var response = await workspace.Client.PostAsJsonAsync("/api/companies", new
        {
            name = "Duplicado rechazado " + Guid.NewGuid().ToString("N"),
            taxId = original.TaxId,
            currency = "COP"
        });
        var problem = await MySqlApiFixture.ReadJsonAsync(response, HttpStatusCode.Conflict);
        var after = await fixture.WithDatabaseAsync(async db => new
        {
            Companies = await db.Companies.CountAsync(),
            Memberships = await db.Memberships.CountAsync()
        });
        var preserved = await fixture.WithDatabaseAsync(db => db.Companies.AsNoTracking()
            .Where(company => company.Id == workspace.CompanyId)
            .Select(company => new { company.Id, company.Name, company.TaxId, company.Currency })
            .SingleAsync());

        Assert.Equal(409, problem.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("correlationId").GetString()));
        Assert.Equal(before, after);
        Assert.Equal(original, preserved);
        Assert.Equal(1, await fixture.WithDatabaseAsync(db => db.Companies.CountAsync(company => company.TaxId == original.TaxId)));
    }

    [Fact]
    public async Task Conflicting_invoice_edit_returns_conflict_without_changing_drafts_or_adding_audit()
    {
        using var workspace = await fixture.CreateWorkspaceAsync();
        var partyId = await workspace.AddPartyAsync();
        var suffix = Guid.NewGuid().ToString("N");
        var firstNumber = "FAC-A-" + suffix;
        var secondNumber = "FAC-B-" + suffix;
        await workspace.PostAsync("/api/invoices", new
        {
            number = firstNumber, thirdPartyId = partyId, amount = "100.0000",
            issueDate = "2026-01-01", dueDate = "2026-01-31"
        });
        var second = await workspace.PostAsync("/api/invoices", new
        {
            number = secondNumber, thirdPartyId = partyId, amount = "200.0000",
            issueDate = "2026-02-01", dueDate = "2026-02-28"
        });
        var secondId = second.GetProperty("id").GetGuid();
        var beforeInvoices = await fixture.WithDatabaseAsync(db => db.Invoices.AsNoTracking()
            .Where(invoice => invoice.CompanyId == workspace.CompanyId).OrderBy(invoice => invoice.Id)
            .Select(invoice => new
            {
                invoice.Id, invoice.CompanyId, invoice.Number, invoice.Amount,
                invoice.ThirdPartyId, invoice.IssueDate, invoice.DueDate, invoice.Status
            }).ToArrayAsync());
        var beforeAudit = await fixture.WithDatabaseAsync(db => db.Audit.CountAsync(item => item.CompanyId == workspace.CompanyId));

        using var response = await workspace.Client.PutAsJsonAsync("/api/invoices/" + secondId, new
        {
            number = firstNumber, thirdPartyId = partyId, amount = "999.0000",
            issueDate = "2026-03-01", dueDate = "2026-03-31"
        }, MySqlApiFixture.Json);
        var problem = await MySqlApiFixture.ReadJsonAsync(response, HttpStatusCode.Conflict);
        var afterInvoices = await fixture.WithDatabaseAsync(db => db.Invoices.AsNoTracking()
            .Where(invoice => invoice.CompanyId == workspace.CompanyId).OrderBy(invoice => invoice.Id)
            .Select(invoice => new
            {
                invoice.Id, invoice.CompanyId, invoice.Number, invoice.Amount,
                invoice.ThirdPartyId, invoice.IssueDate, invoice.DueDate, invoice.Status
            }).ToArrayAsync());
        var afterAudit = await fixture.WithDatabaseAsync(db => db.Audit.CountAsync(item => item.CompanyId == workspace.CompanyId));

        Assert.Equal(409, problem.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("correlationId").GetString()));
        Assert.Equal(2, beforeInvoices.Length);
        Assert.Equal(beforeInvoices, afterInvoices);
        Assert.Equal(beforeAudit, afterAudit);
        Assert.DoesNotContain(afterInvoices, invoice => invoice.Amount == 999m);
    }
}

