using System.Text.Json.Serialization;

namespace Endpoints.Balance;

public record BalanceResponse
{
    [JsonPropertyName("saldo")]
    public BalanceAmountResponse Balance { get; set; } = null!;
    [JsonPropertyName("ultimas_transacoes")]
    public IEnumerable<BalanceTransactionResponse>? LastTransactions { get; set; } = null!;
}

public record BalanceTransactionResponse
{
    [JsonPropertyName("valor")]
    public int Amount { get; set; }
    [JsonPropertyName("tipo")]
    public char Type { get; set; }
    [JsonPropertyName("descricao")]
    public string Description { get; set; }
    [JsonPropertyName("realizada_em")]
    public DateTime CreatedAt { get; set; }
}

public record BalanceAmountResponse
{
    [JsonPropertyName("limite")]
    public int LimitAmount { get; set; }
    [JsonPropertyName("total")]
    public int Balance { get; set; }
    [JsonPropertyName("data_extrato")]
    public DateTime BalanceDate { get; set; }
}
