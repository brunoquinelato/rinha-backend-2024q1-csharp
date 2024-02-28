using Repositories;

namespace Endpoints.Transaction;
public class TransactionHandler
{
    private readonly CustomerRepository _customerRepository;
    private readonly TransactionRepository _transactionRepository;

    public TransactionHandler(
        CustomerRepository customerRepository,
        TransactionRepository transactionRepository)
    {
        _customerRepository = customerRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<IResult> HandleAsync(int customerId, TransactionRequest request)
    {
        if (!request.IsValid())
            return Results.UnprocessableEntity("invalid parameters.");

        var customer = await _customerRepository.GetCustomerFromCacheAsync(customerId);
        if (customer is null)
            return Results.NotFound("customer not found.");

        var transaction = new Entities.Transaction(
            customerId,
            request.Amount,
            request.Type,
            request.Description,
            DateTime.UtcNow);

        var updatedBalance = await _customerRepository
            .UpdateBalanceAsync(
                customerId,
                transaction.IsDepositTransaction() ? transaction.GetDepositAmount() : transaction.Amount);

        if (!updatedBalance.HasValue)
            return Results.UnprocessableEntity("inconsistence balance.");

        _ = await _transactionRepository.InsertAsync(transaction);
        return Results.Ok(new TransactionResponse
        {
            LimitAmount = customer.Limit,
            Balance = updatedBalance.Value
        });
    }
}