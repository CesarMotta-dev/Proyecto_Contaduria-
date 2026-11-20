using AccountingLab.Domain.Education;

namespace AccountingLab.Application.Challenges.DP03Builder;

public sealed record FinancialReportRequest(string Title, DateOnly From, DateOnly To, IReadOnlyList<string> Sections, IReadOnlyList<string> Annexes);
public sealed record FinancialReport(string Title, DateOnly From, DateOnly To, IReadOnlyList<string> Sections, IReadOnlyList<string> Annexes);

public sealed class FinancialReportBuilder
{
    private string _title = string.Empty;
    private DateOnly _from;
    private DateOnly _to;
    private readonly List<string> _sections = new();
    private readonly List<string> _annexes = new();

    public FinancialReportBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public FinancialReportBuilder WithPeriod(DateOnly from, DateOnly to)
    {
        _from = from;
        _to = to;
        return this;
    }

    public FinancialReportBuilder AddSection(string section)
    {
        _sections.Add(section);
        return this;
    }

    public FinancialReportBuilder AddSections(IEnumerable<string> sections)
    {
        _sections.AddRange(sections);
        return this;
    }

    public FinancialReportBuilder AddAnnex(string annex)
    {
        _annexes.Add(annex);
        return this;
    }

    public FinancialReportBuilder AddAnnexes(IEnumerable<string> annexes)
    {
        _annexes.AddRange(annexes);
        return this;
    }

    public FinancialReport Build()
    {
        if (_from > _to)
        {
            throw new ArgumentException("The start date (From) cannot be later than the end date (To).", nameof(_from));
        }

        return new FinancialReport(
            _title,
            _from,
            _to,
            _sections.AsReadOnly(),
            _annexes.AsReadOnly());
    }
}

public interface IReportAssemblyChallenge { FinancialReport Build(FinancialReportRequest request); }
public sealed class ReportAssemblyChallenge : IReportAssemblyChallenge
{
    public FinancialReport Build(FinancialReportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var builder = new FinancialReportBuilder()
            .WithTitle(request.Title)
            .WithPeriod(request.From, request.To);

        if (request.Sections is not null)
        {
            foreach (var section in request.Sections)
            {
                builder.AddSection(section);
            }
        }

        if (request.Annexes is not null)
        {
            foreach (var annex in request.Annexes)
            {
                builder.AddAnnex(annex);
            }
        }

        return builder.Build();
    }
}