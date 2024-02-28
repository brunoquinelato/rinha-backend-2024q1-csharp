using System.Text.Json.Serialization;

namespace Endpoints.Transaction;

public record TransactionResponse
{
    [JsonPropertyName("limite")]
    public int LimitAmount { get; set; }
    [JsonPropertyName("saldo")]
    public int Balance { get; set; }
}
