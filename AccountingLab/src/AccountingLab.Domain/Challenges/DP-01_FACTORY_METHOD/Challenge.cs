using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP01FactoryMethod;

public sealed record VoucherCreationRequest(Guid CompanyId, string Kind, decimal Amount, string Currency);
public sealed record VoucherDraft(Guid Id, Guid CompanyId, string Kind, decimal Amount, string Currency);
public interface IVoucherCreationChallenge { VoucherDraft Create(VoucherCreationRequest request); }
public sealed class VoucherCreationChallenge : IVoucherCreationChallenge
{
    public VoucherDraft Create(VoucherCreationRequest request)
    {
        // TODO(STUDENT): DP-01
        throw new PatternPendingException("DP-01");
    }
}

