using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AccountingLab.Api.Transport;

public sealed class TenantHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType?.GetCustomAttributes(typeof(ServiceFilterAttribute), true)
            .Cast<ServiceFilterAttribute>().Any(x => x.ServiceType == typeof(TenantAccessFilter)) != true) return;
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Company-Id", In = ParameterLocation.Header, Required = true,
            Description = "UUID de la empresa activa. El servidor comprueba la pertenencia y el rol.",
            Schema = new OpenApiSchema { Type = "string", Format = "uuid" }
        });
    }
}
