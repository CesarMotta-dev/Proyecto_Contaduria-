using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP22Visitor;

public interface IAccountingAnalysisDocument { Guid Id { get; } }
public sealed record InvoiceAnalysisDocument(Guid Id, decimal NetAmount, decimal TaxAmount) : IAccountingAnalysisDocument;
public sealed record PaymentAnalysisDocument(Guid Id, decimal Amount) : IAccountingAnalysisDocument;
public sealed record AnalysisPostingLine(decimal Debit, decimal Credit);
public sealed record JournalAnalysisDocument(Guid Id, IReadOnlyList<AnalysisPostingLine> Lines) : IAccountingAnalysisDocument;
public sealed record DocumentAnalysisRequest(string View, IReadOnlyList<IAccountingAnalysisDocument> Documents);
public sealed record DocumentAnalysisResult(decimal TotalAmount, int DocumentCount, IReadOnlyDictionary<string, decimal> ByKind);
public interface IDocumentAnalysisChallenge { DocumentAnalysisResult Analyze(DocumentAnalysisRequest request); }

// --- VISITOR ABSTRACTION ---
internal interface IAccountingDocumentVisitor
{
    decimal VisitInvoice(InvoiceAnalysisDocument invoice);
    decimal VisitPayment(PaymentAnalysisDocument payment);
    decimal VisitJournal(JournalAnalysisDocument journal);
}

// --- CONCRETE VISITORS ---
internal sealed class GrossDocumentVisitor : IAccountingDocumentVisitor
{
    public decimal VisitInvoice(InvoiceAnalysisDocument invoice) => invoice.NetAmount + invoice.TaxAmount;
    public decimal VisitPayment(PaymentAnalysisDocument payment) => payment.Amount;
    public decimal VisitJournal(JournalAnalysisDocument journal) => (journal.Lines ?? Array.Empty<AnalysisPostingLine>()).Sum(line => line.Debit);
}

internal sealed class NetDocumentVisitor : IAccountingDocumentVisitor
{
    public decimal VisitInvoice(InvoiceAnalysisDocument invoice) => invoice.NetAmount;
    public decimal VisitPayment(PaymentAnalysisDocument payment) => payment.Amount;
    public decimal VisitJournal(JournalAnalysisDocument journal) => (journal.Lines ?? Array.Empty<AnalysisPostingLine>()).Sum(line => line.Debit);
}

internal sealed class TaxDocumentVisitor : IAccountingDocumentVisitor
{
    public decimal VisitInvoice(InvoiceAnalysisDocument invoice) => invoice.TaxAmount;
    public decimal VisitPayment(PaymentAnalysisDocument payment) => 0m;
    public decimal VisitJournal(JournalAnalysisDocument journal) => 0m;
}

internal sealed class CountDocumentVisitor : IAccountingDocumentVisitor
{
    public decimal VisitInvoice(InvoiceAnalysisDocument invoice) => 1m;
    public decimal VisitPayment(PaymentAnalysisDocument payment) => 1m;
    public decimal VisitJournal(JournalAnalysisDocument journal) => 1m;
}

// --- ENTRYPOINT / DISPATCHER ---
public sealed class DocumentAnalysisChallenge : IDocumentAnalysisChallenge
{
    public DocumentAnalysisResult Analyze(DocumentAnalysisRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.View))
        {
            throw new ArgumentException("Analysis view cannot be null or whitespace.", nameof(request));
        }

        IAccountingDocumentVisitor visitor = request.View.Trim().ToLowerInvariant() switch
        {
            "gross" or "bruto" or "total" => new GrossDocumentVisitor(),
            "net" or "neto" => new NetDocumentVisitor(),
            "tax" or "impuesto" or "impuestos" or "iva" => new TaxDocumentVisitor(),
            "count" or "conteo" or "cantidad" => new CountDocumentVisitor(),
            _ => throw new ArgumentException($"Unknown analysis view '{request.View}'.", nameof(request))
        };

        var documents = request.Documents ?? Array.Empty<IAccountingAnalysisDocument>();
        var byKind = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var total = 0m;

        foreach (var document in documents)
        {
            ArgumentNullException.ThrowIfNull(document);

            var (kind, amount) = document switch
            {
                InvoiceAnalysisDocument invoice => ("invoice", visitor.VisitInvoice(invoice)),
                PaymentAnalysisDocument payment => ("payment", visitor.VisitPayment(payment)),
                JournalAnalysisDocument journal => ("journal", visitor.VisitJournal(journal)),
                _ => throw new ArgumentException($"Unsupported document type '{document.GetType().Name}'.", nameof(request))
            };

            total += amount;
            byKind[kind] = byKind.GetValueOrDefault(kind) + amount;
        }

        return new DocumentAnalysisResult(
            TotalAmount: total,
            DocumentCount: documents.Count,
            ByKind: byKind.AsReadOnly());
    }
}