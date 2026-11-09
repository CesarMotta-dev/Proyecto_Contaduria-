using System.Net;
using System.Net.Http.Json;
using AccountingLab.Api.Transport;
using AccountingLab.Domain.Education;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AccountingLab.IntegrationTests;

public sealed record TestPendingPayload(string Label);

// Solo se incorpora al host de pruebas. Comprueba el transporte sin sustituir ni resolver un desafío.
[ApiController, Authorize, ServiceFilter(typeof(TenantAccessFilter))]
[Route("api/__tests")]
public sealed class TestPendingController : ControllerBase
{
    [HttpPost("pending/{challengeId}")]
    public IActionResult Pending(string challengeId, TestPendingPayload input) => throw new PatternPendingException(challengeId);

    [HttpPost("unexpected")]
    public IActionResult Unexpected(TestPendingPayload input) => throw new InvalidOperationException("Fallo inesperado exclusivo del host de pruebas.");
}

[Collection(MySqlApiCollection.Name)]
public sealed class PendingTransportTests(MySqlApiFixture fixture)
{
    [Theory]
    [InlineData("DP-01", "Factory Method", "factory-method")]
    [InlineData("DP-10", "Facade", "facade")]
    [InlineData("DP-22", "Visitor", "visitor")]
    public async Task ExplicitPendingException_Returns501MetadataWithoutBusinessOrSuccessAudit(string id, string name, string slug)
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();
        var auditsBefore = await fixture.WithDatabaseAsync(db => db.Audit.CountAsync(x => x.CompanyId == workspace.CompanyId));

        // Act
        using var response = await workspace.Client.PostAsJsonAsync($"/api/__tests/pending/{id}", new { label = "Transporte de pruebas" });
        var problem = await MySqlApiFixture.ReadJsonAsync(response, HttpStatusCode.NotImplemented);

        // Assert
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        Assert.Equal(id, problem.GetProperty("challengeId").GetString());
        Assert.Equal(name, problem.GetProperty("patternName").GetString());
        Assert.Equal("https://refactoring.guru/es/design-patterns/" + slug, problem.GetProperty("referenceUrl").GetString());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("correlationId").GetString()));
        Assert.Equal(auditsBefore, await fixture.WithDatabaseAsync(db => db.Audit.CountAsync(x => x.CompanyId == workspace.CompanyId)));
        Assert.Equal(0, await fixture.WithDatabaseAsync(db => db.JournalEntries.CountAsync(x => x.CompanyId == workspace.CompanyId)));
    }

    [Fact]
    public async Task InvalidPayload_Returns400BeforeTestPendingAction()
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();

        // Act
        using var response = await workspace.Client.PostAsJsonAsync("/api/__tests/pending/DP-01", new { });
        var problem = await MySqlApiFixture.ReadJsonAsync(response, HttpStatusCode.BadRequest);

        // Assert
        Assert.False(problem.TryGetProperty("challengeId", out _));
    }

    [Fact]
    public async Task NoIdentity_Returns401BeforeTestPendingAction()
    {
        // Arrange
        using var client = fixture.Factory.CreateClient();

        // Act
        using var response = await client.PostAsJsonAsync("/api/__tests/pending/DP-01", new { label = "Transporte" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ReadOnlyRole_Returns403BeforeTestPendingAction()
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();
        var email = Guid.NewGuid().ToString("N") + "@example.invalid";
        await workspace.PostAsync("/api/users", new { name = "Consulta de pruebas", email, password = fixture.Password, role = "Consulta" });
        using var member = await fixture.LoginAsync(email, fixture.Password);
        member.DefaultRequestHeaders.Add("X-Company-Id", workspace.CompanyId.ToString());

        // Act
        using var response = await member.PostAsJsonAsync("/api/__tests/pending/DP-01", new { label = "Transporte" });
        var problem = await MySqlApiFixture.ReadJsonAsync(response, HttpStatusCode.Forbidden);

        // Assert
        Assert.False(problem.TryGetProperty("challengeId", out _));
    }

    [Fact]
    public async Task UnexpectedFailure_Remains500InsteadOfAcademicPending()
    {
        // Arrange
        using var workspace = await fixture.CreateWorkspaceAsync();

        // Act
        using var response = await workspace.Client.PostAsJsonAsync("/api/__tests/unexpected", new { label = "Transporte" });
        var problem = await MySqlApiFixture.ReadJsonAsync(response, HttpStatusCode.InternalServerError);

        // Assert
        Assert.False(problem.TryGetProperty("challengeId", out _));
        Assert.DoesNotContain("Fallo inesperado exclusivo", problem.GetProperty("detail").GetString());
    }
}
