using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP02AbstractFactory;

public sealed record AccountingProfileRequest(string Profile, string FictionalRegime);
public sealed record AccountingFamilyMember(string Role, string CompatibilityKey);
public sealed record AccountingServiceFamily(string Profile, string FictionalRegime, IReadOnlyList<AccountingFamilyMember> Members);
public interface IAccountingProfileChallenge { AccountingServiceFamily CreateFamily(AccountingProfileRequest request); }
public sealed class AccountingProfileChallenge : IAccountingProfileChallenge
{
    public AccountingServiceFamily CreateFamily(AccountingProfileRequest request)
    {
        // TODO(STUDENT): DP-02
        throw new PatternPendingException("DP-02");
    }
}

