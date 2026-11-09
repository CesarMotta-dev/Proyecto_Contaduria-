using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP13ChainOfResponsibility;

public sealed record PaymentApprovalRequest(decimal Amount, decimal AvailableFunds, bool PolicyAllows, decimal AuthorizationLimit, IReadOnlyList<string> Checks);
public sealed record PaymentApprovalDecision(bool Approved, string? RejectedBy, IReadOnlyList<string> VisitedChecks);
public interface IPaymentApprovalChallenge { PaymentApprovalDecision Evaluate(PaymentApprovalRequest request); }
public sealed class PaymentApprovalChallenge : IPaymentApprovalChallenge
{
    public PaymentApprovalDecision Evaluate(PaymentApprovalRequest request)
    {
        // TODO(STUDENT): DP-13
        throw new PatternPendingException("DP-13");
    }
}

