using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP14Command;

public sealed record AccountingOperationRequest(Guid CompanyId, Guid OperationId, string Kind, Guid TargetId, decimal Amount);
public sealed record AccountingOperationReceipt(Guid OperationId, Guid TargetId, string Kind, decimal AppliedAmount, decimal RemainingAmount, Guid LinkedJournalEntryId, bool Replayed);
public sealed record AccountingOperationContext(decimal OutstandingAmount, bool TargetIsPublished, IReadOnlyList<AccountingOperationReceipt> CompletedOperations);
public interface IIdentifiedOperationChallenge { AccountingOperationReceipt Execute(AccountingOperationRequest request, AccountingOperationContext context); }

// --- COMMAND ABSTRACTION ---
internal interface IAccountingOperationCommand
{
    AccountingOperationReceipt Execute();
}

// --- CONCRETE COMMAND: APPLY ---
internal sealed class ApplyOperationCommand : IAccountingOperationCommand
{
    private readonly AccountingOperationRequest _request;
    private readonly AccountingOperationContext _context;

    public ApplyOperationCommand(AccountingOperationRequest request, AccountingOperationContext context)
    {
        _request = request;
        _context = context;
    }

    public AccountingOperationReceipt Execute()
    {
        if (_request.Amount > _context.OutstandingAmount)
        {
            throw new InvalidOperationException(
                $"Operation amount ({_request.Amount}) exceeds outstanding amount ({_context.OutstandingAmount}).");
        }

        var remaining = _context.OutstandingAmount - _request.Amount;

        return new AccountingOperationReceipt(
            OperationId: _request.OperationId,
            TargetId: _request.TargetId,
            Kind: _request.Kind.Trim(),
            AppliedAmount: _request.Amount,
            RemainingAmount: remaining,
            LinkedJournalEntryId: Guid.NewGuid(),
            Replayed: false);
    }
}

// --- CONCRETE COMMAND: REVERSE ---
internal sealed class ReverseOperationCommand : IAccountingOperationCommand
{
    private readonly AccountingOperationRequest _request;
    private readonly AccountingOperationContext _context;

    public ReverseOperationCommand(AccountingOperationRequest request, AccountingOperationContext context)
    {
        _request = request;
        _context = context;
    }

    public AccountingOperationReceipt Execute()
    {
        var completed = _context.CompletedOperations ?? Array.Empty<AccountingOperationReceipt>();

        // Verifica si la reversión apunta directamente a un TargetId de una operación aplicada o al OperationId original
        var originalApplied = completed.LastOrDefault(op =>
            !IsReversalKind(op.Kind) &&
            (op.TargetId == _request.TargetId || op.OperationId == _request.TargetId));

        if (originalApplied is not null && _request.Amount > originalApplied.AppliedAmount)
        {
            throw new InvalidOperationException(
                $"Reversal amount ({_request.Amount}) exceeds applied amount ({originalApplied.AppliedAmount}).");
        }

        var remaining = _context.OutstandingAmount + _request.Amount;

        return new AccountingOperationReceipt(
            OperationId: _request.OperationId,
            TargetId: _request.TargetId,
            Kind: _request.Kind.Trim(),
            AppliedAmount: _request.Amount,
            RemainingAmount: remaining,
            LinkedJournalEntryId: Guid.NewGuid(),
            Replayed: false);
    }

    internal static bool IsReversalKind(string kind)
    {
        return kind.Contains("revers", StringComparison.OrdinalIgnoreCase)
            || kind.Contains("revert", StringComparison.OrdinalIgnoreCase)
            || kind.Contains("anul", StringComparison.OrdinalIgnoreCase)
            || kind.Contains("undo", StringComparison.OrdinalIgnoreCase);
    }
}

// --- INVOKER / ENTRYPOINT ---
public sealed class IdentifiedOperationChallenge : IIdentifiedOperationChallenge
{
    public AccountingOperationReceipt Execute(AccountingOperationRequest request, AccountingOperationContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        if (request.CompanyId == Guid.Empty)
        {
            throw new ArgumentException("CompanyId is required.", nameof(request));
        }

        if (request.OperationId == Guid.Empty)
        {
            throw new ArgumentException("OperationId is required.", nameof(request));
        }

        if (request.TargetId == Guid.Empty)
        {
            throw new ArgumentException("TargetId is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Kind))
        {
            throw new ArgumentException("Operation Kind is required.", nameof(request));
        }

        // 1. Idempotencia por OperationId
        var existing = context.CompletedOperations?
            .FirstOrDefault(op => op.OperationId == request.OperationId);

        if (existing is not null)
        {
            if (existing.TargetId != request.TargetId ||
                !string.Equals(existing.Kind, request.Kind.Trim(), StringComparison.OrdinalIgnoreCase) ||
                existing.AppliedAmount != request.Amount)
            {
                throw new InvalidOperationException(
                    $"OperationId '{request.OperationId}' was already executed with different parameters.");
            }

            return existing with { Replayed = true };
        }

        // 2. Reglas comunes
        if (!context.TargetIsPublished)
        {
            throw new InvalidOperationException(
                $"Target '{request.TargetId}' must be published before executing operation '{request.Kind}'.");
        }

        if (request.Amount <= 0m)
        {
            throw new ArgumentException("Operation amount must be greater than zero.", nameof(request));
        }

        // 3. Selección y ejecución del comando concreto (Apply vs Reverse)
        IAccountingOperationCommand command = ReverseOperationCommand.IsReversalKind(request.Kind)
            ? new ReverseOperationCommand(request, context)
            : new ApplyOperationCommand(request, context);

        return command.Execute();
    }
}