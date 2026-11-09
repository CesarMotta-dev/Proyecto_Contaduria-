using Xunit;
using AccountingLab.Domain.Challenges.DP22Visitor;

namespace AccountingLab.PatternChallenges.DP22Visitor;

[Trait("ChallengeId", "DP-22")]
public sealed class DP22VisitorTests
{
    [Fact]
    public void Analyzes_heterogeneous_documents_with_distinct_views()
    {
        IAccountingAnalysisDocument[] documents =
        {
            new InvoiceAnalysisDocument(Guid.NewGuid(), 100m, 13m),
            new PaymentAnalysisDocument(Guid.NewGuid(), 20m),
            new JournalAnalysisDocument(Guid.NewGuid(), new[] { new AnalysisPostingLine(50m, 0m), new AnalysisPostingLine(0m, 50m) })
        };
        var service = new DocumentAnalysisChallenge();
        var gross = service.Analyze(new("gross", documents));
        Assert.Equal(183m, gross.TotalAmount);
        Assert.Equal(3, gross.DocumentCount);
        Assert.Equal(113m, gross.ByKind["invoice"]);
        Assert.Equal(20m, gross.ByKind["payment"]);
        Assert.Equal(50m, gross.ByKind["journal"]);
        var count = service.Analyze(new("count", documents));
        Assert.Equal(1m, count.ByKind["invoice"]);
        Assert.Equal(1m, count.ByKind["payment"]);
        Assert.Equal(1m, count.ByKind["journal"]);
    }
    [Fact]
    public void Rejects_unknown_analysis_view()
    {
        Assert.Throws<ArgumentException>(() => new DocumentAnalysisChallenge().Analyze(new("desconocida", Array.Empty<IAccountingAnalysisDocument>())));
    }
    [Fact]
    public void Empty_document_structure_produces_zero_totals()
    {
        var result = new DocumentAnalysisChallenge().Analyze(new("gross", Array.Empty<IAccountingAnalysisDocument>()));
        Assert.Equal(0m, result.TotalAmount);
        Assert.Equal(0, result.DocumentCount);
        Assert.Empty(result.ByKind);
    }
}

