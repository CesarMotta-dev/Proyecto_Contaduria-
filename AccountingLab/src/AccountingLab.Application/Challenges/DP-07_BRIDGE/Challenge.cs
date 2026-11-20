using System.Globalization;
using System.Text;
using System.Text.Json;
using AccountingLab.Domain.Education;

namespace AccountingLab.Application.Challenges.DP07Bridge;

public sealed record ReportPresentationRequest(string ReportType, string Format, decimal Amount);
public sealed record RenderedFinancialReport(string ReportType, string Format, string ContentType, string Content);
public interface IFinancialRepresentationChallenge { RenderedFinancialReport Render(ReportPresentationRequest request); }

// --- IMPLEMENTOR (Representaciones / formatos disponibles) ---
public interface IReportRepresentation
{
    string FormatKey { get; }
    string ContentType { get; }
    string RenderContent(string reportType, string format, decimal amount);
}

public sealed class JsonReportRepresentation : IReportRepresentation
{
    public string FormatKey => "json";
    public string ContentType => "application/json";

    public string RenderContent(string reportType, string format, decimal amount)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
        {
            writer.WriteStartObject();
            writer.WriteString("reportType", reportType);
            writer.WriteString("format", format);
            writer.WriteString("amount", amount.ToString("0.0000", CultureInfo.InvariantCulture));
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}

public sealed class CsvReportRepresentation : IReportRepresentation
{
    public string FormatKey => "csv";
    public string ContentType => "text/csv";

    public string RenderContent(string reportType, string format, decimal amount)
    {
        var formattedAmount = amount.ToString(CultureInfo.InvariantCulture);
        return $"ReportType,Format,Amount\n{reportType},{format},{formattedAmount}";
    }
}

public sealed class TextReportRepresentation : IReportRepresentation
{
    public string FormatKey => "txt";
    public string ContentType => "text/plain";

    public string RenderContent(string reportType, string format, decimal amount)
    {
        var formattedAmount = amount.ToString(CultureInfo.InvariantCulture);
        return $"{reportType} ({format}): {formattedAmount}";
    }
}

public sealed class XmlReportRepresentation : IReportRepresentation
{
    public string FormatKey => "xml";
    public string ContentType => "application/xml";

    public string RenderContent(string reportType, string format, decimal amount)
    {
        var formattedAmount = amount.ToString(CultureInfo.InvariantCulture);
        return $"<report type=\"{reportType}\" format=\"{format}\"><amount>{formattedAmount}</amount></report>";
    }
}

public sealed class MarkdownReportRepresentation : IReportRepresentation
{
    public string FormatKey => "md";
    public string ContentType => "text/markdown";

    public string RenderContent(string reportType, string format, decimal amount)
    {
        var formattedAmount = amount.ToString(CultureInfo.InvariantCulture);
        return $"# {reportType}\n- Format: {format}\n- Amount: {formattedAmount}";
    }
}

// --- ABSTRACTION (Puente hacia la representación elegida) ---
public abstract class FinancialReportView
{
    protected readonly IReportRepresentation Representation;

    protected FinancialReportView(IReportRepresentation representation)
    {
        Representation = representation ?? throw new ArgumentNullException(nameof(representation));
    }

    public abstract RenderedFinancialReport Present(string reportType, string format, decimal amount);
}

public sealed class StandardFinancialReportView : FinancialReportView
{
    public StandardFinancialReportView(IReportRepresentation representation) : base(representation) { }

    public override RenderedFinancialReport Present(string reportType, string format, decimal amount)
    {
        var content = Representation.RenderContent(reportType, format, amount);
        return new RenderedFinancialReport(reportType, format, Representation.ContentType, content);
    }
}

public sealed class FinancialRepresentationChallenge : IFinancialRepresentationChallenge
{
    private readonly IReadOnlyDictionary<string, IReportRepresentation> _representations;

    public FinancialRepresentationChallenge()
        : this(
        [
            new JsonReportRepresentation(),
            new CsvReportRepresentation(),
            new TextReportRepresentation(),
            new XmlReportRepresentation(),
            new MarkdownReportRepresentation()
        ])
    {
    }

    public FinancialRepresentationChallenge(IEnumerable<IReportRepresentation> representations)
    {
        ArgumentNullException.ThrowIfNull(representations);

        var map = new Dictionary<string, IReportRepresentation>(StringComparer.OrdinalIgnoreCase);
        foreach (var rep in representations)
        {
            map[rep.FormatKey.Trim()] = rep;
        }

        // Alias habitual para texto plano
        if (map.TryGetValue("txt", out var txtRep) && !map.ContainsKey("text"))
        {
            map["text"] = txtRep;
        }

        _representations = map;
    }

    public RenderedFinancialReport Render(ReportPresentationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.ReportType))
        {
            throw new ArgumentException("Report type is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Format))
        {
            throw new ArgumentException("Report format is required.", nameof(request));
        }

        var normalizedFormat = request.Format.Trim();
        if (!_representations.TryGetValue(normalizedFormat, out var representation))
        {
            throw new ArgumentException($"Unavailable representation format '{request.Format}'.", nameof(request));
        }

        FinancialReportView view = new StandardFinancialReportView(representation);
        return view.Present(request.ReportType.Trim(), request.Format.Trim(), request.Amount);
    }
}