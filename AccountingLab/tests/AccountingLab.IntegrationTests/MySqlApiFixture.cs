using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AccountingLab.Api.Transport;
using AccountingLab.Domain.Model;
using AccountingLab.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using Xunit;

namespace AccountingLab.IntegrationTests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class MySqlApiCollection : ICollectionFixture<MySqlApiFixture>
{
    public const string Name = "AccountingLab MySQL API";
}

public sealed class MySqlApiFixture : IAsyncLifetime
{
    public const string DemoEmail = "docente@accountinglab.local";
    public string Password { get; } = Environment.GetEnvironmentVariable("AccountingLabTest__DemoPassword") ?? "Integration_Demo_123!";
    public string ConnectionString { get; private set; } = "";
    public TestApiFactory Factory { get; private set; } = null!;
    public static readonly JsonSerializerOptions Json = CreateJsonOptions();

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new ExactDecimalConverter());
        return options;
    }

    public async Task InitializeAsync()
    {
        ConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__AccountingLabTest")
            ?? throw new InvalidOperationException("Falta ConnectionStrings__AccountingLabTest. Estas pruebas requieren MySQL real en una base desechable separada; no se omiten.");
        var parsed = new MySqlConnectionStringBuilder(ConnectionString);
        if (string.IsNullOrWhiteSpace(parsed.Database) || !parsed.Database.Contains("test", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("La base de integración debe tener un nombre explícito de pruebas que contenga 'test'; no se utilizará la base demo.");
        Factory = new TestApiFactory(ConnectionString, Password);
        await InitializeDatabaseAsync(Factory);
    }

    public async Task InitializeDatabaseAsync(TestApiFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AccountingDbContext>();
        await db.Database.MigrateAsync();
        await DemoSeed.ApplyAsync(db, DemoEmail, Password);
    }

    public Task DisposeAsync() => Factory is null ? Task.CompletedTask : Factory.DisposeAsync().AsTask();

    public async Task<T> WithDatabaseAsync<T>(Func<AccountingDbContext, Task<T>> action)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<AccountingDbContext>());
    }

    public async Task<HttpClient> LoginAsync(string? email = null, string? password = null, TestApiFactory? factory = null)
    {
        var client = (factory ?? Factory).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var response = await client.PostAsJsonAsync("/api/auth/login", new { email = email ?? DemoEmail, password = password ?? Password });
        var content = await ReadJsonAsync(response, HttpStatusCode.OK);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content.GetProperty("accessToken").GetString());
        return client;
    }

    public async Task<TestWorkspace> CreateWorkspaceAsync()
    {
        var client = await LoginAsync();
        var suffix = Guid.NewGuid().ToString("N");
        using var response = await client.PostAsJsonAsync("/api/companies", new { name = "Integration " + suffix, taxId = "IT-" + suffix, currency = "COP" });
        var company = await ReadJsonAsync(response, HttpStatusCode.OK);
        var companyId = company.GetProperty("id").GetGuid();
        client.DefaultRequestHeaders.Add("X-Company-Id", companyId.ToString());
        return new TestWorkspace(companyId, client);
    }

    public static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        var text = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == expected, $"Se esperaba {(int)expected}, se obtuvo {(int)response.StatusCode}: {text}");
        using var document = JsonDocument.Parse(text);
        return document.RootElement.Clone();
    }
}

public sealed class TestApiFactory(string connectionString, string demoPassword) : WebApplicationFactory<global::Program>
{
    public JournalSaveFailure Failure { get; } = new();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:AccountingLab"] = connectionString,
            ["Authentication:SigningKey"] = "Integration_Only_Signing_Key_More_Than_32_Bytes_123!",
            ["Authentication:Issuer"] = "AccountingLab",
            ["Authentication:Audience"] = "AccountingLab.Web",
            ["Demo:Email"] = MySqlApiFixture.DemoEmail,
            ["Demo:Password"] = demoPassword,
            ["Logging:LogLevel:Default"] = "Warning"
        }));
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AccountingDbContext>>();
            services.RemoveAll<AccountingDbContext>();
            services.AddDbContext<AccountingDbContext>(options =>
                options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 0))).AddInterceptors(Failure));
            services.AddControllers().AddApplicationPart(typeof(TestPendingController).Assembly);
        });
    }
}

public sealed class JournalSaveFailure : SaveChangesInterceptor
{
    public Guid? OperationToFail { get; set; }

    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (OperationToFail is Guid operation && eventData.Context!.ChangeTracker.Entries<JournalEntry>().Any(x => x.Entity.OperationId == operation))
            throw new InvalidOperationException("Fallo de almacenamiento inyectado por la prueba después del guardado, antes de confirmar la transacción.");
        return ValueTask.FromResult(result);
    }
}

public sealed class TestWorkspace(Guid companyId, HttpClient client) : IDisposable
{
    public Guid CompanyId { get; } = companyId;
    public HttpClient Client { get; } = client;

    public async Task<JsonElement> PostAsync(string route, object payload)
    {
        using var response = await Client.PostAsJsonAsync(route, payload, MySqlApiFixture.Json);
        var text = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"POST {route} devolvió {(int)response.StatusCode}: {text}");
        using var document = JsonDocument.Parse(text);
        return document.RootElement.Clone();
    }

    public async Task<JsonElement> GetAsync(string route)
    {
        using var response = await Client.GetAsync(route);
        return await MySqlApiFixture.ReadJsonAsync(response, HttpStatusCode.OK);
    }

    public async Task<Guid> AddPartyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N");
        return (await PostAsync("/api/third-parties", new { name = "Tercero " + suffix, document = suffix, email = "party@example.invalid" })).GetProperty("id").GetGuid();
    }

    public async Task<(Guid Debit, Guid Credit)> AddJournalDataAsync()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var debit = await PostAsync("/api/accounts", new { code = "DEBIT-" + suffix, name = "Caja" });
        var credit = await PostAsync("/api/accounts", new { code = "CREDIT-" + suffix, name = "Capital" });
        await PostAsync("/api/periods", new { name = "Periodo de prueba", startDate = "2026-01-01", endDate = "2026-12-31" });
        return (debit.GetProperty("id").GetGuid(), credit.GetProperty("id").GetGuid());
    }

    public static object Journal(Guid operation, Guid debit, Guid credit, string reference, string amount = "123.4567") => new
    {
        operationId = operation,
        reference,
        date = "2026-10-23",
        lines = new[]
        {
            new { accountId = debit, debit = amount, credit = "0.0000", costCenterId = (Guid?)null },
            new { accountId = credit, debit = "0.0000", credit = amount, costCenterId = (Guid?)null }
        }
    };

    public static StringContent RawJson(string text) => new(text, Encoding.UTF8, "application/json");
    public void Dispose() => Client.Dispose();
}
