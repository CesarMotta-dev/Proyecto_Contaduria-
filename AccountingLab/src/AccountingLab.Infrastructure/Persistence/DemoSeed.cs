using AccountingLab.Domain.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AccountingLab.Infrastructure.Persistence;

public static class DemoSeed
{
    public static readonly Guid CompanyOne = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid CompanyTwo = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static async Task ApplyAsync(AccountingDbContext db, string email, string password, CancellationToken ct = default)
    {
        if (password.Length < 12) throw new InvalidOperationException("Demo__Password requiere al menos 12 caracteres.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        if (!await db.Companies.AnyAsync(x => x.Id == CompanyOne, ct))
            db.Companies.Add(new Company { Id = CompanyOne, Name = "Empresa Ficticia Aurora", TaxId = "DEMO-AURORA", Currency = "COP" });
        if (!await db.Companies.AnyAsync(x => x.Id == CompanyTwo, ct))
            db.Companies.Add(new Company { Id = CompanyTwo, Name = "Empresa Ficticia Boreal", TaxId = "DEMO-BOREAL", Currency = "COP" });
        var normalized = email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == normalized, ct);
        if (user is null)
        {
            user = new LabUser { Name = "Docente de demostración", Email = normalized };
            user.PasswordHash = new PasswordHasher<LabUser>().HashPassword(user, password); db.Users.Add(user);
        }
        await db.SaveChangesAsync(ct);
        foreach (var company in new[] { CompanyOne, CompanyTwo })
        {
            if (!await db.Memberships.AnyAsync(x => x.UserId == user.Id && x.CompanyId == company, ct))
                db.Memberships.Add(new Membership { UserId = user.Id, CompanyId = company, Role = "Administrador" });
            if (!await db.Accounts.AnyAsync(x => x.CompanyId == company, ct))
                db.Accounts.AddRange(new Account { CompanyId = company, Code = "1105", Name = "Caja de demostración" }, new Account { CompanyId = company, Code = "3105", Name = "Capital de demostración" });
            if (!await db.Periods.AnyAsync(x => x.CompanyId == company, ct))
                db.Periods.Add(new AccountingPeriod { CompanyId = company, Name = "Periodo didáctico 2026", StartDate = new(2026, 1, 1), EndDate = new(2026, 12, 31) });
            if (!await db.ThirdParties.AnyAsync(x => x.CompanyId == company, ct))
                db.ThirdParties.Add(new ThirdParty { CompanyId = company, Name = "Tercero ficticio", Document = "DEMO-TERCERO", Email = "tercero@example.invalid" });
            if (!await db.CostCenters.AnyAsync(x => x.CompanyId == company, ct))
                db.CostCenters.Add(new CostCenter { CompanyId = company, Code = "ADM", Name = "Administración ficticia" });
            if (!await db.Notices.AnyAsync(x => x.CompanyId == company, ct))
                db.Notices.Add(new Notice { CompanyId = company, Title = "Bienvenido al laboratorio", Message = "Aviso inicial ficticio. Las suscripciones DP-18 siguen pendientes.", IsDemo = true });
        }
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
    }
}
