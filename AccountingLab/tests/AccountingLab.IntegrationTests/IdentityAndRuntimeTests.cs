using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AccountingLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Xunit;

namespace AccountingLab.IntegrationTests;

[Collection(MySqlApiCollection.Name)]
public sealed class IdentityAndRuntimeTests(MySqlApiFixture fixture)
{
    [Fact]
    public async Task ProtectedRoute_WithoutIdentity_ReturnsStructured401()
    {
        // Arrange
        using var client = fixture.Factory.CreateClient();

        // Act
        using var response = await client.GetAsync("/api/accounts");
        var problem = await MySqlApiFixture.ReadJsonAsync(response, HttpStatusCode.Unauthorized);

        // Assert
        Assert.Equal(401, problem.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("correlationId").GetString()));
    }

    [Fact]
    public async Task Login_InvalidPassword_DoesNotIssueToken()
    {
        // Arrange
        using var client = fixture.Factory.CreateClient();

        // Act
        using var response = await client.PostAsJsonAsync("/api/auth/login", new { email = MySqlApiFixture.DemoEmail, password = "Wrong_Only_For_Test_123!" });
        var problem = await MySqlApiFixture.ReadJsonAsync(response, HttpStatusCode.Unauthorized);

        // Assert
        Assert.False(problem.TryGetProperty("accessToken", out _));
    }

    [Theory]
    [InlineData("Consulta", HttpStatusCode.Forbidden)]
    [InlineData("Contador", HttpStatusCode.OK)]
    public async Task MemberRole_ControlsBasicWritesAndUserManagement(string role, HttpStatusCode expectedWrite)
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();
        var email = Guid.NewGuid().ToString("N") + "@example.invalid";
        await workspace.PostAsync("/api/users", new { name = "Usuario de prueba", email, password = fixture.Password, role });
        using var member = await fixture.LoginAsync(email, fixture.Password);
        member.DefaultRequestHeaders.Add("X-Company-Id", workspace.CompanyId.ToString());

        // Act
        using var read = await member.GetAsync("/api/accounts");
        using var write = await member.PostAsJsonAsync("/api/accounts", new { code = Guid.NewGuid().ToString("N"), name = "Cuenta del miembro" });
        using var users = await member.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        Assert.Equal(expectedWrite, write.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, users.StatusCode);
    }

    [Fact]
    public async Task ForeignCompany_RejectsAccessAndPreventsDataDisclosure()
    {
        // Arrange
        using var own = await fixture.CreateWorkspaceAsync();
        using var foreign = await fixture.CreateWorkspaceAsync();
        var email = Guid.NewGuid().ToString("N") + "@example.invalid";
        await own.PostAsync("/api/users", new { name = "Usuario aislado", email, password = fixture.Password, role = "Contador" });
        using var member = await fixture.LoginAsync(email, fixture.Password);
        member.DefaultRequestHeaders.Add("X-Company-Id", foreign.CompanyId.ToString());

        // Act
        using var response = await member.GetAsync("/api/accounts");
        var problem = await MySqlApiFixture.ReadJsonAsync(response, HttpStatusCode.Forbidden);

        // Assert
        Assert.False(problem.TryGetProperty("items", out _));
        Assert.Equal(403, problem.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task MissingCompany_Produces400BeforeBusinessAccess()
    {
        // Arrange
        using var client = await fixture.LoginAsync();

        // Act
        using var response = await client.GetAsync("/api/accounts");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task HealthAndSwagger_MigratedDatabase_AreAvailableAndDocumentExactMoney()
    {
        // Arrange
        using var client = fixture.Factory.CreateClient();

        // Act
        using var live = await client.GetAsync("/health/live");
        using var ready = await client.GetAsync("/health/ready");
        using var swaggerResponse = await client.GetAsync("/swagger/v1/swagger.json");
        var swagger = await MySqlApiFixture.ReadJsonAsync(swaggerResponse, HttpStatusCode.OK);

        // Assert
        Assert.Equal(HttpStatusCode.OK, live.StatusCode);
        Assert.Equal(HttpStatusCode.OK, ready.StatusCode);
        var paths = swagger.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/api/journal-entries", out _));
        Assert.True(paths.TryGetProperty("/api/accounting-periods/close", out _));
        Assert.True(paths.TryGetProperty("/api/reports/balance-sheet", out _));
        var requestSchema = ResolveSchema(swagger, paths.GetProperty("/api/journal-entries").GetProperty("post").GetProperty("requestBody").GetProperty("content").GetProperty("application/json").GetProperty("schema"));
        var journalLineSchema = ResolveSchema(swagger, requestSchema.GetProperty("properties").GetProperty("lines").GetProperty("items"));
        Assert.True(journalLineSchema.GetProperty("properties").TryGetProperty("debit", out var debitSchema), journalLineSchema.GetRawText());
        debitSchema = ResolveSchema(swagger, debitSchema);
        Assert.True(debitSchema.TryGetProperty("type", out var debitType), debitSchema.GetRawText());
        Assert.Equal("string", debitType.GetString());
        Assert.True(swagger.GetProperty("components").GetProperty("securitySchemes").TryGetProperty("Bearer", out _));
    }

    private static JsonElement ResolveSchema(JsonElement swagger, JsonElement schema)
    {
        while (schema.TryGetProperty("$ref", out var reference))
            schema = swagger.GetProperty("components").GetProperty("schemas").GetProperty(reference.GetString()!.Split('/')[^1]);
        return schema;
    }

    [Fact]
    public async Task Readiness_UnknownDatabase_DoesNotAdvertiseHealthySchema()
    {
        // Arrange
        var connection = new MySqlConnectionStringBuilder(fixture.ConnectionString) { Database = "missing_test_" + Guid.NewGuid().ToString("N") };
        await using var unavailable = new TestApiFactory(connection.ConnectionString, fixture.Password);
        using var client = unavailable.CreateClient();

        // Act
        using var response = await client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task SeedAndMigrations_RepeatedInitialization_PreserveExistingRowsAndHistory()
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();
        var preserved = await workspace.PostAsync("/api/accounts", new { code = Guid.NewGuid().ToString("N"), name = "Trabajo que debe conservarse" });
        var before = await fixture.WithDatabaseAsync(async db => new[]
        {
            await db.Companies.CountAsync(), await db.Users.CountAsync(), await db.Memberships.CountAsync(),
            await db.Accounts.CountAsync(), await db.Periods.CountAsync(), await db.ThirdParties.CountAsync(),
            await db.CostCenters.CountAsync(), await db.Notices.CountAsync()
        });

        // Act
        await fixture.InitializeDatabaseAsync(fixture.Factory);
        await fixture.InitializeDatabaseAsync(fixture.Factory);
        var after = await fixture.WithDatabaseAsync(async db => new[]
        {
            await db.Companies.CountAsync(), await db.Users.CountAsync(), await db.Memberships.CountAsync(),
            await db.Accounts.CountAsync(), await db.Periods.CountAsync(), await db.ThirdParties.CountAsync(),
            await db.CostCenters.CountAsync(), await db.Notices.CountAsync()
        });
        var migrationState = await fixture.WithDatabaseAsync(async db => (Applied: (await db.Database.GetAppliedMigrationsAsync()).ToArray(), Pending: (await db.Database.GetPendingMigrationsAsync()).ToArray()));
        var listed = await workspace.GetAsync("/api/accounts");

        // Assert
        Assert.Equal(before, after);
        Assert.NotEmpty(migrationState.Applied);
        Assert.Empty(migrationState.Pending);
        Assert.Contains(listed.GetProperty("items").EnumerateArray(), x => x.GetProperty("id").GetGuid() == preserved.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task ApplicationRestart_UsesExistingMigrationsAndPreservesCompanyData()
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();
        var account = await workspace.PostAsync("/api/accounts", new { code = Guid.NewGuid().ToString("N"), name = "Cuenta persistente" });

        // Act
        await using var restarted = new TestApiFactory(fixture.ConnectionString, fixture.Password);
        await fixture.InitializeDatabaseAsync(restarted);
        using var client = await fixture.LoginAsync(factory: restarted);
        client.DefaultRequestHeaders.Add("X-Company-Id", workspace.CompanyId.ToString());
        using var response = await client.GetAsync("/api/accounts");
        var page = await MySqlApiFixture.ReadJsonAsync(response, HttpStatusCode.OK);

        // Assert
        Assert.Contains(page.GetProperty("items").EnumerateArray(), item => item.GetProperty("id").GetGuid() == account.GetProperty("id").GetGuid());
    }
}
