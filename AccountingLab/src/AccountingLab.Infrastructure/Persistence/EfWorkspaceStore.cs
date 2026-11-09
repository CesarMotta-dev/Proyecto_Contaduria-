using System.Security.Cryptography;
using System.Text.Json;
using AccountingLab.Application.Contracts;
using AccountingLab.Domain.Model;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace AccountingLab.Infrastructure.Persistence;

public sealed class EfWorkspaceStore(AccountingDbContext db, TimeProvider clock) : IWorkspaceStore
{
    public async Task<PagedResult<T>> ListAsync<T>(Guid companyId, ListQuery query, CancellationToken ct) where T : class, ITenantEntity
    {
        IQueryable<T> source = db.Set<T>().AsNoTracking().Where(x => x.CompanyId == companyId);
        if (typeof(T) == typeof(JournalEntry)) source = source.Include("Lines");
        if (typeof(T) == typeof(Obligation) && query.Kind is not null) source = source.Where(x => EF.Property<string>(x, "Kind") == query.Kind);
        if (query.From is not null && typeof(T) == typeof(JournalEntry)) source = source.Where(x => EF.Property<DateOnly>(x, "Date") >= query.From);
        if (query.To is not null && typeof(T) == typeof(JournalEntry)) source = source.Where(x => EF.Property<DateOnly>(x, "Date") <= query.To);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var field = new[] { "Name", "Reference", "Description", "Title", "Action", "Number" }.FirstOrDefault(n => typeof(T).GetProperty(n)?.PropertyType == typeof(string));
            if (field is not null) source = source.Where(x => EF.Property<string>(x, field).Contains(query.Search));
        }
        var total = await source.CountAsync(ct);
        var items = await source.OrderBy(x => x.Id).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new(items, total, query.Page, query.PageSize);
    }
    public Task<T?> FindAsync<T>(Guid companyId, Guid id, CancellationToken ct) where T : class, ITenantEntity
    {
        IQueryable<T> source = db.Set<T>().AsNoTracking();
        if (typeof(T) == typeof(JournalEntry)) source = source.Include("Lines");
        return source.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, ct);
    }

    public async Task<T> AddAsync<T>(Guid companyId, Guid actorId, T entity, CancellationToken ct) where T : class, ITenantEntity
    {
        await ValidateReferencesAsync(companyId, entity, ct);
        db.Add(entity); AddAudit(companyId, actorId, entity.Id, typeof(T).Name, "Created");
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (IsUniqueConflict(ex)) { throw new ResourceConflictException("El código o referencia ya existe en esta empresa."); }
        return entity;
    }
    public async Task<T> UpdateAsync<T>(Guid companyId, Guid actorId, Guid id, T entity, CancellationToken ct) where T : class, ITenantEntity
    {
        var old = await db.Set<T>().FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, ct)
            ?? throw new ResourceNotFoundException("No se encontró el borrador.");
        if ((old is InvoiceDraft invoice && invoice.Status != "Draft") ||
            (old is BudgetDraft budget && budget.Status != "Draft"))
            throw new ResourceConflictException("Solo se pueden editar documentos que permanecen en borrador.");
        await ValidateReferencesAsync(companyId, entity, ct);
        db.Entry(old).CurrentValues.SetValues(entity);
        AddAudit(companyId, actorId, id, typeof(T).Name, "DraftUpdated");
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (IsUniqueConflict(ex)) { throw new ResourceConflictException("El código o referencia ya existe en esta empresa."); }
        return old;
    }
    private async Task ValidateReferencesAsync(Guid companyId, ITenantEntity entity, CancellationToken ct)
    {
        Guid? partyId = entity switch { Obligation x => x.ThirdPartyId, PaymentDraft x => x.ThirdPartyId, InvoiceDraft x => x.ThirdPartyId, _ => null };
        if (partyId is not null && !await db.ThirdParties.AnyAsync(x => x.CompanyId == companyId && x.Id == partyId, ct))
            throw new BusinessRuleException("El tercero no pertenece a la empresa activa.");
        if (entity is Account { ParentId: not null } account && !await db.Accounts.AnyAsync(x => x.Id == account.ParentId && x.CompanyId == companyId && x.IsGroup, ct))
            throw new BusinessRuleException("La cuenta agrupadora no existe en la empresa.");
        if (entity is AccountingPeriod period && await db.Periods.AnyAsync(x => x.CompanyId == companyId && x.StartDate <= period.EndDate && x.EndDate >= period.StartDate, ct))
            throw new BusinessRuleException("El periodo se superpone con otro periodo existente.");
    }
    public async Task<JournalResult> PostManualAsync(Guid companyId, Guid actorId, JournalEntry entry, CancellationToken ct)
    {
        entry.RequestHash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new
        { entry.Reference, entry.Date, Lines = entry.Lines.Select(x => new { x.AccountId, x.Debit, x.Credit, x.CostCenterId }) })));
        var previous = await db.JournalEntries.AsNoTracking().Include(x => x.Lines).FirstOrDefaultAsync(x => x.CompanyId == companyId && x.OperationId == entry.OperationId, ct);
        if (previous is not null) return Replay(previous, entry.RequestHash);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var accountIds = entry.Lines.Select(x => x.AccountId).Distinct().ToArray();
        var validAccounts = await db.Accounts.CountAsync(x => x.CompanyId == companyId && accountIds.Contains(x.Id) && !x.IsGroup && x.IsActive, ct);
        if (validAccounts != accountIds.Length) throw new BusinessRuleException("Todas las cuentas deben estar activas, ser de movimiento y pertenecer a la empresa.");
        if (!await db.Periods.AnyAsync(x => x.CompanyId == companyId && x.Status == "Open" && x.StartDate <= entry.Date && x.EndDate >= entry.Date, ct))
            throw new BusinessRuleException("La fecha requiere un periodo abierto de la empresa.");
        var costIds = entry.Lines.Where(x => x.CostCenterId.HasValue).Select(x => x.CostCenterId!.Value).Distinct().ToArray();
        if (await db.CostCenters.CountAsync(x => x.CompanyId == companyId && costIds.Contains(x.Id), ct) != costIds.Length)
            throw new BusinessRuleException("El centro de costos no pertenece a la empresa.");
        db.JournalEntries.Add(entry); AddAudit(companyId, actorId, entry.Id, "JournalEntry", "ManualPosted");
        try
        {
            await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
            return new(entry, false);
        }
        catch (DbUpdateException ex) when (IsUniqueConflict(ex))
        {
            await tx.RollbackAsync(ct); db.ChangeTracker.Clear();
            previous = await db.JournalEntries.AsNoTracking().Include(x => x.Lines).SingleAsync(x => x.CompanyId == companyId && x.OperationId == entry.OperationId, ct);
            return Replay(previous, entry.RequestHash);
        }
    }
    private static JournalResult Replay(JournalEntry previous, string requestHash) => previous.RequestHash == requestHash
        ? new(previous, true) : throw new ResourceConflictException("El identificador de operación ya fue usado con otro contenido.");

    public async Task<PagedResult<LedgerRow>> LedgerAsync(Guid companyId, Guid? accountId, ListQuery query, CancellationToken ct)
    {
        var source = from line in db.JournalLines.AsNoTracking()
                     join entry in db.JournalEntries.AsNoTracking() on line.EntryId equals entry.Id
                     join account in db.Accounts.AsNoTracking() on line.AccountId equals account.Id
                     where entry.CompanyId == companyId && (accountId == null || line.AccountId == accountId)
                     orderby entry.Date, entry.Id, line.Id
                     select new { entry.Id, entry.Date, entry.Reference, line.AccountId, AccountName = account.Name, line.Debit, line.Credit };
        var rows = await source.ToListAsync(ct);
        decimal balance = 0;
        var all = new List<LedgerRow>();
        foreach (var row in rows)
        {
            balance += row.Debit - row.Credit;
            if ((query.From is null || row.Date >= query.From) && (query.To is null || row.Date <= query.To))
                all.Add(new(row.Id, row.Date, row.Reference, row.AccountId, row.AccountName, row.Debit, row.Credit, balance));
        }
        return new(all.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToArray(), all.Count, query.Page, query.PageSize);
    }
    public async Task<Summary> SummaryAsync(Guid companyId, CancellationToken ct)
    {
        var lines = from line in db.JournalLines.AsNoTracking() join entry in db.JournalEntries on line.EntryId equals entry.Id where entry.CompanyId == companyId select line;
        var debits = await lines.SumAsync(x => (decimal?)x.Debit, ct) ?? 0;
        var credits = await lines.SumAsync(x => (decimal?)x.Credit, ct) ?? 0;
        var receivable = await db.Obligations.Where(x => x.CompanyId == companyId && x.Kind == "Receivable").SumAsync(x => (decimal?)x.Amount, ct) ?? 0;
        var payable = await db.Obligations.Where(x => x.CompanyId == companyId && x.Kind == "Payable").SumAsync(x => (decimal?)x.Amount, ct) ?? 0;
        return new(debits, credits, debits - credits, await db.JournalEntries.CountAsync(x => x.CompanyId == companyId, ct), receivable, payable);
    }
    private void AddAudit(Guid companyId, Guid actorId, Guid resourceId, string resource, string action) =>
        db.Audit.Add(new AuditEvent { CompanyId = companyId, UserId = actorId, ResourceId = resourceId, Resource = resource, Action = action, OccurredAt = clock.GetUtcNow().UtcDateTime });
    private static bool IsUniqueConflict(DbUpdateException ex) => ex.InnerException is MySqlException { Number: 1062 };
}
