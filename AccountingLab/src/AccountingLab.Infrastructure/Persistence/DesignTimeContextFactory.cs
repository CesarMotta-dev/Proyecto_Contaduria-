using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AccountingLab.Infrastructure.Persistence;

public sealed class DesignTimeContextFactory : IDesignTimeDbContextFactory<AccountingDbContext>
{
    public AccountingDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__AccountingLab")
            ?? "Server=localhost;Port=3308;Database=accountinglab;User=accountinglab;Password=design-time-not-used";
        return new AccountingDbContext(new DbContextOptionsBuilder<AccountingDbContext>()
            .UseMySql(connection, new MySqlServerVersion(new Version(8, 4, 0))).Options);
    }
}
