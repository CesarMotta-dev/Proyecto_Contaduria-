using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using AccountingLab.Domain.Education;

namespace AccountingLab.Infrastructure.Challenges.DP06Adapter;

public sealed record BankImportRequest(string Provider, string Payload);
public sealed record ImportedBankMovement(string ExternalId, DateOnly Date, string Description, decimal Debit, decimal Credit, string Currency);
public interface IBankImportChallenge { IReadOnlyList<ImportedBankMovement> Import(BankImportRequest request); }

public interface IBankPayloadAdapter
{
    bool CanHandle(string provider);
    IReadOnlyList<ImportedBankMovement> Adapt(string payload);
}

internal sealed class FlexibleDecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetDecimal(),
            JsonTokenType.String => ParseDecimalString(reader.GetString()),
            JsonTokenType.Null => 0m,
            _ => throw new FormatException($"Cannot convert token '{reader.TokenType}' to decimal.")
        };
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value);

    private static decimal ParseDecimalString(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return 0m;
        }

        return decimal.Parse(raw.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture);
    }
}

internal static class JsonMovementHelper
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new FlexibleDecimalConverter());
        return options;
    }

    public static IReadOnlyList<T> ReadJsonItems<T>(string payload)
    {
        try
        {
            var json = payload.Trim();

            using (var initialDoc = JsonDocument.Parse(json))
            {
                if (initialDoc.RootElement.ValueKind == JsonValueKind.String)
                {
                    json = initialDoc.RootElement.GetString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        throw new FormatException("Payload JSON string is empty.");
                    }
                }
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                return JsonSerializer.Deserialize<List<T>>(json, Options)
                    ?? throw new FormatException("Malformed JSON array.");
            }

            if (root.ValueKind == JsonValueKind.Object)
            {
                var single = JsonSerializer.Deserialize<T>(json, Options)
                    ?? throw new FormatException("Malformed JSON object.");
                return [single];
            }

            throw new FormatException($"Unexpected JSON root kind '{root.ValueKind}'.");
        }
        catch (FormatException)
        {
            throw;
        }
        catch (JsonException ex)
        {
            throw new FormatException("Malformed JSON payload.", ex);
        }
    }

    public static DateOnly ParseDate(string rawDate)
    {
        var clean = rawDate.Trim();
        if (DateOnly.TryParse(clean, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
        {
            return d;
        }

        if (DateTimeOffset.TryParse(clean, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dto))
        {
            return DateOnly.FromDateTime(dto.UtcDateTime);
        }

        return DateOnly.FromDateTime(DateTime.Parse(clean, CultureInfo.InvariantCulture));
    }
}

public sealed record BancoAItemDto(
    string id,
    string date,
    string description,
    [property: JsonConverter(typeof(FlexibleDecimalConverter))] decimal debit,
    [property: JsonConverter(typeof(FlexibleDecimalConverter))] decimal credit,
    string currency);

public sealed class BankAAdapter : IBankPayloadAdapter
{
    public bool CanHandle(string provider) =>
        string.Equals(provider, "BancoA", StringComparison.OrdinalIgnoreCase);

    public IReadOnlyList<ImportedBankMovement> Adapt(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        return JsonMovementHelper
            .ReadJsonItems<BancoAItemDto>(payload)
            .Select(item => new ImportedBankMovement(
                ExternalId: item.id,
                Date: JsonMovementHelper.ParseDate(item.date),
                Description: item.description,
                Debit: item.debit,
                Credit: item.credit,
                Currency: item.currency))
            .ToList()
            .AsReadOnly();
    }
}

public sealed record BancoBItemDto(
    string reference,
    string bookedOn,
    string concept,
    [property: JsonConverter(typeof(FlexibleDecimalConverter))] decimal signedAmount,
    string isoCurrency);

public sealed class BankBAdapter : IBankPayloadAdapter
{
    public bool CanHandle(string provider) =>
        string.Equals(provider, "BancoB", StringComparison.OrdinalIgnoreCase);

    public IReadOnlyList<ImportedBankMovement> Adapt(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        return JsonMovementHelper
            .ReadJsonItems<BancoBItemDto>(payload)
            .Select(item => new ImportedBankMovement(
                ExternalId: item.reference,
                Date: JsonMovementHelper.ParseDate(item.bookedOn),
                Description: item.concept,
                Debit: item.signedAmount < 0m ? Math.Abs(item.signedAmount) : 0m,
                Credit: item.signedAmount > 0m ? item.signedAmount : 0m,
                Currency: item.isoCurrency))
            .ToList()
            .AsReadOnly();
    }
}

public sealed class BankImportChallenge : IBankImportChallenge
{
    private readonly IReadOnlyList<IBankPayloadAdapter> _adapters;

    public BankImportChallenge()
        : this([new BankAAdapter(), new BankBAdapter()])
    {
    }

    public BankImportChallenge(IEnumerable<IBankPayloadAdapter> adapters)
    {
        _adapters = adapters?.ToList() ?? throw new ArgumentNullException(nameof(adapters));
    }

    public IReadOnlyList<ImportedBankMovement> Import(BankImportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var adapter = _adapters.FirstOrDefault(a => a.CanHandle(request.Provider))
            ?? throw new NotSupportedException($"Unsupported bank provider '{request.Provider}'.");

        return adapter.Adapt(request.Payload);
    }
}