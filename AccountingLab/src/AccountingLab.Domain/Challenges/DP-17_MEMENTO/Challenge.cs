using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP17Memento;

public sealed class BudgetDraftLine
{
    public string AccountCode { get; set; } = "";
    public decimal Amount { get; set; }
}
public sealed class BudgetDraft
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public bool Approved { get; set; }
    public List<BudgetDraftLine> Lines { get; set; } = new();
}
public interface IBudgetSnapshot { }
public interface IBudgetHistoryChallenge
{
    IBudgetSnapshot Save(BudgetDraft draft);
    BudgetDraft Restore(BudgetDraft current, IBudgetSnapshot snapshot);
}

// --- CONCRETE MEMENTO (INMUTABLE + DEEP COPY) ---
internal sealed class BudgetDraftMemento : IBudgetSnapshot
{
    private readonly Guid _id;
    private readonly string _name;
    private readonly bool _approved;
    private readonly IReadOnlyList<(string AccountCode, decimal Amount)> _lines;

    public BudgetDraftMemento(BudgetDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        _id = draft.Id;
        _name = draft.Name ?? string.Empty;
        _approved = draft.Approved;
        _lines = (draft.Lines ?? Enumerable.Empty<BudgetDraftLine>())
            .Select(line =>
            {
                ArgumentNullException.ThrowIfNull(line);
                return (line.AccountCode ?? string.Empty, line.Amount);
            })
            .ToArray();
    }

    public void RestoreInto(BudgetDraft target)
    {
        ArgumentNullException.ThrowIfNull(target);

        target.Id = _id;
        target.Name = _name;
        target.Approved = _approved;
        target.Lines = _lines
            .Select(item => new BudgetDraftLine
            {
                AccountCode = item.AccountCode,
                Amount = item.Amount
            })
            .ToList();
    }
}

// --- ORIGINATOR / CARETAKER ENTRYPOINT ---
public sealed class BudgetHistoryChallenge : IBudgetHistoryChallenge
{
    public IBudgetSnapshot Save(BudgetDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);

        if (draft.Approved)
        {
            throw new InvalidOperationException("Cannot create an editing snapshot from an approved budget draft.");
        }

        return new BudgetDraftMemento(draft);
    }

    public BudgetDraft Restore(BudgetDraft current, IBudgetSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(snapshot);

        if (snapshot is not BudgetDraftMemento memento)
        {
            throw new ArgumentException("Invalid or foreign budget snapshot instance.", nameof(snapshot));
        }

        memento.RestoreInto(current);
        return current;
    }
}