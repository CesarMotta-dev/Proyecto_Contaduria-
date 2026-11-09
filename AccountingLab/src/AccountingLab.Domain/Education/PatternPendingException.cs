namespace AccountingLab.Domain.Education;

public sealed class PatternPendingException : NotImplementedException
{
    public string ChallengeId { get; }
    public string PatternName { get; }
    public string ReferenceUrl { get; }

    public PatternPendingException(string challengeId)
        : base($"Falta implementar el patrón académico {ChallengeCatalog.Get(challengeId).Name} ({challengeId}). Consulta {ChallengeCatalog.Get(challengeId).ReferenceUrl}")
    {
        var descriptor = ChallengeCatalog.Get(challengeId);
        ChallengeId = descriptor.Id;
        PatternName = descriptor.Name;
        ReferenceUrl = descriptor.ReferenceUrl;
    }
}
