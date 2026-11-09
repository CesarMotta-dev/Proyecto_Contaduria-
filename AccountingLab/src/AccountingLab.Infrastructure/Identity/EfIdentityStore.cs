using AccountingLab.Application.Contracts;
using AccountingLab.Domain.Model;
using AccountingLab.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace AccountingLab.Infrastructure.Identity;

public sealed class EfIdentityStore(AccountingDbContext db) : IIdentityStore
{
    private readonly PasswordHasher<LabUser> hasher = new();
    public async Task<LoginIdentity?> AuthenticateAsync(string email, string password, CancellationToken ct)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Email == normalized, ct);
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed) return null;
        return new(user.Id, user.Name, user.Email, await CompaniesAsync(user.Id, ct));
    }
    public Task<string?> GetRoleAsync(Guid userId, Guid companyId, CancellationToken ct) =>
        db.Memberships.AsNoTracking().Where(x => x.UserId == userId && x.CompanyId == companyId).Select(x => x.Role).SingleOrDefaultAsync(ct);
    public async Task<IReadOnlyList<UserCompany>> CompaniesAsync(Guid userId, CancellationToken ct) =>
        await (from membership in db.Memberships.AsNoTracking() join company in db.Companies.AsNoTracking() on membership.CompanyId equals company.Id
               where membership.UserId == userId orderby company.Name select new UserCompany(company.Id, company.Name, company.Currency, membership.Role)).ToListAsync(ct);
    public async Task<Company> CreateCompanyAsync(Guid userId, Company company, CancellationToken ct)
    {
        if (!await db.Memberships.AnyAsync(x => x.UserId == userId && x.Role == "Administrador", ct)) throw new AccessDeniedException("Solo un administrador puede registrar empresas.");
        BasicDataRules.Required(company.Name, "Nombre"); BasicDataRules.Required(company.TaxId, "Identificación");
        company.Currency = company.Currency.Trim().ToUpperInvariant();
        if (company.Currency.Length != 3 || !company.Currency.All(char.IsAsciiLetterUpper)) throw new BusinessRuleException("La moneda debe tener tres letras mayúsculas.");
        company.Id = Guid.NewGuid();
        db.Companies.Add(company);
        db.Memberships.Add(new Membership { CompanyId = company.Id, UserId = userId, Role = "Administrador" });
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
        { throw new ResourceConflictException("La identificación de empresa ya existe."); }
        return company;
    }
    public async Task<IReadOnlyList<UserView>> UsersAsync(Guid companyId, CancellationToken ct) =>
        await (from membership in db.Memberships.AsNoTracking() join user in db.Users.AsNoTracking() on membership.UserId equals user.Id
               where membership.CompanyId == companyId orderby user.Name select new UserView(user.Id, user.Name, user.Email, membership.Role)).ToListAsync(ct);
    public async Task<UserView> AddUserAsync(Guid companyId, NewUser input, CancellationToken ct)
    {
        BasicDataRules.Required(input.Name, "Nombre"); BasicDataRules.Required(input.Email, "Email");
        if (input.Password.Length < 12 || !input.Email.Contains('@') || input.Role is not ("Administrador" or "Contador" or "Consulta"))
            throw new BusinessRuleException("Se requiere correo válido, contraseña de al menos 12 caracteres y rol reconocido.");
        var normalized = input.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == normalized, ct)) throw new ResourceConflictException("El correo ya está registrado.");
        var user = new LabUser { Name = input.Name.Trim(), Email = normalized };
        user.PasswordHash = hasher.HashPassword(user, input.Password);
        db.Users.Add(user); db.Memberships.Add(new Membership { UserId = user.Id, CompanyId = companyId, Role = input.Role });
        await db.SaveChangesAsync(ct);
        return new(user.Id, user.Name, user.Email, input.Role);
    }
}
