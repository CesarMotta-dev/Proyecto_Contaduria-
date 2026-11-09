using AccountingLab.Application.Contracts;
using AccountingLab.Domain.Model;

namespace AccountingLab.Application.Services;

public sealed class CatalogService(IWorkspaceStore store)
{
    public Task<PagedResult<T>> ListAsync<T>(Guid companyId, ListQuery query, CancellationToken ct) where T : class, ITenantEntity
    {
        ValidatePage(query);
        return store.ListAsync<T>(companyId, query, ct);
    }
    public async Task<T> GetAsync<T>(Guid companyId, Guid id, CancellationToken ct) where T : class, ITenantEntity =>
        await store.FindAsync<T>(companyId, id, ct) ?? throw new ResourceNotFoundException("No se encontró el recurso en la empresa activa.");
    public Task<T> AddAsync<T>(Guid companyId, Guid actorId, T entity, CancellationToken ct) where T : class, ITenantEntity
    {
        entity.Id = Guid.NewGuid(); entity.CompanyId = companyId;
        BasicDataRules.Validate(entity);
        return store.AddAsync(companyId, actorId, entity, ct);
    }
    public Task<T> UpdateAsync<T>(Guid companyId, Guid actorId, Guid id, T entity, CancellationToken ct) where T : class, ITenantEntity
    {
        if (entity is not (InvoiceDraft or BudgetDraft)) throw new BusinessRuleException("Solo se permite editar borradores de facturas y presupuestos.");
        entity.Id = id; entity.CompanyId = companyId;
        BasicDataRules.Validate(entity);
        return store.UpdateAsync(companyId, actorId, id, entity, ct);
    }
    public Task<JournalResult> PostJournalAsync(Guid companyId, Guid actorId, JournalInput input, CancellationToken ct)
    {
        if (input.Lines is null) throw new BusinessRuleException("Las líneas son obligatorias.");
        var entry = new JournalEntry { CompanyId = companyId, OperationId = input.OperationId, Reference = input.Reference, Date = input.Date,
            Lines = input.Lines.Select(x => new JournalLine { AccountId = x.AccountId, Debit = x.Debit, Credit = x.Credit, CostCenterId = x.CostCenterId }).ToList() };
        JournalRules.Validate(entry);
        return store.PostManualAsync(companyId, actorId, entry, ct);
    }
    public Task<PagedResult<LedgerRow>> LedgerAsync(Guid companyId, Guid? accountId, ListQuery query, CancellationToken ct)
    { ValidatePage(query); return store.LedgerAsync(companyId, accountId, query, ct); }
    public Task<Summary> SummaryAsync(Guid companyId, CancellationToken ct) => store.SummaryAsync(companyId, ct);
    public static void ValidatePage(ListQuery query)
    {
        if (query.Page < 1 || query.PageSize < 1 || query.PageSize > 100 ||
            ((long)query.Page - 1) * query.PageSize > int.MaxValue || query.To < query.From)
            throw new BusinessRuleException("Paginación o intervalo de fechas inválidos; pageSize debe estar entre 1 y 100.");
    }
}
