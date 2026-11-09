using AccountingLab.Domain.Education;

namespace AccountingLab.Application.Challenges.DP12Proxy;

public sealed record SensitiveReportRequest(Guid CompanyId, bool MayRead, string ReportKey);
public sealed record SensitiveFinancialReport(Guid CompanyId, string ReportKey, decimal ConfidentialAmount);
public interface ISensitiveReportChallenge { SensitiveFinancialReport Get(SensitiveReportRequest request, Func<SensitiveFinancialReport> loadReport); }
public sealed class SensitiveReportChallenge : ISensitiveReportChallenge
{
    public SensitiveFinancialReport Get(SensitiveReportRequest request, Func<SensitiveFinancialReport> loadReport)
    {
        // TODO(STUDENT): DP-12
        throw new PatternPendingException("DP-12");
    }
}

