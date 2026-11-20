using AccountingLab.Domain.Education;

namespace AccountingLab.Application.Challenges.DP21TemplateMethod;

public sealed record BankStatementItem(string Id, string Reference, DateOnly Date, decimal Amount);
public sealed record ReconciliationLedgerItem(string Id, string Reference, DateOnly Date, decimal Amount);
public sealed record BankReconciliationRequest(string Mode, IReadOnlyList<BankStatementItem> BankItems, IReadOnlyList<ReconciliationLedgerItem> LedgerItems, decimal Tolerance);
public sealed record ReconciliationMatch(string BankItemId, string LedgerItemId, decimal Difference);
public sealed record BankReconciliationResult(IReadOnlyList<ReconciliationMatch> Matches, IReadOnlyList<string> UnmatchedBankItems, IReadOnlyList<string> UnmatchedLedgerItems);
public interface IBankReconciliationChallenge { BankReconciliationResult Reconcile(BankReconciliationRequest request); }

// --- ABSTRACT CLASS (TEMPLATE METHOD) ---
internal abstract class BankReconciliationTemplate
{
    public BankReconciliationResult Execute(BankReconciliationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Tolerance < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Tolerance),
                request.Tolerance,
                "Reconciliation tolerance cannot be negative.");
        }

        var bankItems = request.BankItems ?? Array.Empty<BankStatementItem>();
        var ledgerItems = request.LedgerItems ?? Array.Empty<ReconciliationLedgerItem>();

        var matches = new List<ReconciliationMatch>();
        var matchedBankIds = new HashSet<string>(StringComparer.Ordinal);
        var matchedLedgerIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var bankItem in bankItems)
        {
            ArgumentNullException.ThrowIfNull(bankItem);

            if (matchedBankIds.Contains(bankItem.Id))
            {
                continue;
            }

            var candidate = FindBestLedgerMatch(bankItem, ledgerItems, matchedLedgerIds, request.Tolerance);
            if (candidate is not null)
            {
                matchedBankIds.Add(bankItem.Id);
                matchedLedgerIds.Add(candidate.Id);

                var difference = bankItem.Amount - candidate.Amount;
                matches.Add(new ReconciliationMatch(bankItem.Id, candidate.Id, difference));
            }
        }

        var unmatchedBank = bankItems
            .Where(item => !matchedBankIds.Contains(item.Id))
            .Select(item => item.Id)
            .ToArray();

        var unmatchedLedger = ledgerItems
            .Where(item => !matchedLedgerIds.Contains(item.Id))
            .Select(item => item.Id)
            .ToArray();

        return new BankReconciliationResult(
            Matches: matches.AsReadOnly(),
            UnmatchedBankItems: unmatchedBank,
            UnmatchedLedgerItems: unmatchedLedger);
    }

    protected abstract ReconciliationLedgerItem? FindBestLedgerMatch(
        BankStatementItem bankItem,
        IReadOnlyList<ReconciliationLedgerItem> ledgerItems,
        IReadOnlySet<string> usedLedgerIds,
        decimal tolerance);

    protected static bool HasMatchingReference(BankStatementItem bankItem, ReconciliationLedgerItem ledgerItem)
    {
        if (string.IsNullOrWhiteSpace(bankItem.Reference) || string.IsNullOrWhiteSpace(ledgerItem.Reference))
        {
            return true;
        }

        return string.Equals(
            bankItem.Reference.Trim(),
            ledgerItem.Reference.Trim(),
            StringComparison.OrdinalIgnoreCase);
    }
}

// --- CONCRETE TEMPLATE 1: EXACT ---
internal sealed class ExactBankReconciliation : BankReconciliationTemplate
{
    protected override ReconciliationLedgerItem? FindBestLedgerMatch(
        BankStatementItem bankItem,
        IReadOnlyList<ReconciliationLedgerItem> ledgerItems,
        IReadOnlySet<string> usedLedgerIds,
        decimal tolerance)
    {
        return ledgerItems
            .Where(ledger =>
                ledger is not null &&
                !usedLedgerIds.Contains(ledger.Id) &&
                bankItem.Amount == ledger.Amount &&
                HasMatchingReference(bankItem, ledger))
            .OrderBy(ledger => Math.Abs(bankItem.Date.DayNumber - ledger.Date.DayNumber))
            .FirstOrDefault();
    }
}

// --- CONCRETE TEMPLATE 2: TOLERANCE ---
internal sealed class ToleranceBankReconciliation : BankReconciliationTemplate
{
    protected override ReconciliationLedgerItem? FindBestLedgerMatch(
        BankStatementItem bankItem,
        IReadOnlyList<ReconciliationLedgerItem> ledgerItems,
        IReadOnlySet<string> usedLedgerIds,
        decimal tolerance)
    {
        return ledgerItems
            .Where(ledger =>
                ledger is not null &&
                !usedLedgerIds.Contains(ledger.Id) &&
                Math.Abs(bankItem.Amount - ledger.Amount) <= tolerance &&
                HasMatchingReference(bankItem, ledger))
            .OrderBy(ledger => Math.Abs(bankItem.Amount - ledger.Amount))
            .ThenBy(ledger => Math.Abs(bankItem.Date.DayNumber - ledger.Date.DayNumber))
            .FirstOrDefault();
    }
}

// --- ENTRYPOINT ---
public sealed class BankReconciliationChallenge : IBankReconciliationChallenge
{
    public BankReconciliationResult Reconcile(BankReconciliationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Mode))
        {
            throw new ArgumentException("Reconciliation mode cannot be null or whitespace.", nameof(request));
        }

        BankReconciliationTemplate template = request.Mode.Trim().ToLowerInvariant() switch
        {
            "exact" or "exacto" => new ExactBankReconciliation(),
            "tolerance" or "tolerancia" => new ToleranceBankReconciliation(),
            _ => throw new ArgumentException($"Unsupported reconciliation mode '{request.Mode}'.", nameof(request))
        };

        return template.Execute(request);
    }
}