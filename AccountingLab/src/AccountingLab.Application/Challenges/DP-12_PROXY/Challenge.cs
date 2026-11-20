using AccountingLab.Domain.Education;

namespace AccountingLab.Application.Challenges.DP12Proxy;

public sealed record SensitiveReportRequest(Guid CompanyId, bool IsAuthorized, string Scope);
public sealed record SensitiveReport(Guid CompanyId, string Scope, decimal ConfidentialAmount);
public interface ISensitiveReportChallenge { SensitiveReport Get(SensitiveReportRequest request, Func<SensitiveReport> loadReport); }

// --- SUBJECT / REAL SUBJECT / PROXY ---
internal interface ISensitiveReportSource
{
    SensitiveReport Fetch(SensitiveReportRequest request);
}

internal sealed class DelegateSensitiveReportSource : ISensitiveReportSource
{
    private readonly Func<SensitiveReport> _loadReport;

    public DelegateSensitiveReportSource(Func<SensitiveReport> loadReport)
    {
        _loadReport = loadReport ?? throw new ArgumentNullException(nameof(loadReport));
    }

    public SensitiveReport Fetch(SensitiveReportRequest request)
    {
        return _loadReport();
    }
}

internal sealed class SensitiveReportProtectionProxy : ISensitiveReportSource
{
    private readonly ISensitiveReportSource _realSource;

    public SensitiveReportProtectionProxy(ISensitiveReportSource realSource)
    {
        _realSource = realSource ?? throw new ArgumentNullException(nameof(realSource));
    }

    public SensitiveReport Fetch(SensitiveReportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!request.IsAuthorized)
        {
            throw new UnauthorizedAccessException(
                $"Access denied to sensitive report '{request.Scope}' for company '{request.CompanyId}'.");
        }

        var report = _realSource.Fetch(request)
            ?? throw new InvalidOperationException("Sensitive report source returned null.");

        if (report.CompanyId != request.CompanyId)
        {
            throw new InvalidOperationException(
                $"Company mismatch: expected '{request.CompanyId}', but source returned '{report.CompanyId}'.");
        }

        return report;
    }
}

// --- ENTRYPOINT ---
public sealed class SensitiveReportChallenge : ISensitiveReportChallenge
{
    public SensitiveReport Get(SensitiveReportRequest request, Func<SensitiveReport> loadReport)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(loadReport);

        ISensitiveReportSource source = new SensitiveReportProtectionProxy(
            new DelegateSensitiveReportSource(loadReport));

        return source.Fetch(request);
    }
}
