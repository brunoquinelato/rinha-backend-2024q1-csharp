namespace Entities;


public class Transaction
{
    public int CustomerId { get; }
    public int Amount { get; }
    public char Type { get; }
    public string Description { get; }
    public DateTime CreatedAt { get; private set; }
    public int CurrentBalance { get; set; }

    public Transaction(int customerId, int amount, char type, string description, DateTime createdAt)
    {
        CustomerId = customerId;
        Amount = amount;
        Type = type;
        Description = description;
        CreatedAt = createdAt;
    }

    public Transaction(int customerId, int amount, char type, string description, DateTime createdAt, int currentBalance)
    {
        CustomerId = customerId;
        Amount = amount;
        Type = type;
        Description = description;
        CreatedAt = createdAt;
        CurrentBalance = currentBalance;
    }

    public bool IsDepositTransaction()
    {
        return Type == 'd';
    }

    public int GetDepositAmount()
    {
        return Amount * -1;
    }
};