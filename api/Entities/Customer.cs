namespace Entities;

public class Customer
{
    public Customer(
        int id,
        int limit)
    {
        Id = id;
        Limit = limit;
    }

    public Customer(
        int id,
        int limit,
        int balance,
        DateTime modifiedAt,
        long originalBalance)
    {
        Id = id;
        Limit = limit;
        Balance = balance;
        ModifiedAt = modifiedAt;
        OriginalBalance = originalBalance;
    }

    public int Id { get; private set; }
    public int Limit { get; private set; }
    public int Balance { get; private set; }
    public DateTime ModifiedAt { get; private set; }
    public long OriginalBalance { get; }

    public bool HasEnoughBalance(Transaction transaction)
    {
        if (!transaction.IsDepositTransaction())
            return true;

        var updatedBalance = Balance + transaction.GetDepositAmount();
        return Limit >= updatedBalance * -1;
    }

    public void UpdateBalance(Transaction transaction)
    {
        var amount = transaction.IsDepositTransaction() ?
            transaction.GetDepositAmount() :
            transaction.Amount;

        Balance += amount;
    }
}