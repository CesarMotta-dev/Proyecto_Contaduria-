using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP18Observer;

public sealed record FinancialEvent(Guid EventId, string Kind, decimal Amount);
public interface IFinancialNotificationsChallenge
{
    void Subscribe(string subscriberId, Action<FinancialEvent> receive);
    void Unsubscribe(string subscriberId);
    IReadOnlyList<string> Publish(FinancialEvent financialEvent);
}
public sealed class FinancialNotificationsChallenge : IFinancialNotificationsChallenge
{
    public void Subscribe(string subscriberId, Action<FinancialEvent> receive)
    {
        // TODO(STUDENT): DP-18
        throw new PatternPendingException("DP-18");
    }
    public void Unsubscribe(string subscriberId)
    {
        // TODO(STUDENT): DP-18
        throw new PatternPendingException("DP-18");
    }
    public IReadOnlyList<string> Publish(FinancialEvent financialEvent)
    {
        // TODO(STUDENT): DP-18
        throw new PatternPendingException("DP-18");
    }
}

