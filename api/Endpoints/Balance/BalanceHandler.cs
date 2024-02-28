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
        var customer = await _customerRepository.GetCustomerFromCacheAsync(customerId);
        if (customer is null)
            return Results.NotFound("customer not found.");
        
        var balance = await _customerRepository.GetCustomerBalanceByIdAsync(customerId);
        var transactions = await _transactionRepository
            .GetLastTransactionsByCustomerId(customerId);

        return Results.Ok(new BalanceResponse
        {
            Balance = new BalanceAmountResponse
            {
                Balance = balance ?? default,
                BalanceDate = DateTime.UtcNow,
                LimitAmount = customer!.Limit
            },
            LastTransactions = transactions?.Select(p => new BalanceTransactionResponse
            {
                Amount = p.Amount,
                Type = p.Type,
                Description = p.Description,
                CreatedAt = p.CreatedAt
            })
        });
    }
}
