using System.Security.Claims;
using AccountingLab.Application.Contracts;
using AccountingLab.Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AccountingLab.Api.Transport;

public sealed class TenantContext
{
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = "";
}
public sealed class TenantAccessFilter(IIdentityStore identity, TenantContext tenant) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;
        if (!Guid.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? http.User.FindFirstValue("sub"), out var userId))
        { context.Result = new UnauthorizedResult(); return; }
        if (!Guid.TryParse(http.Request.Headers["X-Company-Id"], out var companyId))
            throw new BusinessRuleException("El encabezado X-Company-Id es obligatorio y debe ser un UUID válido.");
        var role = await identity.GetRoleAsync(userId, companyId, http.RequestAborted);
        if (role is null) throw new AccessDeniedException("No perteneces a la empresa solicitada.");
        tenant.UserId = userId; tenant.CompanyId = companyId; tenant.Role = role;
        if (http.Request.Method is not ("GET" or "HEAD") && role == "Consulta")
            throw new AccessDeniedException("El rol Consulta no permite modificar datos ni ejecutar acciones avanzadas.");
        await next();
    }
}
