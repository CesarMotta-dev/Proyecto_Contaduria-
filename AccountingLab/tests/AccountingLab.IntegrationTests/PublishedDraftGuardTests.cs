using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AccountingLab.IntegrationTests;

[Collection(MySqlApiCollection.Name)]
public sealed class PublishedDraftGuardTests(MySqlApiFixture fixture)
{
    [Fact]
    public async Task Issued_invoice_cannot_be_overwritten_by_submitting_a_draft()
    {
        using var workspace = await fixture.CreateWorkspaceAsync();
        var partyId = await workspace.AddPartyAsync();
        var invoice = await workspace.PostAsync("/api/invoices", new
        {
            number = "FAC-GUARD-" + Guid.NewGuid().ToString("N"),
            thirdPartyId = partyId, amount = "100.0000",
            issueDate = "2026-01-01", dueDate = "2026-01-31"
        });
        var id = invoice.GetProperty("id").GetGuid();
        // Dato de fixture: no se ejecuta ni se proporciona la transición académica DP-19.
        await fixture.WithDatabaseAsync(async db =>
        {
            var stored = await db.Invoices.SingleAsync(item => item.Id == id && item.CompanyId == workspace.CompanyId);
            stored.Status = "Issued";
            return await db.SaveChangesAsync();
        });
        var before = await fixture.WithDatabaseAsync(db => db.Invoices.AsNoTracking()
            .Where(item => item.Id == id && item.CompanyId == workspace.CompanyId)
            .Select(item => new
            {
                item.Id, item.CompanyId, item.Number, item.ThirdPartyId,
                item.Amount, item.IssueDate, item.DueDate, item.Status
            }).SingleAsync());
        var auditBefore = await fixture.WithDatabaseAsync(db => db.Audit.CountAsync(item => item.CompanyId == workspace.CompanyId));

        using var response = await workspace.Client.PutAsJsonAsync("/api/invoices/" + id, new
        {
            number = before.Number, thirdPartyId = partyId, amount = "999.0000",
            issueDate = "2026-02-01", dueDate = "2026-02-28", status = "Draft"
        }, MySqlApiFixture.Json);
        var problem = await MySqlApiFixture.ReadJsonAsync(response, HttpStatusCode.Conflict);
        var after = await fixture.WithDatabaseAsync(db => db.Invoices.AsNoTracking()
            .Where(item => item.Id == id && item.CompanyId == workspace.CompanyId)
            .Select(item => new
            {
                item.Id, item.CompanyId, item.Number, item.ThirdPartyId,
                item.Amount, item.IssueDate, item.DueDate, item.Status
            }).SingleAsync());
        var auditAfter = await fixture.WithDatabaseAsync(db => db.Audit.CountAsync(item => item.CompanyId == workspace.CompanyId));

        Assert.Equal("Issued", before.Status);
        Assert.Equal(409, problem.GetProperty("status").GetInt32());
        Assert.Equal(before, after);
        Assert.Equal(auditBefore, auditAfter);
    }

    [Fact]
    public async Task Approved_budget_cannot_be_overwritten_by_submitting_a_draft()
    {
        using var workspace = await fixture.CreateWorkspaceAsync();
        var budget = await workspace.PostAsync("/api/budgets", new
        {
            name = "Presupuesto protegido " + Guid.NewGuid().ToString("N"), year = 2026, amount = "200.0000"
        });
        var id = budget.GetProperty("id").GetGuid();
        // Dato de fixture: no se implementa aprobación, restauración ni historial del desafío DP-17.
        await fixture.WithDatabaseAsync(async db =>
        {
            var stored = await db.Budgets.SingleAsync(item => item.Id == id && item.CompanyId == workspace.CompanyId);
            stored.Status = "Approved";
            return await db.SaveChangesAsync();
        });
        var before = await fixture.WithDatabaseAsync(db => db.Budgets.AsNoTracking()
            .Where(item => item.Id == id && item.CompanyId == workspace.CompanyId)
            .Select(item => new { item.Id, item.CompanyId, item.Name, item.Year, item.Amount, item.Status })
            .SingleAsync());
        var auditBefore = await fixture.WithDatabaseAsync(db => db.Audit.CountAsync(item => item.CompanyId == workspace.CompanyId));

        using var response = await workspace.Client.PutAsJsonAsync("/api/budgets/" + id, new
        {
            name = "Cambio que debe rechazarse", year = 2027, amount = "999.0000", status = "Draft"
        }, MySqlApiFixture.Json);
        var problem = await MySqlApiFixture.ReadJsonAsync(response, HttpStatusCode.Conflict);
        var after = await fixture.WithDatabaseAsync(db => db.Budgets.AsNoTracking()
            .Where(item => item.Id == id && item.CompanyId == workspace.CompanyId)
            .Select(item => new { item.Id, item.CompanyId, item.Name, item.Year, item.Amount, item.Status })
            .SingleAsync());
        var auditAfter = await fixture.WithDatabaseAsync(db => db.Audit.CountAsync(item => item.CompanyId == workspace.CompanyId));

        Assert.Equal("Approved", before.Status);
        Assert.Equal(409, problem.GetProperty("status").GetInt32());
        Assert.Equal(before, after);
        Assert.Equal(auditBefore, auditAfter);
    }
}

