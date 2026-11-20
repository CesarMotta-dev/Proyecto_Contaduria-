using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP02AbstractFactory;

public sealed record AccountingProfileRequest(string Profile, string FictionalRegime);
public sealed record AccountingFamilyMember(string Role, string CompatibilityKey);
public sealed record AccountingServiceFamily(string Profile, string FictionalRegime, IReadOnlyList<AccountingFamilyMember> Members);
public interface IAccountingProfileChallenge { AccountingServiceFamily CreateFamily(AccountingProfileRequest request); }

public interface IAccountingProfileFamilyFactory
{
    string ProfileName { get; }
    AccountingServiceFamily Create(string profile, string fictionalRegime);
}

public sealed class CommercialProfileFamilyFactory : IAccountingProfileFamilyFactory
{
    public string ProfileName => "comercial";

    public AccountingServiceFamily Create(string profile, string fictionalRegime)
    {
        var regime = fictionalRegime.Trim().ToLowerInvariant();
        var compatibilityKey = $"com-{regime}-v1";

        IReadOnlyList<AccountingFamilyMember> members =
        [
            new AccountingFamilyMember("calculo", compatibilityKey),
            new AccountingFamilyMember("validacion", compatibilityKey),
            new AccountingFamilyMember("reporte", compatibilityKey)
        ];

        return new AccountingServiceFamily(profile, fictionalRegime, members);
    }
}

public sealed class ServicesProfileFamilyFactory : IAccountingProfileFamilyFactory
{
    public string ProfileName => "servicios";

    public AccountingServiceFamily Create(string profile, string fictionalRegime)
    {
        var regime = fictionalRegime.Trim().ToLowerInvariant();
        var compatibilityKey = $"srv-{regime}-v1";

        IReadOnlyList<AccountingFamilyMember> members =
        [
            new AccountingFamilyMember("calculo", compatibilityKey),
            new AccountingFamilyMember("validacion", compatibilityKey),
            new AccountingFamilyMember("reporte", compatibilityKey)
        ];

        return new AccountingServiceFamily(profile, fictionalRegime, members);
    }
}

public sealed class StandardProfileFamilyFactory : IAccountingProfileFamilyFactory
{
    public string ProfileName => "standard";

    public AccountingServiceFamily Create(string profile, string fictionalRegime)
    {
        var regime = fictionalRegime.Trim().ToLowerInvariant();
        var compatibilityKey = $"std-{regime}-v1";

        IReadOnlyList<AccountingFamilyMember> members =
        [
            new AccountingFamilyMember("calculo", compatibilityKey),
            new AccountingFamilyMember("validacion", compatibilityKey),
            new AccountingFamilyMember("reporte", compatibilityKey)
        ];

        return new AccountingServiceFamily(profile, fictionalRegime, members);
    }
}

public sealed class AuditProfileFamilyFactory : IAccountingProfileFamilyFactory
{
    public string ProfileName => "audit";

    public AccountingServiceFamily Create(string profile, string fictionalRegime)
    {
        var regime = fictionalRegime.Trim().ToLowerInvariant();
        var compatibilityKey = $"aud-{regime}-v1";

        IReadOnlyList<AccountingFamilyMember> members =
        [
            new AccountingFamilyMember("calculo", compatibilityKey),
            new AccountingFamilyMember("validacion", compatibilityKey),
            new AccountingFamilyMember("reporte", compatibilityKey)
        ];

        return new AccountingServiceFamily(profile, fictionalRegime, members);
    }
}

public sealed class AccountingProfileChallenge : IAccountingProfileChallenge
{
    private readonly Dictionary<string, IAccountingProfileFamilyFactory> _factories;

    public AccountingProfileChallenge()
        : this(
        [
            new CommercialProfileFamilyFactory(),
            new ServicesProfileFamilyFactory(),
            new StandardProfileFamilyFactory(),
            new AuditProfileFamilyFactory()
        ])
    {
    }

    public AccountingProfileChallenge(IEnumerable<IAccountingProfileFamilyFactory> factories)
    {
        ArgumentNullException.ThrowIfNull(factories);

        _factories = new Dictionary<string, IAccountingProfileFamilyFactory>(StringComparer.OrdinalIgnoreCase);
        foreach (var factory in factories)
        {
            _factories[factory.ProfileName.Trim()] = factory;
        }
    }

    public AccountingServiceFamily CreateFamily(AccountingProfileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Profile))
        {
            throw new ArgumentException("Profile is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.FictionalRegime))
        {
            throw new ArgumentException("Fictional regime is required.", nameof(request));
        }

        var profileKey = request.Profile.Trim();
        if (!_factories.TryGetValue(profileKey, out var factory))
        {
            throw new ArgumentException($"Unknown company profile '{request.Profile}'.", nameof(request));
        }

        return factory.Create(request.Profile, request.FictionalRegime);
    }
}