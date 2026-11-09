using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP05Singleton;

public sealed record GlobalAccountingRegistry(string Version, int MonetaryScale, string Scope);
public interface IGlobalRegistryChallenge { GlobalAccountingRegistry Acquire(); }
public sealed class GlobalRegistryChallenge : IGlobalRegistryChallenge
{
    public GlobalAccountingRegistry Acquire()
    {
        // TODO(STUDENT): DP-05
        throw new PatternPendingException("DP-05");
    }
}

