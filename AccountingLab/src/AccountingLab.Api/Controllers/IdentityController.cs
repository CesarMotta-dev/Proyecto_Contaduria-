using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AccountingLab.Application.Contracts;
using AccountingLab.Domain.Model;
using AccountingLab.Api.Transport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AccountingLab.Api.Controllers;

public sealed record LoginRequest(string Email, string Password);

[ApiController, Route("api/auth")]
public sealed class IdentityController(IIdentityStore identity, IConfiguration configuration) : ControllerBase
{
    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password)) return Unauthorized();
        var user = await identity.AuthenticateAsync(request.Email, request.Password, ct);
        if (user is null) return Unauthorized();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Authentication:SigningKey"]!));
        var token = new JwtSecurityToken(configuration["Authentication:Issuer"] ?? "AccountingLab", configuration["Authentication:Audience"] ?? "AccountingLab.Web",
            [new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim(JwtRegisteredClaimNames.Email, user.Email)],
            expires: DateTime.UtcNow.AddHours(2), signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return Ok(new { AccessToken = new JwtSecurityTokenHandler().WriteToken(token), User = new { user.Id, user.Name, user.Email }, user.Companies });
    }
}

[ApiController, Route("api/companies"), Authorize]
public sealed class CompaniesController(IIdentityStore identity) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        AccountingLab.Application.Services.CatalogService.ValidatePage(new(page, pageSize));
        var companies = await identity.CompaniesAsync(UserId, ct);
        return Ok(new PagedResult<UserCompany>(companies.Skip((page - 1) * pageSize).Take(pageSize).ToArray(), companies.Count, page, pageSize));
    }
    [HttpPost]
    public async Task<IActionResult> Create(Company input, CancellationToken ct) => Ok(await identity.CreateCompanyAsync(UserId, input, ct));
}

[ApiController, Route("api/users"), Authorize, ServiceFilter(typeof(TenantAccessFilter))]
public sealed class UsersController(IIdentityStore identity, TenantContext tenant) : ControllerBase
{
    private void RequireAdmin() { if (tenant.Role != "Administrador") throw new AccessDeniedException("La gestión de usuarios requiere rol Administrador."); }
    [HttpGet]
    public async Task<IActionResult> List(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        RequireAdmin(); AccountingLab.Application.Services.CatalogService.ValidatePage(new(page, pageSize));
        var users = await identity.UsersAsync(tenant.CompanyId, ct);
        return Ok(new PagedResult<UserView>(users.Skip((page - 1) * pageSize).Take(pageSize).ToArray(), users.Count, page, pageSize));
    }
    [HttpPost]
    public async Task<IActionResult> Create(NewUser input, CancellationToken ct)
    { RequireAdmin(); return Ok(await identity.AddUserAsync(tenant.CompanyId, input, ct)); }
}
