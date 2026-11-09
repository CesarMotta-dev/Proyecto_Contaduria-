using AccountingLab.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace AccountingLab.Infrastructure.Persistence;

public sealed class AccountingDbContext(DbContextOptions<AccountingDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<LabUser> Users => Set<LabUser>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AccountingPeriod> Periods => Set<AccountingPeriod>();
    public DbSet<ThirdParty> ThirdParties => Set<ThirdParty>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<TaxRule> TaxRules => Set<TaxRule>();
    public DbSet<Obligation> Obligations => Set<Obligation>();
    public DbSet<PaymentDraft> Payments => Set<PaymentDraft>();
    public DbSet<InvoiceDraft> Invoices => Set<InvoiceDraft>();
    public DbSet<BankStatement> BankStatements => Set<BankStatement>();
    public DbSet<ReconciliationDraft> Reconciliations => Set<ReconciliationDraft>();
    public DbSet<BudgetDraft> Budgets => Set<BudgetDraft>();
    public DbSet<ApprovalDraft> Approvals => Set<ApprovalDraft>();
    public DbSet<Notice> Notices => Set<Notice>();
    public DbSet<AuditEvent> Audit => Set<AuditEvent>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalLine> JournalLines => Set<JournalLine>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        foreach (var type in model.Model.GetEntityTypes().ToArray())
        {
            foreach (var property in type.GetProperties())
            {
                if (property.ClrType == typeof(string)) property.SetMaxLength(200);
                if (property.ClrType == typeof(decimal)) { property.SetPrecision(19); property.SetScale(4); }
            }
            if (typeof(ITenantEntity).IsAssignableFrom(type.ClrType))
            {
                var entity = model.Entity(type.ClrType);
                entity.HasAlternateKey("CompanyId", "Id");
                entity.HasOne(typeof(Company)).WithMany().HasForeignKey("CompanyId").OnDelete(DeleteBehavior.Restrict);
            }
        }
        model.Entity<LabUser>().HasIndex(x => x.Email).IsUnique();
        model.Entity<LabUser>().Property(x => x.PasswordHash).HasMaxLength(500);
        model.Entity<Company>().HasIndex(x => x.TaxId).IsUnique();
        model.Entity<Membership>().HasIndex(x => new { x.UserId, x.CompanyId }).IsUnique();
        model.Entity<Membership>().HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Membership>().HasOne<LabUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Account>().HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        model.Entity<Account>().HasOne<Account>().WithMany().HasForeignKey(x => new { x.CompanyId, x.ParentId }).HasPrincipalKey(x => new { x.CompanyId, x.Id }).OnDelete(DeleteBehavior.Restrict);
        model.Entity<CostCenter>().HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        model.Entity<InvoiceDraft>().HasIndex(x => new { x.CompanyId, x.Number }).IsUnique();
        model.Entity<TaxRule>().Property(x => x.Rate).HasPrecision(9, 6);
        foreach (var type in new[] { typeof(InvoiceDraft), typeof(PaymentDraft), typeof(Obligation) })
            model.Entity(type).HasOne(typeof(ThirdParty)).WithMany().HasForeignKey("CompanyId", "ThirdPartyId").HasPrincipalKey("CompanyId", "Id").OnDelete(DeleteBehavior.Restrict);
        model.Entity<JournalEntry>().HasIndex(x => new { x.CompanyId, x.OperationId }).IsUnique();
        model.Entity<JournalEntry>().HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.EntryId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<JournalLine>().HasOne<Account>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<JournalLine>().HasOne<CostCenter>().WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (ChangeTracker.Entries().Any(x => (x.Entity is JournalEntry or JournalLine or AuditEvent) && x.State is EntityState.Modified or EntityState.Deleted))
            throw new BusinessRuleException("Los hechos contables y su auditoría son inmutables.");
        return base.SaveChangesAsync(cancellationToken);
    }
}
