using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using AccountingLab.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AccountingLab.IntegrationTests;

[Collection(MySqlApiCollection.Name)]
public sealed class FinancialGuaranteesTests(MySqlApiFixture fixture)
{
    [Fact]
    public async Task ManualJournal_ExactFourDecimalAmounts_PersistsAndAppearsInBooks()
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();
        var accounts = await workspace.AddJournalDataAsync();
        var operation = Guid.NewGuid();
        var reference = "EXACT-" + Guid.NewGuid().ToString("N");

        // Act
        using var response = await workspace.Client.PostAsJsonAsync("/api/journal-entries", TestWorkspace.Journal(operation, accounts.Debit, accounts.Credit, reference));
        var posted = await MySqlApiFixture.ReadJsonAsync(response, HttpStatusCode.Created);
        var id = posted.GetProperty("id").GetGuid();
        var journal = await workspace.GetAsync($"/api/journal-entries/{id}");
        var ledger = await workspace.GetAsync($"/api/ledger?accountId={accounts.Debit}&from=2026-10-01&to=2026-10-31");
        var stored = await fixture.WithDatabaseAsync(db => db.JournalEntries.AsNoTracking().Include(x => x.Lines).SingleAsync(x => x.CompanyId == workspace.CompanyId && x.OperationId == operation));

        // Assert
        Assert.Equal(reference, journal.GetProperty("reference").GetString());
        Assert.Equal("Posted", journal.GetProperty("status").GetString());
        Assert.Equal(123.4567m, stored.Lines.Sum(x => x.Debit));
        Assert.Equal(123.4567m, stored.Lines.Sum(x => x.Credit));
        Assert.All(journal.GetProperty("lines").EnumerateArray(), line => Assert.Equal(JsonValueKind.String, line.GetProperty("debit").ValueKind));
        var row = Assert.Single(ledger.GetProperty("items").EnumerateArray());
        Assert.Equal("123.4567", row.GetProperty("debit").GetString());
        Assert.Equal("123.4567", row.GetProperty("balance").GetString());
        var audits = await fixture.WithDatabaseAsync(db => db.Audit.AsNoTracking().Where(x => x.CompanyId == workspace.CompanyId && x.ResourceId == id).ToArrayAsync());
        Assert.Single(audits, audit => audit.Action == "ManualPosted");
    }

    [Theory]
    [InlineData("Unbalanced")]
    [InlineData("OneLine")]
    [InlineData("BothSides")]
    [InlineData("Negative")]
    [InlineData("ExcessPrecision")]
    [InlineData("ExcessScaleZero")]
    [InlineData("NumericTransport")]
    [InlineData("OutsidePeriod")]
    [InlineData("ForeignAccount")]
    [InlineData("UnknownCostCenter")]
    public async Task ManualJournal_InvalidContract_RejectsWithoutFinancialOrAuditEffects(string invalidCase)
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();
        var accounts = await workspace.AddJournalDataAsync();
        var operation = Guid.NewGuid();
        var node = JsonNode.Parse(JsonSerializer.Serialize(TestWorkspace.Journal(operation, accounts.Debit, accounts.Credit, "INVALID-" + operation)))!;
        var lines = node["lines"]!.AsArray();
        switch (invalidCase)
        {
            case "Unbalanced": lines[1]!["credit"] = "1.0000"; break;
            case "OneLine": lines.RemoveAt(1); break;
            case "BothSides": lines[0]!["credit"] = "1.0000"; break;
            case "Negative": lines[0]!["debit"] = "-123.4567"; break;
            case "ExcessPrecision": lines[0]!["debit"] = "123.45678"; lines[1]!["credit"] = "123.45678"; break;
            case "ExcessScaleZero": lines[0]!["debit"] = "123.45670"; lines[1]!["credit"] = "123.45670"; break;
            case "NumericTransport": lines[0]!["debit"] = 123.4567m; break;
            case "OutsidePeriod": node["date"] = "2027-01-01"; break;
            case "ForeignAccount":
                using (var foreign = await fixture.CreateWorkspaceAsync())
                {
                    var foreignAccount = await foreign.PostAsync("/api/accounts", new { code = "FOREIGN", name = "Cuenta ajena" });
                    lines[0]!["accountId"] = foreignAccount.GetProperty("id").GetGuid().ToString();
                }
                break;
            case "UnknownCostCenter": lines[0]!["costCenterId"] = Guid.NewGuid().ToString(); break;
            default: throw new InvalidOperationException("Caso inválido no declarado.");
        }
        var auditBefore = await fixture.WithDatabaseAsync(db => db.Audit.CountAsync(x => x.CompanyId == workspace.CompanyId));

        // Act
        using var response = await workspace.Client.PostAsync("/api/journal-entries", TestWorkspace.RawJson(node.ToJsonString()));
        var problem = await MySqlApiFixture.ReadJsonAsync(response, HttpStatusCode.BadRequest);

        // Assert
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("correlationId").GetString()));
        Assert.Equal(0, await fixture.WithDatabaseAsync(db => db.JournalEntries.CountAsync(x => x.CompanyId == workspace.CompanyId)));
        Assert.Equal(auditBefore, await fixture.WithDatabaseAsync(db => db.Audit.CountAsync(x => x.CompanyId == workspace.CompanyId)));
        var summary = await workspace.GetAsync("/api/reports/summary");
        Assert.Equal("0", summary.GetProperty("debits").GetString());
        Assert.Equal("0", summary.GetProperty("credits").GetString());
    }

    [Fact]
    public async Task FailureAfterSaving_BeforeTransactionCommit_RollsBackJournalLinesAndAudit()
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();
        var accounts = await workspace.AddJournalDataAsync();
        var operation = Guid.NewGuid();
        var auditBefore = await fixture.WithDatabaseAsync(db => db.Audit.CountAsync(x => x.CompanyId == workspace.CompanyId));
        fixture.Factory.Failure.OperationToFail = operation;
        HttpResponseMessage response;

        // Act
        try { response = await workspace.Client.PostAsJsonAsync("/api/journal-entries", TestWorkspace.Journal(operation, accounts.Debit, accounts.Credit, "ROLLBACK-" + operation)); }
        finally { fixture.Factory.Failure.OperationToFail = null; }

        // Assert
        using (response)
        {
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(0, await fixture.WithDatabaseAsync(db => db.JournalEntries.CountAsync(x => x.CompanyId == workspace.CompanyId)));
            Assert.Equal(auditBefore, await fixture.WithDatabaseAsync(db => db.Audit.CountAsync(x => x.CompanyId == workspace.CompanyId)));
            Assert.Equal(0, await fixture.WithDatabaseAsync(db => (from line in db.JournalLines join entry in db.JournalEntries on line.EntryId equals entry.Id where entry.CompanyId == workspace.CompanyId select line).CountAsync()));
        }
        using var retry = await workspace.Client.PostAsJsonAsync("/api/journal-entries", TestWorkspace.Journal(operation, accounts.Debit, accounts.Credit, "ROLLBACK-" + operation));
        Assert.Equal(HttpStatusCode.Created, retry.StatusCode);
        Assert.Equal(1, await fixture.WithDatabaseAsync(db => db.JournalEntries.CountAsync(x => x.CompanyId == workspace.CompanyId && x.OperationId == operation)));
    }

    [Fact]
    public async Task RepeatedOperation_SameContentReplays_DifferentContentConflicts()
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();
        var accounts = await workspace.AddJournalDataAsync();
        var operation = Guid.NewGuid();
        var request = TestWorkspace.Journal(operation, accounts.Debit, accounts.Credit, "REPLAY-" + operation);

        // Act
        using var first = await workspace.Client.PostAsJsonAsync("/api/journal-entries", request);
        using var repeated = await workspace.Client.PostAsJsonAsync("/api/journal-entries", request);
        using var conflict = await workspace.Client.PostAsJsonAsync("/api/journal-entries", TestWorkspace.Journal(operation, accounts.Debit, accounts.Credit, "REPLAY-" + operation, "50.0000"));
        var firstBody = await MySqlApiFixture.ReadJsonAsync(first, HttpStatusCode.Created);
        var repeatedBody = await MySqlApiFixture.ReadJsonAsync(repeated, HttpStatusCode.OK);

        // Assert
        Assert.Equal(firstBody.GetProperty("id").GetGuid(), repeatedBody.GetProperty("id").GetGuid());
        Assert.Equal("true", Assert.Single(repeated.Headers.GetValues("Idempotent-Replay")));
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.Equal(1, await fixture.WithDatabaseAsync(db => db.JournalEntries.CountAsync(x => x.CompanyId == workspace.CompanyId && x.OperationId == operation)));
        Assert.Equal(1, await fixture.WithDatabaseAsync(db => db.Audit.CountAsync(x => x.CompanyId == workspace.CompanyId && x.ResourceId == firstBody.GetProperty("id").GetGuid() && x.Action == "ManualPosted")));
    }

    [Fact]
    public async Task ConcurrentIdenticalRequests_CommitExactlyOneBusinessOperation()
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();
        var accounts = await workspace.AddJournalDataAsync();
        var operation = Guid.NewGuid();
        var request = TestWorkspace.Journal(operation, accounts.Debit, accounts.Credit, "CONCURRENT-" + operation);

        // Act
        var responses = await Task.WhenAll(workspace.Client.PostAsJsonAsync("/api/journal-entries", request), workspace.Client.PostAsJsonAsync("/api/journal-entries", request));

        // Assert
        try
        {
            Assert.Equal(new[] { HttpStatusCode.OK, HttpStatusCode.Created }, responses.Select(x => x.StatusCode).OrderBy(x => x));
            var bodies = await Task.WhenAll(responses.Select(x => x.Content.ReadFromJsonAsync<JsonElement>()));
            Assert.Equal(bodies[0].GetProperty("id").GetGuid(), bodies[1].GetProperty("id").GetGuid());
            Assert.Equal(1, await fixture.WithDatabaseAsync(db => db.JournalEntries.CountAsync(x => x.CompanyId == workspace.CompanyId && x.OperationId == operation)));
            var summary = await workspace.GetAsync("/api/reports/summary");
            Assert.Equal("123.4567", summary.GetProperty("debits").GetString());
        }
        finally { foreach (var response in responses) response.Dispose(); }
    }

    [Theory]
    [InlineData("Entry")]
    [InlineData("Line")]
    [InlineData("Audit")]
    public async Task PostedFacts_RejectMutationAndRetainOriginalValues(string fact)
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();
        var accounts = await workspace.AddJournalDataAsync();
        var reference = "IMMUTABLE-" + Guid.NewGuid().ToString("N");
        var posted = await workspace.PostAsync("/api/journal-entries", TestWorkspace.Journal(Guid.NewGuid(), accounts.Debit, accounts.Credit, reference));
        var id = posted.GetProperty("id").GetGuid();

        // Act
        await fixture.WithDatabaseAsync(async db =>
        {
            if (fact == "Entry") (await db.JournalEntries.SingleAsync(x => x.Id == id)).Reference = "Alterado";
            else if (fact == "Line") (await db.JournalLines.SingleAsync(x => x.EntryId == id && x.AccountId == accounts.Debit)).Debit = 999m;
            else (await db.Audit.SingleAsync(x => x.ResourceId == id && x.Action == "ManualPosted")).Action = "Alterado";
            await Assert.ThrowsAsync<BusinessRuleException>(() => db.SaveChangesAsync());
            return true;
        });

        // Assert
        var unchanged = await workspace.GetAsync($"/api/journal-entries/{id}");
        Assert.Equal(reference, unchanged.GetProperty("reference").GetString());
        Assert.Equal(123.4567m, await fixture.WithDatabaseAsync(db => db.JournalLines.Where(x => x.EntryId == id).SumAsync(x => x.Debit)));
        Assert.Equal(1, await fixture.WithDatabaseAsync(db => db.Audit.CountAsync(x => x.ResourceId == id && x.Action == "ManualPosted")));
    }

    [Fact]
    public async Task MissingJournal_Returns404WithCorrelation()
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();

        // Act
        using var response = await workspace.Client.GetAsync("/api/journal-entries/" + Guid.NewGuid());
        var problem = await MySqlApiFixture.ReadJsonAsync(response, HttpStatusCode.NotFound);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("correlationId").GetString()));
    }
}
