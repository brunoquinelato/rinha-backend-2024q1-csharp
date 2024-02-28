using Entities;
using Npgsql;

namespace Repositories;

public class TransactionRepository
{    
    private readonly IConfiguration _configuration;

    public TransactionRepository(
        IConfiguration configuration)
    {
        _configuration = configuration;        
    }

    public async Task<bool> InsertAsync(Transaction transaction)
    {
        using var _dbConnection = new NpgsqlConnection(_configuration.GetConnectionString("DbConnection"));
        await _dbConnection.OpenAsync();
        var command = new NpgsqlCommand
        {
            Connection = _dbConnection,
            CommandText = @"
                INSERT INTO transactions 
                    (
                        customer_id,                    
                        type, 
                        amount, 
                        description, 
                        created_at
                    )
                VALUES
                    (
                        @customer_id,
                        @type,
                        @amount,
                        @description,
                        @created_at
                    )"
        };

        command.Parameters.AddWithValue("@customer_id", transaction.CustomerId);
        command.Parameters.AddWithValue("@type", transaction.Type);
        command.Parameters.AddWithValue("@amount", transaction.Amount);
        command.Parameters.AddWithValue("@description", transaction.Description);
        command.Parameters.AddWithValue("@created_at", transaction.CreatedAt);
        
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<ICollection<Transaction>?> GetLastTransactionsByCustomerId(int customerId)
    {
        using var _dbConnection = new NpgsqlConnection(_configuration.GetConnectionString("DbConnection"));
        await _dbConnection.OpenAsync();
        var command = new NpgsqlCommand
        {
            Connection = _dbConnection,
            CommandText = @"
                SELECT 
                    type, 
                    amount, 
                    description, 
                    created_at
                FROM 
                    transactions 
                WHERE customer_id = @customer_id
                ORDER BY created_at desc
                LIMIT 10"
        };
        command.Parameters.AddWithValue("@customer_id", customerId);

        var transactions = new List<Transaction>();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            transactions.Add(new Transaction
            (
                customerId,
                reader.GetInt32(1),
                reader.GetChar(0),
                reader.GetString(2),
                reader.GetDateTime(3)
            ));
        }

        return transactions;
    }
}