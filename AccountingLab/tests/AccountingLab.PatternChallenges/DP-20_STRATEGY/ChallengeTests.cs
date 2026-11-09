using Xunit;
using AccountingLab.Domain.Challenges.DP20Strategy;

namespace AccountingLab.PatternChallenges.DP20Strategy;

[Trait("ChallengeId", "DP-20")]
public sealed class DP20StrategyTests
{
    [Fact]
    public void Configured_algorithm_and_rate_determine_fictional_tax()
    {
        var service = new ConfigurableTaxChallenge();
        var percentage = new FictionalTaxRule("DEMO-P", "venta", new(2026, 1, 1), new(2026, 12, 31), "percentage", 0.1375m, 0m);
        var result = service.Calculate(new(100m, "venta", new(2026, 1, 1), new[] { percentage }));
        Assert.Equal(13.7500m, result.TaxAmount);
        Assert.Equal(0.1375m, result.AppliedRate);
        Assert.Equal("DEMO-P", result.RuleName);
        var fixedRule = new FictionalTaxRule("DEMO-F", "servicio", new(2026, 1, 1), new(2026, 12, 31), "fixed", 0m, 7.2500m);
        var fixedResult = service.Calculate(new(100m, "servicio", new(2026, 6, 1), new[] { fixedRule }));
        Assert.Equal(7.2500m, fixedResult.TaxAmount);
        Assert.Equal("DEMO-F", fixedResult.RuleName);
    }
    [Fact]
    public void Missing_valid_rule_is_explicitly_rejected()
    {
        var expired = new FictionalTaxRule("DEMO-OLD", "venta", new(2025, 1, 1), new(2025, 12, 31), "percentage", 0.1m, 0m);
        Assert.Throws<InvalidOperationException>(() => new ConfigurableTaxChallenge().Calculate(new(100m, "venta", new(2026, 1, 1), new[] { expired })));
    }
    [Fact]
    public void Monetary_midpoint_uses_four_places_and_away_from_zero()
    {
        var rule = new FictionalTaxRule("DEMO-ROUND", "venta", new(2026, 1, 1), new(2026, 1, 31), "percentage", 0.005m, 0m);
        var result = new ConfigurableTaxChallenge().Calculate(new(0.01m, "venta", new(2026, 1, 31), new[] { rule }));
        Assert.Equal(0.0001m, result.TaxAmount);
    }
}

