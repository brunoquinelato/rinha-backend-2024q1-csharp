namespace Endpoints.Balance;

public static class BalanceEndpoints
{
    public static void RegisterBalanceEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("clientes/{customerId:int}/extrato", async (int customerId, BalanceHandler handler) =>
        {
            return await handler.HandleAsync(customerId);
        });
    }
}