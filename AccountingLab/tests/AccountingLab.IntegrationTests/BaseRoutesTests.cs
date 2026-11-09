using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AccountingLab.IntegrationTests;

[Collection(MySqlApiCollection.Name)]
public sealed class BaseRoutesTests(MySqlApiFixture fixture)
{
    [Theory]
    [InlineData("companies")]
    [InlineData("users")]
    [InlineData("accounts")]
    [InlineData("periods")]
    [InlineData("third-parties")]
    [InlineData("cost-centers")]
    [InlineData("tax-rules")]
    [InlineData("journal-entries")]
    [InlineData("ledger")]
    [InlineData("receivables")]
    [InlineData("payables")]
    [InlineData("payments")]
    [InlineData("invoices")]
    [InlineData("bank-statements")]
    [InlineData("bank-reconciliation")]
    [InlineData("budgets")]
    [InlineData("approvals")]
    [InlineData("notifications")]
    [InlineData("audit")]
    public async Task ModuleList_AuthorizedCompany_ReturnsRealPagedContract(string resource)
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();

        // Act
        var page = await workspace.GetAsync("/api/" + resource);

        // Assert
        Assert.Equal(JsonValueKind.Array, page.GetProperty("items").ValueKind);
        Assert.True(page.GetProperty("total").GetInt32() >= 0);
        Assert.Equal(1, page.GetProperty("page").GetInt32());
    }

    [Theory]
    [InlineData("accounts")]
    [InlineData("periods")]
    [InlineData("third-parties")]
    [InlineData("cost-centers")]
    [InlineData("tax-rules")]
    [InlineData("receivables")]
    [InlineData("payables")]
    [InlineData("payments")]
    [InlineData("invoices")]
    [InlineData("bank-statements")]
    [InlineData("bank-reconciliation")]
    [InlineData("budgets")]
    [InlineData("approvals")]
    public async Task ModuleCreation_ValidInput_IsPersistedAndListedWithinCompany(string resource)
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();
        var party = await workspace.AddPartyAsync();
        var unique = Guid.NewGuid().ToString("N");
        object payload = resource switch
        {
            "accounts" => new { code = unique, name = "Cuenta de integración", isGroup = false },
            "periods" => new { name = "Periodo", startDate = "2026-01-01", endDate = "2026-12-31" },
            "third-parties" => new { name = "Tercero de integración", document = unique, email = "party@example.invalid" },
            "cost-centers" => new { code = unique, name = "Centro de integración" },
            "tax-rules" => new { name = "Regla ficticia", rate = "0.123456", validFrom = "2026-01-01", validTo = "2026-12-31" },
            "receivables" or "payables" => new { thirdPartyId = party, description = unique, amount = "12.3456", dueDate = "2026-12-31" },
            "payments" => new { thirdPartyId = party, reference = unique, amount = "12.3456", direction = "Receipt" },
            "invoices" => new { thirdPartyId = party, number = unique, amount = "12.3456", issueDate = "2026-10-23", dueDate = "2026-12-31" },
            "bank-statements" => new { reference = unique, amount = "12.3456", bookingDate = "2026-10-23", description = "Extracto interno" },
            "bank-reconciliation" => new { name = unique, statementDate = "2026-10-23" },
            "budgets" => new { name = unique, year = 2026, amount = "12.3456" },
            "approvals" => new { reference = unique, description = "Solicitud borrador", amount = "12.3456" },
            _ => throw new InvalidOperationException("Caso de prueba no declarado.")
        };

        // Act
        var created = await workspace.PostAsync("/api/" + resource, payload);
        var listed = await workspace.GetAsync("/api/" + resource + "?pageSize=100");

        // Assert
        var id = created.GetProperty("id").GetGuid();
        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal(workspace.CompanyId, created.GetProperty("companyId").GetGuid());
        Assert.Contains(listed.GetProperty("items").EnumerateArray(), item => item.GetProperty("id").GetGuid() == id);
        if (created.TryGetProperty("amount", out var amount))
        {
            Assert.Equal(JsonValueKind.String, amount.ValueKind);
            Assert.Equal("12.3456", amount.GetString());
        }
        if (resource == "tax-rules") Assert.Equal("0.123456", created.GetProperty("rate").GetString());
        if (resource is "payments" or "invoices" or "bank-reconciliation" or "budgets" or "approvals")
            Assert.Equal("Draft", created.GetProperty("status").GetString());
    }

    [Theory]
    [InlineData("invoices")]
    [InlineData("budgets")]
    public async Task DraftEdit_ValidChanges_PersistsWithoutExecutingAdvancedLifecycle(string resource)
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();
        var party = await workspace.AddPartyAsync();
        object initial = resource == "invoices"
            ? new { thirdPartyId = party, number = Guid.NewGuid().ToString("N"), amount = "10.0000", issueDate = "2026-10-23", dueDate = "2026-12-31" }
            : new { name = "Presupuesto original", year = 2026, amount = "10.0000" };
        var created = await workspace.PostAsync("/api/" + resource, initial);
        object updated = resource == "invoices"
            ? new { thirdPartyId = party, number = created.GetProperty("number").GetString(), amount = "22.2222", issueDate = "2026-10-23", dueDate = "2026-12-31", status = "Draft" }
            : new { name = "Presupuesto actualizado", year = 2026, amount = "22.2222", status = "Draft" };

        // Act
        using var response = await workspace.Client.PutAsJsonAsync($"/api/{resource}/{created.GetProperty("id").GetGuid()}", updated);
        var edited = await MySqlApiFixture.ReadJsonAsync(response, HttpStatusCode.OK);
        var list = await workspace.GetAsync("/api/" + resource);

        // Assert
        Assert.Equal("Draft", edited.GetProperty("status").GetString());
        Assert.Equal("22.2222", edited.GetProperty("amount").GetString());
        Assert.Contains(list.GetProperty("items").EnumerateArray(), item => item.GetProperty("id").GetGuid() == edited.GetProperty("id").GetGuid() && item.GetProperty("amount").GetString() == "22.2222");
    }

    [Fact]
    public async Task CompanyCreation_AddsMembershipWithoutAssumingSeedOrder()
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();

        // Act
        var found = false;
        var pageNumber = 1;
        JsonElement listed;
        do
        {
            listed = await workspace.GetAsync($"/api/companies?pageSize=100&page={pageNumber}");
            found |= listed.GetProperty("items").EnumerateArray().Any(item => item.GetProperty("id").GetGuid() == workspace.CompanyId && item.GetProperty("role").GetString() == "Administrador");
            pageNumber++;
        } while (!found && (pageNumber - 1) * 100 < listed.GetProperty("total").GetInt32());

        // Assert
        Assert.True(found, "La membresía de la empresa recién creada debe estar presente en sus páginas autorizadas.");
    }

    [Fact]
    public async Task SummaryAndCsv_AfterManualPosting_ReflectPersistedValues()
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();
        var accounts = await workspace.AddJournalDataAsync();
        var reference = "CSV-" + Guid.NewGuid().ToString("N");
        await workspace.PostAsync("/api/journal-entries", TestWorkspace.Journal(Guid.NewGuid(), accounts.Debit, accounts.Credit, reference));

        // Act
        var summary = await workspace.GetAsync("/api/reports/summary");
        using var entriesCsv = await workspace.Client.GetAsync("/api/exports/journal-entries");
        using var accountsCsv = await workspace.Client.GetAsync("/api/exports/accounts");

        // Assert
        Assert.Equal("123.4567", summary.GetProperty("debits").GetString());
        Assert.Equal("123.4567", summary.GetProperty("credits").GetString());
        Assert.Equal(1, summary.GetProperty("entryCount").GetInt32());
        Assert.Equal(HttpStatusCode.OK, entriesCsv.StatusCode);
        Assert.Equal("text/csv", entriesCsv.Content.Headers.ContentType!.MediaType);
        Assert.Contains(reference, await entriesCsv.Content.ReadAsStringAsync());
        Assert.Contains("123.4567", await entriesCsv.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, accountsCsv.StatusCode);
        Assert.Contains("Caja", await accountsCsv.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task SeededNotifications_AreExplicitlyDemonstrationData()
    {
        // Arrange
        using var client = await fixture.LoginAsync();
        client.DefaultRequestHeaders.Add("X-Company-Id", AccountingLab.Infrastructure.Persistence.DemoSeed.CompanyOne.ToString());

        // Act
        using var response = await client.GetAsync("/api/notifications");
        var page = await MySqlApiFixture.ReadJsonAsync(response, HttpStatusCode.OK);

        // Assert
        Assert.NotEmpty(page.GetProperty("items").EnumerateArray());
        Assert.All(page.GetProperty("items").EnumerateArray(), notice => Assert.True(notice.GetProperty("isDemo").GetBoolean()));
    }
}
