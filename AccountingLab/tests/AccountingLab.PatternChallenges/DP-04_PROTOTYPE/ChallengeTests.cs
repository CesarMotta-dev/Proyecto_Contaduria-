using Xunit;
using AccountingLab.Domain.Challenges.DP04Prototype;

namespace AccountingLab.PatternChallenges.DP04Prototype;

[Trait("ChallengeId", "DP-04")]
public sealed class DP04PrototypeTests
{
    private static RecurringTemplate Source() => new()
    {
        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Arriendo",
        Lines = new() { new() { AccountCode = "5105", Amount = 450m, Tags = new() { "mensual" } } }
    };
    [Fact]
    public void Copy_preserves_values_with_independent_identity_and_lines()
    {
        var source = Source();
        var copy = new TemplateCopyChallenge().Copy(source);
        Assert.NotSame(source, copy);
        Assert.NotEqual(source.Id, copy.Id);
        Assert.Equal(source.Name, copy.Name);
        Assert.Equal(source.Lines[0].Amount, copy.Lines[0].Amount);
        copy.Lines[0].Amount = 900m;
        Assert.Equal(450m, source.Lines[0].Amount);
    }
    [Fact]
    public void Rejects_null_source_template()
    {
        Assert.Throws<ArgumentNullException>(() => new TemplateCopyChallenge().Copy(null!));
    }
    [Fact]
    public void Nested_mutable_tags_are_independent_of_source()
    {
        var source = Source();
        var copy = new TemplateCopyChallenge().Copy(source);
        copy.Lines[0].Tags.Add("urgente");
        copy.Lines.Add(new() { AccountCode = "1110", Amount = 1m });
        Assert.Equal(new[] { "mensual" }, source.Lines[0].Tags);
        Assert.Single(source.Lines);
    }
}

