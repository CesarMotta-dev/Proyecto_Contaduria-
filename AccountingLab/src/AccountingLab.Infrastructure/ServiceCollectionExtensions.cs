using AccountingLab.Application.Contracts;
using AccountingLab.Application.Services;
using AccountingLab.Infrastructure.Identity;
using AccountingLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AccountingLab.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccountingLab(this IServiceCollection services, string connection)
    {
        services.AddDbContext<AccountingDbContext>(o => o.UseMySql(connection, new MySqlServerVersion(new Version(8, 4, 0))));
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IWorkspaceStore, EfWorkspaceStore>();
        services.AddScoped<IIdentityStore, EfIdentityStore>();
        services.AddScoped<CatalogService>();
        return services;
    }
}
