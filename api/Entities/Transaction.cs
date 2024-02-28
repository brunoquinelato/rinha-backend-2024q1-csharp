namespace Entities;

public record Transaction(int CustomerId, int Amount, char Type, string Description, DateTime CreatedAt)
{
    public bool IsDepositTransaction()
    {
        return Type == 'd';
    }

    public int GetDepositAmount()
    {
        return Amount * -1;
    }
};