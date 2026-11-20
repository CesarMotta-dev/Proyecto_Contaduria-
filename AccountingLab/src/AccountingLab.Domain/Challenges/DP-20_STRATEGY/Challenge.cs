using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP20Strategy;

public sealed record FictionalTaxRule(string Name, string OperationKind, DateOnly ValidFrom, DateOnly ValidTo, string CalculationKind, decimal Rate, decimal FixedAmount);
public sealed record TaxCalculationRequest(decimal BaseAmount, string OperationKind, DateOnly AccountingDate, IReadOnlyList<FictionalTaxRule> Rules);
public sealed record TaxCalculationResult(decimal BaseAmount, decimal AppliedRate, decimal TaxAmount, string RuleName);
public interface IConfigurableTaxChallenge { TaxCalculationResult Calculate(TaxCalculationRequest request); }

// --- STRATEGY ABSTRACTION ---
internal interface ITaxCalculationStrategy
{
    TaxCalculationResult Compute(decimal baseAmount, FictionalTaxRule rule);
}

// --- CONCRETE STRATEGIES ---
internal sealed class PercentageTaxStrategy : ITaxCalculationStrategy
{
    public TaxCalculationResult Compute(decimal baseAmount, FictionalTaxRule rule)
    {
        var rawTax = baseAmount * rule.Rate;
        var roundedTax = Math.Round(rawTax, 4, MidpointRounding.AwayFromZero);

        return new TaxCalculationResult(
            BaseAmount: baseAmount,
            AppliedRate: rule.Rate,
            TaxAmount: roundedTax,
            RuleName: rule.Name);
    }
}

internal sealed class FixedAmountTaxStrategy : ITaxCalculationStrategy
{
    public TaxCalculationResult Compute(decimal baseAmount, FictionalTaxRule rule)
    {
        var roundedTax = Math.Round(rule.FixedAmount, 4, MidpointRounding.AwayFromZero);

        return new TaxCalculationResult(
            BaseAmount: baseAmount,
            AppliedRate: 0m,
            TaxAmount: roundedTax,
            RuleName: rule.Name);
    }
}

internal sealed class HybridTaxStrategy : ITaxCalculationStrategy
{
    public TaxCalculationResult Compute(decimal baseAmount, FictionalTaxRule rule)
    {
        var rawTax = (baseAmount * rule.Rate) + rule.FixedAmount;
        var roundedTax = Math.Round(rawTax, 4, MidpointRounding.AwayFromZero);

        return new TaxCalculationResult(
            BaseAmount: baseAmount,
            AppliedRate: rule.Rate,
            TaxAmount: roundedTax,
            RuleName: rule.Name);
    }
}

// --- CONTEXT / ENTRYPOINT ---
public sealed class ConfigurableTaxChallenge : IConfigurableTaxChallenge
{
    private static readonly Dictionary<string, ITaxCalculationStrategy> Strategies =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["percentage"] = new PercentageTaxStrategy(),
            ["percent"] = new PercentageTaxStrategy(),
            ["rate"] = new PercentageTaxStrategy(),
            ["porcentaje"] = new PercentageTaxStrategy(),
            ["tasa"] = new PercentageTaxStrategy(),
            ["fixed"] = new FixedAmountTaxStrategy(),
            ["flat"] = new FixedAmountTaxStrategy(),
            ["fijo"] = new FixedAmountTaxStrategy(),
            ["hybrid"] = new HybridTaxStrategy(),
            ["mixed"] = new HybridTaxStrategy(),
            ["mixto"] = new HybridTaxStrategy()
        };

    public TaxCalculationResult Calculate(TaxCalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.BaseAmount < 0m)
        {
            throw new ArgumentException("BaseAmount cannot be negative.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.OperationKind))
        {
            throw new ArgumentException("OperationKind cannot be null or whitespace.", nameof(request));
        }

        var normalizedOperation = request.OperationKind.Trim();

        var applicableRule = request.Rules?
            .FirstOrDefault(rule =>
                rule is not null &&
                string.Equals(rule.OperationKind?.Trim(), normalizedOperation, StringComparison.OrdinalIgnoreCase) &&
                request.AccountingDate >= rule.ValidFrom &&
                request.AccountingDate <= rule.ValidTo);

        if (applicableRule is null)
        {
            throw new InvalidOperationException(
                $"No valid tax rule found for operation '{normalizedOperation}' on accounting date '{request.AccountingDate:yyyy-MM-dd}'.");
        }

        if (string.IsNullOrWhiteSpace(applicableRule.CalculationKind) ||
            !Strategies.TryGetValue(applicableRule.CalculationKind.Trim(), out var strategy))
        {
            throw new InvalidOperationException(
                $"Unsupported tax calculation strategy '{applicableRule.CalculationKind}' in rule '{applicableRule.Name}'.");
        }

        return strategy.Compute(request.BaseAmount, applicableRule);
    }
}