using Entities;
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

        if (!await _customerRepository.CustomerExistsByIdAsync(customerId))
            return Results.NotFound("customer not found.");

        var transaction = new Entities.Transaction(
            customerId,
            request.Amount,
            request.Type,
            request.Description,
            DateTime.UtcNow);

        Customer? customer;
        while (true)
        {
            customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (!customer!.HasEnoughBalance(transaction))
                return Results.UnprocessableEntity("inconsistence balance.");

            customer.UpdateBalance(transaction);
            var succeeded = await _customerRepository.UpdateAsync(customer);
            if (succeeded)
                break;
        }

        transaction.CurrentBalance = customer.Balance;
        _ = await _transactionRepository.InsertAsync(transaction);
        return Results.Ok(new
        {
            limite = customer!.Limit,
            saldo = customer!.Balance
        });
    }
}