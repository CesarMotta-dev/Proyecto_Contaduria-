using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP18Observer;

public sealed record FinancialEvent(Guid EventId, string Kind, decimal Amount);
public interface IFinancialNotificationsChallenge
{
    void Subscribe(string subscriberId, Action<FinancialEvent> receive);
    void Unsubscribe(string subscriberId);
    IReadOnlyList<string> Publish(FinancialEvent financialEvent);
}

// --- OBSERVER ABSTRACTION ---
internal interface IFinancialEventObserver
{
    string SubscriberId { get; }
    void OnFinancialEvent(FinancialEvent financialEvent);
}

// --- CONCRETE OBSERVER ---
internal sealed class DelegateFinancialObserver : IFinancialEventObserver
{
    private readonly Action<FinancialEvent> _receive;

    public string SubscriberId { get; }

    public DelegateFinancialObserver(string subscriberId, Action<FinancialEvent> receive)
    {
        if (string.IsNullOrWhiteSpace(subscriberId))
        {
            throw new ArgumentException("Subscriber identity cannot be null or whitespace.", nameof(subscriberId));
        }

        SubscriberId = subscriberId.Trim();
        _receive = receive ?? throw new ArgumentNullException(nameof(receive));
    }

    public void OnFinancialEvent(FinancialEvent financialEvent)
    {
        _receive(financialEvent);
    }
}

// --- SUBJECT / PUBLISHER ---
public sealed class FinancialNotificationsChallenge : IFinancialNotificationsChallenge
{
    private readonly Dictionary<string, IFinancialEventObserver> _observers = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _subscriptionOrder = new();

    public void Subscribe(string subscriberId, Action<FinancialEvent> receive)
    {
        ArgumentNullException.ThrowIfNull(receive);

        var observer = new DelegateFinancialObserver(subscriberId, receive);

        if (!_observers.TryAdd(observer.SubscriberId, observer))
        {
            throw new ArgumentException(
                $"Subscriber '{observer.SubscriberId}' is already subscribed.",
                nameof(subscriberId));
        }

        _subscriptionOrder.Add(observer.SubscriberId);
    }

    public void Unsubscribe(string subscriberId)
    {
        if (string.IsNullOrWhiteSpace(subscriberId))
        {
            return;
        }

        var normalizedId = subscriberId.Trim();
        if (_observers.Remove(normalizedId))
        {
            _subscriptionOrder.RemoveAll(id => string.Equals(id, normalizedId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public IReadOnlyList<string> Publish(FinancialEvent financialEvent)
    {
        ArgumentNullException.ThrowIfNull(financialEvent);

        var snapshotIds = _subscriptionOrder.ToArray();
        var notified = new List<string>(snapshotIds.Length);

        foreach (var id in snapshotIds)
        {
            if (_observers.TryGetValue(id, out var observer))
            {
                observer.OnFinancialEvent(financialEvent);
                notified.Add(observer.SubscriberId);
            }
        }

        return notified.AsReadOnly();
    }
}