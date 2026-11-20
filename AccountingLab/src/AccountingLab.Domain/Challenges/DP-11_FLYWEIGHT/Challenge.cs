using AccountingLab.Domain.Education;

namespace AccountingLab.Domain.Challenges.DP11Flyweight;

public sealed record MovementClassificationInput(Guid Id, decimal Amount, string Code, string Label, string FictionalTaxScope);
public sealed record ClassificationMetadata(string Code, string Label, string FictionalTaxScope);
public sealed record ClassifiedMovement(Guid Id, decimal Amount, ClassificationMetadata Metadata);
public interface ISharedClassificationChallenge { IReadOnlyList<ClassifiedMovement> Prepare(IReadOnlyList<MovementClassificationInput> movements); }

internal sealed class ClassificationMetadataPool
{
    private readonly Dictionary<(string Code, string Label, string FictionalTaxScope), ClassificationMetadata> _pool = new();

    public ClassificationMetadata GetOrCreate(string code, string label, string fictionalTaxScope)
    {
        var key = (
            Code: code ?? string.Empty,
            Label: label ?? string.Empty,
            FictionalTaxScope: fictionalTaxScope ?? string.Empty);

        if (!_pool.TryGetValue(key, out var metadata))
        {
            metadata = new ClassificationMetadata(key.Code, key.Label, key.FictionalTaxScope);
            _pool[key] = metadata;
        }

        return metadata;
    }
}

public sealed class SharedClassificationChallenge : ISharedClassificationChallenge
{
    private readonly ClassificationMetadataPool _pool = new();

    public IReadOnlyList<ClassifiedMovement> Prepare(IReadOnlyList<MovementClassificationInput> movements)
    {
        ArgumentNullException.ThrowIfNull(movements);

        var result = new List<ClassifiedMovement>(movements.Count);

        foreach (var item in movements)
        {
            ArgumentNullException.ThrowIfNull(item);

            var metadata = _pool.GetOrCreate(item.Code, item.Label, item.FictionalTaxScope);
            result.Add(new ClassifiedMovement(item.Id, item.Amount, metadata));
        }

        return result.AsReadOnly();
    }
}
