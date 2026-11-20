using AccountingLab.Domain.Education;

namespace AccountingLab.Application.Challenges.DP10Facade;

public sealed record ClosingAccountBalance(string AccountCode, decimal Debit, decimal Credit);
public sealed record ClosePeriodRequest(Guid CompanyId, DateOnly From, DateOnly To, IReadOnlyList<ClosingAccountBalance> Balances, int UnpostedDocuments);
public sealed record PeriodClosingSummary(Guid CompanyId, DateOnly From, DateOnly To, bool Closed, decimal TotalDebit, decimal TotalCredit);
public interface IPeriodClosingChallenge { PeriodClosingSummary Close(ClosePeriodRequest request); }

// --- SUBSISTEMAS COORDINADOS POR LA FACHADA ---
internal sealed class PeriodValidationSubsystem
{
    public void EnsureCanClose(ClosePeriodRequest request)
    {
        if (request.CompanyId == Guid.Empty)
        {
            throw new ArgumentException("CompanyId is required.", nameof(request));
        }

        if (request.From > request.To)
        {
            throw new ArgumentException("Start date (From) cannot be later than end date (To).", nameof(request));
        }

        if (request.UnpostedDocuments < 0)
        {
            throw new ArgumentException("UnpostedDocuments cannot be negative.", nameof(request));
        }

        if (request.UnpostedDocuments > 0)
        {
            throw new InvalidOperationException(
                $"Cannot close period while there are {request.UnpostedDocuments} unposted document(s).");
        }
    }
}

internal sealed class LedgerConsolidationSubsystem
{
    public (decimal TotalDebit, decimal TotalCredit) Consolidate(IEnumerable<ClosingAccountBalance>? balances)
    {
        decimal totalDebit = 0m;
        decimal totalCredit = 0m;

        if (balances is not null)
        {
            foreach (var item in balances)
            {
                if (item is null)
                {
                    continue;
                }

                totalDebit += item.Debit;
                totalCredit += item.Credit;
            }
        }

        return (totalDebit, totalCredit);
    }
}

internal sealed class BalanceEquilibriumSubsystem
{
    public void EnsureBalanced(decimal totalDebit, decimal totalCredit)
    {
        if (totalDebit != totalCredit)
        {
            throw new InvalidOperationException(
                $"Period is out of balance: TotalDebit ({totalDebit}) != TotalCredit ({totalCredit}).");
        }
    }
}

// --- FACADE ---
public sealed class PeriodClosingChallenge : IPeriodClosingChallenge
{
    private readonly PeriodValidationSubsystem _validation;
    private readonly LedgerConsolidationSubsystem _consolidation;
    private readonly BalanceEquilibriumSubsystem _equilibrium;

    public PeriodClosingChallenge()
        : this(
            new PeriodValidationSubsystem(),
            new LedgerConsolidationSubsystem(),
            new BalanceEquilibriumSubsystem())
    {
    }

    internal PeriodClosingChallenge(
        PeriodValidationSubsystem validation,
        LedgerConsolidationSubsystem consolidation,
        BalanceEquilibriumSubsystem equilibrium)
    {
        _validation = validation ?? throw new ArgumentNullException(nameof(validation));
        _consolidation = consolidation ?? throw new ArgumentNullException(nameof(consolidation));
        _equilibrium = equilibrium ?? throw new ArgumentNullException(nameof(equilibrium));
    }

    public PeriodClosingSummary Close(ClosePeriodRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        _validation.EnsureCanClose(request);

        var (totalDebit, totalCredit) = _consolidation.Consolidate(request.Balances);

        _equilibrium.EnsureBalanced(totalDebit, totalCredit);

        return new PeriodClosingSummary(
            CompanyId: request.CompanyId,
            From: request.From,
            To: request.To,
            Closed: true,
            TotalDebit: totalDebit,
            TotalCredit: totalCredit);
    }
}