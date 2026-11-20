using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP01FactoryMethod;

public sealed record VoucherCreationRequest(Guid CompanyId, string Kind, decimal Amount, string Currency);
public sealed record VoucherDraft(Guid Id, Guid CompanyId, string Kind, decimal Amount, string Currency);
public interface IVoucherCreationChallenge { VoucherDraft Create(VoucherCreationRequest request); }

public abstract class VoucherDraftFactory
{
    public abstract IReadOnlyList<string> SupportedKinds { get; }

    public VoucherDraft CreateDraft(VoucherCreationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Amount < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Voucher amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            throw new ArgumentException("Currency is required.", nameof(request));
        }

        return new VoucherDraft(
            Id: Guid.NewGuid(),
            CompanyId: request.CompanyId,
            Kind: request.Kind.Trim().ToLowerInvariant(),
            Amount: request.Amount,
            Currency: request.Currency.Trim().ToUpperInvariant());
    }
}

public sealed class IncomeVoucherFactory : VoucherDraftFactory
{
    public override IReadOnlyList<string> SupportedKinds => ["ingreso", "ingresos", "receipt", "income"];
}

public sealed class ExpenseVoucherFactory : VoucherDraftFactory
{
    public override IReadOnlyList<string> SupportedKinds => ["egreso", "egresos", "gasto", "gastos", "payment", "expense"];
}

public sealed class JournalVoucherFactory : VoucherDraftFactory
{
    public override IReadOnlyList<string> SupportedKinds => ["diario", "ajuste", "traslado", "nota", "journal", "transfer"];
}

public sealed class VoucherCreationChallenge : IVoucherCreationChallenge
{
    private readonly Dictionary<string, VoucherDraftFactory> _factoriesByKind;

    public VoucherCreationChallenge()
        : this(
        [
            new IncomeVoucherFactory(),
            new ExpenseVoucherFactory(),
            new JournalVoucherFactory()
        ])
    {
    }

    public VoucherCreationChallenge(IEnumerable<VoucherDraftFactory> factories)
    {
        ArgumentNullException.ThrowIfNull(factories);

        _factoriesByKind = new Dictionary<string, VoucherDraftFactory>(StringComparer.OrdinalIgnoreCase);
        foreach (var factory in factories)
        {
            foreach (var kind in factory.SupportedKinds)
            {
                _factoriesByKind[kind.Trim()] = factory;
            }
        }
    }

    public VoucherDraft Create(VoucherCreationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Kind))
        {
            throw new ArgumentException("Voucher kind is required.", nameof(request));
        }

        if (!_factoriesByKind.TryGetValue(request.Kind.Trim(), out var factory))
        {
            throw new ArgumentException($"Unsupported voucher kind '{request.Kind}'.", nameof(request));
        }

        return factory.CreateDraft(request);
    }
}