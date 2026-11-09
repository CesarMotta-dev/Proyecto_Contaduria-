using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using AccountingLab.Api.Transport;
using AccountingLab.Domain.Education;
using AccountingLab.Domain.Model;
using AccountingLab.Infrastructure;
using AccountingLab.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

var migrateOnly = args.Contains("--migrate-only", StringComparer.Ordinal);
var builder = WebApplication.CreateBuilder(args.Where(x => x != "--migrate-only").ToArray());
var connection = builder.Configuration.GetConnectionString("AccountingLab") ?? throw new InvalidOperationException("Configura ConnectionStrings__AccountingLab para MySQL 8.");
builder.Services.AddAccountingLab(connection);
builder.Services.AddScoped<TenantContext>(); builder.Services.AddScoped<TenantAccessFilter>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.Converters.Add(new ExactDecimalConverter());
    var resolver = new DefaultJsonTypeInfoResolver(); resolver.Modifiers.Add(AnalysisJsonTypes.Configure);
    o.JsonSerializerOptions.TypeInfoResolver = resolver;
});
builder.Services.Configure<ApiBehaviorOptions>(o => o.InvalidModelStateResponseFactory = context =>
{
    var details = new ValidationProblemDetails(context.ModelState) { Status = 400, Title = "Solicitud inválida" };
    details.Extensions["code"] = "validation_error"; details.Extensions["correlationId"] = context.HttpContext.TraceIdentifier;
    return new BadRequestObjectResult(details);
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.CustomSchemaIds(t => (t.FullName ?? t.Name).Split('`')[0].Replace('.', '_').Replace('+', '_')
        + (t.IsGenericType ? "_" + string.Join("_", t.GetGenericArguments().Select(x => x.Name)) : ""));
    o.OperationFilter<TenantHeaderOperationFilter>();
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "AccountingLab — Laboratorio contable", Version = "v1", Description = "Base operativa y 22 desafíos académicos pendientes. Importes como cadenas decimales. Usa Bearer y X-Company-Id." });
    o.MapType<decimal>(() => new OpenApiSchema { Type = "string", Format = "decimal", Example = new OpenApiString("100.0000") });
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement { [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = [] });
});
if (!migrateOnly)
{
    var signingKey = builder.Configuration["Authentication:SigningKey"];
    if (string.IsNullOrWhiteSpace(signingKey) || Encoding.UTF8.GetByteCount(signingKey) < 32) throw new InvalidOperationException("Authentication__SigningKey debe contener al menos 32 bytes.");
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Authentication:Issuer"] ?? "AccountingLab", ValidAudience = builder.Configuration["Authentication:Audience"] ?? "AccountingLab.Web",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)), ClockSkew = TimeSpan.FromSeconds(30)
    });
    builder.Services.AddAuthorization();
}
var app = builder.Build();
if (migrateOnly)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AccountingDbContext>();
    await db.Database.MigrateAsync();
    await DemoSeed.ApplyAsync(db, builder.Configuration["Demo:Email"] ?? "docente@accountinglab.local", builder.Configuration["Demo:Password"] ?? "");
    return;
}
app.Use(async (context, next) =>
{
    try { await next(context); }
    catch (Exception error)
    {
        if (context.Response.HasStarted) throw;
        var status = error switch { PatternPendingException => 501, BusinessRuleException or JsonException or BadHttpRequestException => 400, AccessDeniedException => 403, ResourceNotFoundException => 404, ResourceConflictException or DbUpdateConcurrencyException => 409, _ => 500 };
        context.Response.Clear(); context.Response.StatusCode = status;
        var details = new ProblemDetails { Status = status, Title = status == 501 ? "Patrón de diseño pendiente" : "No se pudo completar la solicitud", Detail = status == 500 ? "Error inesperado. Consulta la correlación en los registros." : error.Message };
        details.Extensions["code"] = status == 501 ? "pattern_pending" : $"http_{status}"; details.Extensions["correlationId"] = context.TraceIdentifier;
        if (error is PatternPendingException pending)
        { details.Extensions["challengeId"] = pending.ChallengeId; details.Extensions["patternName"] = pending.PatternName; details.Extensions["referenceUrl"] = pending.ReferenceUrl; }
        if (status == 500) app.Logger.LogError(error, "Solicitud fallida {CorrelationId}", context.TraceIdentifier);
        await context.Response.WriteAsJsonAsync(details, options: (JsonSerializerOptions?)null, contentType: "application/problem+json", cancellationToken: context.RequestAborted);
    }
});
app.UseStatusCodePages(async statusContext =>
{
    var context = statusContext.HttpContext;
    await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = context.Response.StatusCode, Title = "Solicitud no admitida", Extensions = { ["code"] = $"http_{context.Response.StatusCode}", ["correlationId"] = context.TraceIdentifier } }, options: (JsonSerializerOptions?)null, contentType: "application/problem+json", cancellationToken: context.RequestAborted);
});
app.UseSwagger(); app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication(); app.UseAuthorization();
app.MapControllers();
app.MapGet("/health/live", () => Results.Ok(new { Status = "alive" })).AllowAnonymous();
app.MapGet("/health/ready", async (AccountingDbContext db) =>
{
    try { return await db.Database.CanConnectAsync() && !(await db.Database.GetPendingMigrationsAsync()).Any() ? Results.Ok(new { Status = "ready" }) : Results.StatusCode(503); }
    catch { return Results.StatusCode(503); }
}).AllowAnonymous();
app.Run();

public partial class Program;
