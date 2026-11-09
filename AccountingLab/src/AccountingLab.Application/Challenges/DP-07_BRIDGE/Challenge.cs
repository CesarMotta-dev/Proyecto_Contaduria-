using AccountingLab.Domain.Education;

namespace AccountingLab.Application.Challenges.DP07Bridge;

public sealed record ReportPresentationRequest(string ReportType, string Format, decimal Amount);
public sealed record RenderedFinancialReport(string ReportType, string Format, string ContentType, string Content);
public interface IFinancialRepresentationChallenge { RenderedFinancialReport Render(ReportPresentationRequest request); }
public sealed class FinancialRepresentationChallenge : IFinancialRepresentationChallenge
{
    public RenderedFinancialReport Render(ReportPresentationRequest request)
    {
        // TODO(STUDENT): DP-07
        throw new PatternPendingException("DP-07");
    }
}

