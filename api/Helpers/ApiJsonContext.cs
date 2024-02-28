using System.Text.Json.Serialization;
using Endpoints.Transaction;
using Endpoints.Balance;

namespace Helpers;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(TransactionRequest))]
[JsonSerializable(typeof(TransactionResponse))]
[JsonSerializable(typeof(BalanceResponse))]
[JsonSerializable(typeof(BalanceAmountResponse))]
[JsonSerializable(typeof(BalanceTransactionResponse))]
[JsonSerializable(typeof(IEnumerable<BalanceTransactionResponse>))]
internal partial class ApiJsonContext : JsonSerializerContext
{
}