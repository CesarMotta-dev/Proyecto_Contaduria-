using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP19State;

public sealed record InvoiceLifecycleRequest(string CurrentState, string Action, decimal OutstandingAmount, DateOnly DueDate, DateOnly BusinessDate);
public sealed record InvoiceTransitionResult(string State, bool PaymentAllowed);
public interface IInvoiceLifecycleChallenge { InvoiceTransitionResult Transition(InvoiceLifecycleRequest request); }

// --- STATE ABSTRACTION ---
internal interface IInvoiceState
{
    string Name { get; }
    bool PaymentAllowed { get; }
    IInvoiceState Emit(InvoiceLifecycleRequest request);
    IInvoiceState Pay(InvoiceLifecycleRequest request);
    IInvoiceState MarkOverdue(InvoiceLifecycleRequest request);
}

// --- CONCRETE STATES ---
internal sealed class DraftInvoiceState : IInvoiceState
{
    public string Name => "draft";
    public bool PaymentAllowed => false;

    public IInvoiceState Emit(InvoiceLifecycleRequest request)
    {
        if (request.OutstandingAmount <= 0m)
        {
            throw new InvalidOperationException("Cannot emit an invoice without a positive outstanding amount.");
        }

        return request.BusinessDate > request.DueDate
            ? new OverdueInvoiceState()
            : new IssuedInvoiceState();
    }

    public IInvoiceState Pay(InvoiceLifecycleRequest request)
        => throw new InvalidOperationException("Cannot pay an invoice that is still in draft state.");

    public IInvoiceState MarkOverdue(InvoiceLifecycleRequest request)
        => throw new InvalidOperationException("Cannot mark a draft invoice as overdue before it is issued.");
}

internal sealed class IssuedInvoiceState : IInvoiceState
{
    public string Name => "issued";
    public bool PaymentAllowed => true;

    public IInvoiceState Emit(InvoiceLifecycleRequest request)
        => throw new InvalidOperationException("Invoice has already been emitted.");

    public IInvoiceState Pay(InvoiceLifecycleRequest request)
        => new PaidInvoiceState();

    public IInvoiceState MarkOverdue(InvoiceLifecycleRequest request)
        => request.BusinessDate > request.DueDate && request.OutstandingAmount > 0m
            ? new OverdueInvoiceState()
            : this;
}

internal sealed class OverdueInvoiceState : IInvoiceState
{
    public string Name => "overdue";
    public bool PaymentAllowed => true;

    public IInvoiceState Emit(InvoiceLifecycleRequest request)
        => throw new InvalidOperationException("Overdue invoice has already been emitted.");

    public IInvoiceState Pay(InvoiceLifecycleRequest request)
        => new PaidInvoiceState();

    public IInvoiceState MarkOverdue(InvoiceLifecycleRequest request)
        => request.BusinessDate <= request.DueDate
            ? new IssuedInvoiceState()
            : this;
}

internal sealed class PaidInvoiceState : IInvoiceState
{
    public string Name => "paid";
    public bool PaymentAllowed => false;

    public IInvoiceState Emit(InvoiceLifecycleRequest request)
        => throw new InvalidOperationException("Already paid invoice cannot be emitted again.");

    public IInvoiceState Pay(InvoiceLifecycleRequest request)
        => throw new InvalidOperationException("Invoice is already paid.");

    public IInvoiceState MarkOverdue(InvoiceLifecycleRequest request)
        => throw new InvalidOperationException("Paid invoice cannot become overdue.");
}

// --- CONTEXT / ENTRYPOINT ---
public sealed class InvoiceLifecycleChallenge : IInvoiceLifecycleChallenge
{
    public InvoiceTransitionResult Transition(InvoiceLifecycleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.CurrentState))
        {
            throw new ArgumentException("CurrentState cannot be null or whitespace.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Action))
        {
            throw new ArgumentException("Action cannot be null or whitespace.", nameof(request));
        }

        var currentState = ResolveState(request.CurrentState.Trim());
        var nextState = ApplyAction(currentState, request.Action.Trim(), request);

        return new InvoiceTransitionResult(nextState.Name, nextState.PaymentAllowed);
    }

    private static IInvoiceState ResolveState(string state)
    {
        return state.ToLowerInvariant() switch
        {
            "draft" or "borrador" => new DraftInvoiceState(),
            "issued" or "emitted" or "emitida" or "emitido" => new IssuedInvoiceState(),
            "overdue" or "vencida" or "vencido" => new OverdueInvoiceState(),
            "paid" or "pagada" or "pagado" => new PaidInvoiceState(),
            _ => throw new ArgumentException($"Unknown invoice state '{state}'.", nameof(state))
        };
    }

    private static IInvoiceState ApplyAction(
        IInvoiceState currentState,
        string action,
        InvoiceLifecycleRequest request)
    {
        return action.ToLowerInvariant() switch
        {
            "emit" or "issue" or "emitir" or "publish" or "publicar" => currentState.Emit(request),
            "pay" or "pagar" or "settle" or "liquidar" => currentState.Pay(request),
            "mark-overdue" or "mark_overdue" or "evaluate" or "check-due" or "refresh" => currentState.MarkOverdue(request),
            _ => throw new ArgumentException($"Unknown invoice lifecycle action '{action}'.", nameof(action))
        };
    }
}