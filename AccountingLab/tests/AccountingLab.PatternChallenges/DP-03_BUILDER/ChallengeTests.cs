using Xunit;
using AccountingLab.Application.Challenges.DP03Builder;

namespace AccountingLab.PatternChallenges.DP03Builder;

[Trait("ChallengeId", "DP-03")]
public sealed class DP03BuilderTests
{
    [Fact]
    public void Produces_selected_sections_annexes_and_date_filter()
    {
        var request = new FinancialReportRequest("Balance demo", new(2026, 1, 1), new(2026, 1, 31), new[] { "activos", "pasivos" }, new[] { "notas" });
        var result = new ReportAssemblyChallenge().Build(request);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.From, result.From);
        Assert.Equal(request.To, result.To);
        Assert.Equal(request.Sections, result.Sections);
        Assert.Equal(request.Annexes, result.Annexes);
    }
    [Fact]
    public void Rejects_report_with_inverted_accounting_interval()
    {
        Assert.Throws<ArgumentException>(() => new ReportAssemblyChallenge().Build(new("Inválido", new(2026, 2, 1), new(2026, 1, 1), new[] { "activos" }, Array.Empty<string>())));
    }
    [Fact]
    public void Supports_minimal_report_without_optional_annexes()
    {
        var result = new ReportAssemblyChallenge().Build(new("Mínimo", new(2026, 1, 1), new(2026, 1, 1), new[] { "activos" }, Array.Empty<string>()));
        Assert.Single(result.Sections);
        Assert.Empty(result.Annexes);
    }
}

