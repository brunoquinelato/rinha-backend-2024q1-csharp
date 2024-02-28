using System.Text.Json.Serialization;

namespace Endpoints.Transaction;

public sealed record TransactionRequest
{
    [JsonPropertyName("valor")]
    public object? RawAmount { get; set; }
    public int Amount { get; set; }

    [JsonPropertyName("tipo")]
    public char Type { get; set; }

    [JsonPropertyName("descricao")]
    public string Description { get; set; } = null!;

    public bool IsValid()
    {
        var validAmount = int.TryParse(RawAmount?.ToString(), out int convertedAmount);
        Amount = convertedAmount;

        return
            (Type == 'c' || Type == 'd') &&
            validAmount &&
            Description?.Trim()?.Length >= 1 &&
            Description?.Trim()?.Length <= 10;
    }
}
