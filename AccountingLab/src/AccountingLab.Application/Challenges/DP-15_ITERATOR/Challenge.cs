using AccountingLab.Domain.Education;

namespace AccountingLab.Application.Challenges.DP15Iterator;

public sealed record LedgerRecord(string EntryCode, DateOnly Date, decimal Debit, decimal Credit);
public sealed record LedgerPage(IReadOnlyList<LedgerRecord> Records);
public interface ILedgerCursor { bool MoveNext(); LedgerRecord Current { get; } }
public interface ILedgerTraversalChallenge { ILedgerCursor Open(IReadOnlyList<LedgerPage> pages); }

// --- CONCRETE ITERATOR ---
internal sealed class PaginatedLedgerCursor : ILedgerCursor
{
    private readonly IReadOnlyList<LedgerPage> _pages;
    private int _pageIndex;
    private int _recordIndex = -1;
    private LedgerRecord? _current;
    private bool _exhausted;

    public PaginatedLedgerCursor(IReadOnlyList<LedgerPage> pages)
    {
        _pages = pages ?? throw new ArgumentNullException(nameof(pages));
    }

    public LedgerRecord Current =>
        _current ?? throw new InvalidOperationException("Cursor is not positioned on a valid ledger record.");

    public bool MoveNext()
    {
        if (_exhausted)
        {
            return false;
        }

        while (_pageIndex < _pages.Count)
        {
            var page = _pages[_pageIndex];
            var records = page?.Records;

            if (records is not null && _recordIndex + 1 < records.Count)
            {
                _recordIndex++;
                _current = records[_recordIndex];
                return true;
            }

            _pageIndex++;
            _recordIndex = -1;
        }

        _current = null;
        _exhausted = true;
        return false;
    }
}

// --- AGGREGATE / ENTRYPOINT ---
public sealed class LedgerTraversalChallenge : ILedgerTraversalChallenge
{
    public ILedgerCursor Open(IReadOnlyList<LedgerPage> pages)
    {
        ArgumentNullException.ThrowIfNull(pages);
        return new PaginatedLedgerCursor(pages);
    }
}