using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP13ChainOfResponsibility;

public sealed record PaymentApprovalRequest(decimal Amount, decimal AvailableFunds, bool PolicyAllows, decimal AuthorizationLimit, IReadOnlyList<string> Checks);
public sealed record PaymentApprovalDecision(bool Approved, string? RejectedBy, IReadOnlyList<string> VisitedChecks);
public interface IPaymentApprovalChallenge { PaymentApprovalDecision Evaluate(PaymentApprovalRequest request); }

// --- HANDLER BASE ---
internal abstract class PaymentApprovalHandler
{
    private PaymentApprovalHandler? _next;

    public string CheckName { get; }

    protected PaymentApprovalHandler(string checkName)
    {
        CheckName = checkName;
    }

    public PaymentApprovalHandler SetNext(PaymentApprovalHandler next)
    {
        _next = next;
        return next;
    }

    public PaymentApprovalDecision Handle(PaymentApprovalRequest request, List<string> visitedChecks)
    {
        visitedChecks.Add(CheckName);

        if (!Passes(request))
        {
            return new PaymentApprovalDecision(
                Approved: false,
                RejectedBy: CheckName,
                VisitedChecks: visitedChecks.AsReadOnly());
        }

        if (_next is not null)
        {
            return _next.Handle(request, visitedChecks);
        }

        return new PaymentApprovalDecision(
            Approved: true,
            RejectedBy: null,
            VisitedChecks: visitedChecks.AsReadOnly());
    }

    protected abstract bool Passes(PaymentApprovalRequest request);
}

// --- CONCRETE HANDLERS ---
internal sealed class AvailableFundsHandler : PaymentApprovalHandler
{
    public AvailableFundsHandler(string checkName) : base(checkName) { }

    protected override bool Passes(PaymentApprovalRequest request)
        => request.Amount >= 0m && request.Amount <= request.AvailableFunds;
}

internal sealed class PolicyHandler : PaymentApprovalHandler
{
    public PolicyHandler(string checkName) : base(checkName) { }

    protected override bool Passes(PaymentApprovalRequest request)
        => request.PolicyAllows;
}

internal sealed class AuthorizationLimitHandler : PaymentApprovalHandler
{
    public AuthorizationLimitHandler(string checkName) : base(checkName) { }

    protected override bool Passes(PaymentApprovalRequest request)
        => request.Amount >= 0m && request.Amount <= request.AuthorizationLimit;
}

// --- COORDINATOR ---
public sealed class PaymentApprovalChallenge : IPaymentApprovalChallenge
{
    private static readonly Dictionary<string, Func<string, PaymentApprovalHandler>> HandlerFactories =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["funds"] = name => new AvailableFundsHandler(name),
            ["fondos"] = name => new AvailableFundsHandler(name),
            ["available_funds"] = name => new AvailableFundsHandler(name),
            ["saldo"] = name => new AvailableFundsHandler(name),
            ["policy"] = name => new PolicyHandler(name),
            ["politica"] = name => new PolicyHandler(name),
            ["política"] = name => new PolicyHandler(name),
            ["limit"] = name => new AuthorizationLimitHandler(name),
            ["limite"] = name => new AuthorizationLimitHandler(name),
            ["límite"] = name => new AuthorizationLimitHandler(name),
            ["authorization"] = name => new AuthorizationLimitHandler(name),
            ["autorizacion"] = name => new AuthorizationLimitHandler(name),
            ["autorización"] = name => new AuthorizationLimitHandler(name)
        };

    public PaymentApprovalDecision Evaluate(PaymentApprovalRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Checks is null || request.Checks.Count == 0)
        {
            return new PaymentApprovalDecision(
                Approved: true,
                RejectedBy: null,
                VisitedChecks: Array.Empty<string>());
        }

        PaymentApprovalHandler? head = null;
        PaymentApprovalHandler? current = null;

        foreach (var rawCheck in request.Checks)
        {
            if (string.IsNullOrWhiteSpace(rawCheck))
            {
                throw new ArgumentException("Check name cannot be null or whitespace.", nameof(request));
            }

            var checkKey = rawCheck.Trim();
            if (!HandlerFactories.TryGetValue(checkKey, out var createHandler))
            {
                throw new ArgumentException($"Unknown payment approval check '{rawCheck}'.", nameof(request));
            }

            var nextHandler = createHandler(checkKey);
            if (head is null)
            {
                head = nextHandler;
                current = nextHandler;
            }
            else
            {
                current = current!.SetNext(nextHandler);
            }
        }

        var visited = new List<string>(request.Checks.Count);
        return head!.Handle(request, visited);
    }
}
