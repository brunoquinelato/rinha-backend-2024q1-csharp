namespace Endpoints.Transaction;

public static class TransactionEndpoints
{
    public static void RegisterTransactionEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("clientes/{customerId:int}/transacoes", async (int customerId, TransactionRequest request, TransactionHandler handler) =>
        {
            return await handler.HandleAsync(customerId, request);
        });
    }
}