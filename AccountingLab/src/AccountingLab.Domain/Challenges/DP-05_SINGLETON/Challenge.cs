using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP05Singleton;

public sealed record GlobalAccountingRegistry(string Version, int MonetaryScale, string Scope);
public interface IGlobalRegistryChallenge { GlobalAccountingRegistry Acquire(); }
public sealed class GlobalRegistryChallenge : IGlobalRegistryChallenge
{
    private static readonly GlobalAccountingRegistry _instance = new("DEMO-1", 4, "process");

    public GlobalAccountingRegistry Acquire() => _instance;
}