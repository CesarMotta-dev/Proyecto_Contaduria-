using Xunit;
using AccountingLab.Domain.Challenges.DP11Flyweight;

namespace AccountingLab.PatternChallenges.DP11Flyweight;

[Trait("ChallengeId", "DP-11")]
public sealed class DP11FlyweightTests
{
    [Fact]
    public void Reuses_metadata_identity_for_equal_classifications()
    {
        var input = new[] { new MovementClassificationInput(Guid.NewGuid(), 1m, "VENTA", "Venta demo", "DEMO-A"), new MovementClassificationInput(Guid.NewGuid(), 2m, "VENTA", "Venta demo", "DEMO-A") };
        var result = new SharedClassificationChallenge().Prepare(input);
        Assert.Equal(2, result.Count);
        Assert.Same(result[0].Metadata, result[1].Metadata);
    }
    [Fact]
    public void Distinct_classification_values_are_not_accidentally_shared()
    {
        var input = new[] { new MovementClassificationInput(Guid.NewGuid(), 1m, "VENTA", "Venta demo", "DEMO-A"), new MovementClassificationInput(Guid.NewGuid(), 1m, "VENTA", "Venta demo", "DEMO-B") };
        var result = new SharedClassificationChallenge().Prepare(input);
        Assert.NotSame(result[0].Metadata, result[1].Metadata);
        Assert.Equal("DEMO-A", result[0].Metadata.FictionalTaxScope);
        Assert.Equal("DEMO-B", result[1].Metadata.FictionalTaxScope);
    }
    [Fact]
    public void Shared_metadata_does_not_mix_per_movement_identity_or_amount()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var result = new SharedClassificationChallenge().Prepare(new[] { new MovementClassificationInput(first, 0m, "VENTA", "Venta demo", "DEMO-A"), new MovementClassificationInput(second, 999.0001m, "VENTA", "Venta demo", "DEMO-A") });
        Assert.Equal(first, result[0].Id);
        Assert.Equal(second, result[1].Id);
        Assert.Equal(0m, result[0].Amount);
        Assert.Equal(999.0001m, result[1].Amount);
    }
}

