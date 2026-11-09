using Xunit;
using AccountingLab.Domain.Challenges.DP18Observer;

namespace AccountingLab.PatternChallenges.DP18Observer;

[Trait("ChallengeId", "DP-18")]
public sealed class DP18ObserverTests
{
    [Fact]
    public void Delivers_financial_fact_once_to_each_active_subscriber()
    {
        var service = new FinancialNotificationsChallenge();
        var deliveries = new List<string>();
        var fact = new FinancialEvent(Guid.NewGuid(), "pago-registrado", 50m);
        service.Subscribe("tesoreria", item => deliveries.Add("tesoreria:" + item.EventId));
        service.Subscribe("auditoria", item => deliveries.Add("auditoria:" + item.EventId));
        var recipients = service.Publish(fact);
        Assert.Equal(new[] { "auditoria", "tesoreria" }, recipients.OrderBy(x => x));
        Assert.Equal(2, deliveries.Count);
        Assert.Contains("tesoreria:" + fact.EventId, deliveries);
        Assert.Contains("auditoria:" + fact.EventId, deliveries);
    }
    [Fact]
    public void Removed_subscriber_receives_no_future_event()
    {
        var service = new FinancialNotificationsChallenge();
        var calls = 0;
        service.Subscribe("tesoreria", _ => calls++);
        service.Unsubscribe("tesoreria");
        Assert.Empty(service.Publish(new(Guid.NewGuid(), "pago-registrado", 0m)));
        Assert.Equal(0, calls);
    }
    [Fact]
    public void Duplicate_subscriber_identity_is_rejected()
    {
        var service = new FinancialNotificationsChallenge();
        service.Subscribe("auditoria", _ => { });
        Assert.Throws<ArgumentException>(() => service.Subscribe("auditoria", _ => { }));
    }
}

