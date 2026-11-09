using AccountingLab.Domain.Model;

namespace AccountingLab.Application.Contracts;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
public sealed record ListQuery(int Page = 1, int PageSize = 20, string? Search = null, string? Kind = null, DateOnly? From = null, DateOnly? To = null);
public sealed record JournalLineInput(Guid AccountId, decimal Debit, decimal Credit, Guid? CostCenterId = null);
public sealed record JournalInput(Guid OperationId, string Reference, DateOnly Date, List<JournalLineInput> Lines);
public sealed record JournalResult(JournalEntry Entry, bool Replayed);
public sealed record LedgerRow(Guid EntryId, DateOnly Date, string Reference, Guid AccountId, string AccountName, decimal Debit, decimal Credit, decimal Balance);
public sealed record Summary(decimal Debits, decimal Credits, decimal Balance, int EntryCount, decimal ReceivableTotal, decimal PayableTotal);
public sealed record UserCompany(Guid Id, string Name, string Currency, string Role);
public sealed record UserView(Guid Id, string Name, string Email, string Role);
public sealed record NewUser(string Name, string Email, string Password, string Role);
public sealed record LoginIdentity(Guid Id, string Name, string Email, IReadOnlyList<UserCompany> Companies);

public interface IWorkspaceStore
{
    Task<PagedResult<T>> ListAsync<T>(Guid companyId, ListQuery query, CancellationToken ct) where T : class, ITenantEntity;
    Task<T?> FindAsync<T>(Guid companyId, Guid id, CancellationToken ct) where T : class, ITenantEntity;
    Task<T> AddAsync<T>(Guid companyId, Guid actorId, T entity, CancellationToken ct) where T : class, ITenantEntity;
    Task<T> UpdateAsync<T>(Guid companyId, Guid actorId, Guid id, T entity, CancellationToken ct) where T : class, ITenantEntity;
    Task<JournalResult> PostManualAsync(Guid companyId, Guid actorId, JournalEntry entry, CancellationToken ct);
    Task<PagedResult<LedgerRow>> LedgerAsync(Guid companyId, Guid? accountId, ListQuery query, CancellationToken ct);
    Task<Summary> SummaryAsync(Guid companyId, CancellationToken ct);
}
public interface IIdentityStore
{
    Task<LoginIdentity?> AuthenticateAsync(string email, string password, CancellationToken ct);
    Task<string?> GetRoleAsync(Guid userId, Guid companyId, CancellationToken ct);
    Task<IReadOnlyList<UserCompany>> CompaniesAsync(Guid userId, CancellationToken ct);
    Task<Company> CreateCompanyAsync(Guid userId, Company company, CancellationToken ct);
    Task<IReadOnlyList<UserView>> UsersAsync(Guid companyId, CancellationToken ct);
    Task<UserView> AddUserAsync(Guid companyId, NewUser user, CancellationToken ct);
}
