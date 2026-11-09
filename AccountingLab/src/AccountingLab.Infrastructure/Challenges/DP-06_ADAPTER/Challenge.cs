using AccountingLab.Domain.Education;

namespace AccountingLab.Infrastructure.Challenges.DP06Adapter;

public sealed record BankImportRequest(string Provider, string Payload);
public sealed record ImportedBankMovement(string ExternalId, DateOnly Date, string Description, decimal Debit, decimal Credit, string Currency);
public interface IBankImportChallenge { IReadOnlyList<ImportedBankMovement> Import(BankImportRequest request); }
public sealed class BankImportChallenge : IBankImportChallenge
{
    public IReadOnlyList<ImportedBankMovement> Import(BankImportRequest request)
    {
        // TODO(STUDENT): DP-06
        throw new PatternPendingException("DP-06");
    }
}

