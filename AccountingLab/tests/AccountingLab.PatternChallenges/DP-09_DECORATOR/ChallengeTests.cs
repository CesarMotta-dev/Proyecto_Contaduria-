using Xunit;
using AccountingLab.Application.Challenges.DP09Decorator;

namespace AccountingLab.PatternChallenges.DP09Decorator;

[Trait("ChallengeId", "DP-09")]
public sealed class DP09DecoratorTests
{
    [Fact]
    public void Combines_optional_features_without_changing_original_document()
    {
        var metadata = new Dictionary<string, string> { ["empresa"] = "DEMO" };
        var request = new DocumentExportRequest("Comprobante 001", new[] { "auditoria", "metadatos" }, metadata);
        var result = new DocumentEnrichmentChallenge().Export(request);
        Assert.Contains("Comprobante 001", result.Content);
        Assert.Contains("[AUDIT]", result.Content);
        Assert.Equal("DEMO", result.Metadata["empresa"]);
        Assert.Equal(request.Features, result.AppliedFeatures);
        Assert.Equal("Comprobante 001", request.Body);
    }
    [Fact]
    public void Rejects_unknown_export_feature()
    {
        Assert.Throws<ArgumentException>(() => new DocumentEnrichmentChallenge().Export(new("Documento", new[] { "desconocida" }, new Dictionary<string, string>())));
    }
    [Fact]
    public void Export_without_optional_features_preserves_plain_content()
    {
        var result = new DocumentEnrichmentChallenge().Export(new("Documento limpio", Array.Empty<string>(), new Dictionary<string, string>()));
        Assert.Equal("Documento limpio", result.Content);
        Assert.Empty(result.AppliedFeatures);
        Assert.Empty(result.Metadata);
    }
}

