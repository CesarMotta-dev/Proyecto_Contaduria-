using AccountingLab.Domain.Education;

namespace AccountingLab.Application.Challenges.DP09Decorator;

public sealed record DocumentExportRequest(string Body, IReadOnlyList<string> Features, IReadOnlyDictionary<string, string> Metadata);
public sealed record ExportedAccountingDocument(string Content, IReadOnlyList<string> AppliedFeatures, IReadOnlyDictionary<string, string> Metadata);
public interface IDocumentEnrichmentChallenge { ExportedAccountingDocument Export(DocumentExportRequest request); }
public sealed class DocumentEnrichmentChallenge : IDocumentEnrichmentChallenge
{
    public ExportedAccountingDocument Export(DocumentExportRequest request)
    {
        // TODO(STUDENT): DP-09
        throw new PatternPendingException("DP-09");
    }
}

