using Repositories;

namespace Endpoints.Balance;

public class BalanceHandler
{
    private readonly CustomerRepository _customerRepository;
    private readonly TransactionRepository _transactionRepository;

    public BalanceHandler(
        CustomerRepository customerRepository,
        TransactionRepository transactionRepository)
    {
        _customerRepository = customerRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<IResult> HandleAsync(int customerId)
    {
        if (!await _customerRepository.CustomerExistsByIdAsync(customerId))
            return Results.NotFound("customer not found.");
        
        var customer = await _customerRepository.GetCustomerFromCacheAsync(customerId);

        var transactions = await _transactionRepository
            .GetLastTransactionsByCustomerId(customerId);

        return Results.Ok(new
        {
            saldo = new
            {
                total = transactions?.FirstOrDefault()?.CurrentBalance ?? customer.Balance,
                data_extrato = DateTime.UtcNow,
                limite = customer?.Limit
            },
            ultimas_transacoes = transactions?.Select(p => new
            {
                valor = p.Amount,
                tipo = p.Type,
                descricao = p.Description,
                realizada_em = p.CreatedAt
            })
        });
    }
}
