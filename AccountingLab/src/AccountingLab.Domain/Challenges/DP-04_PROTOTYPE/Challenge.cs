using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP04Prototype;

public sealed class TemplateLine
{
    public string AccountCode { get; set; } = "";
    public decimal Amount { get; set; }
    public List<string> Tags { get; set; } = new();
}
public sealed class RecurringTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public List<TemplateLine> Lines { get; set; } = new();
}
public interface ITemplateCopyChallenge { RecurringTemplate Copy(RecurringTemplate source); }
public sealed class TemplateCopyChallenge : ITemplateCopyChallenge
{
    public RecurringTemplate Copy(RecurringTemplate source)
    {
        // TODO(STUDENT): DP-04
        throw new PatternPendingException("DP-04");
    }
}

