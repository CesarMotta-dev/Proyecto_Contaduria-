using Xunit;
using AccountingLab.Application.Challenges.DP07Bridge;

namespace AccountingLab.PatternChallenges.DP07Bridge;

[Trait("ChallengeId", "DP-07")]
public sealed class DP07BridgeTests
{
    [Fact]
    public void Each_report_type_supports_each_available_representation()
    {
        foreach (var reportType in new[] { "balance", "resultados" })
        foreach (var format in new[] { "json", "csv" })
        {
            var result = new FinancialRepresentationChallenge().Render(new(reportType, format, 125.2500m));
            Assert.Equal(reportType, result.ReportType);
            Assert.Equal(format, result.Format);
            Assert.Contains("125.2500", result.Content);
            Assert.Contains(reportType, result.Content);
            Assert.Equal(format == "json" ? "application/json" : "text/csv", result.ContentType);
        }
    }
    [Fact]
    public void Rejects_unavailable_representation()
    {
        Assert.Throws<ArgumentException>(() => new FinancialRepresentationChallenge().Render(new("balance", "desconocido", 10m)));
    }
    [Fact]
    public void Zero_amount_remains_exact_in_json_content()
    {
        var result = new FinancialRepresentationChallenge().Render(new("balance", "json", 0m));
        using var json = System.Text.Json.JsonDocument.Parse(result.Content);
        Assert.Equal("balance", json.RootElement.GetProperty("reportType").GetString());
        Assert.Equal("0.0000", json.RootElement.GetProperty("amount").GetString());
    }
}

