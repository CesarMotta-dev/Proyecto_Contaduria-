using AccountingLab.Domain.Education;

namespace AccountingLab.Application.Challenges.DP03Builder;

public sealed record FinancialReportRequest(string Title, DateOnly From, DateOnly To, IReadOnlyList<string> Sections, IReadOnlyList<string> Annexes);
public sealed record FinancialReport(string Title, DateOnly From, DateOnly To, IReadOnlyList<string> Sections, IReadOnlyList<string> Annexes);
public interface IReportAssemblyChallenge { FinancialReport Build(FinancialReportRequest request); }
public sealed class ReportAssemblyChallenge : IReportAssemblyChallenge
{
    public FinancialReport Build(FinancialReportRequest request)
    {
        // TODO(STUDENT): DP-03
        throw new PatternPendingException("DP-03");
    }
}

