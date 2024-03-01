using Entities;
using Npgsql;

namespace Repositories;

public class TransactionRepository(NpgsqlConnection dbConnection) : BaseRepository(dbConnection)
{
    private static readonly NpgsqlCommand InsertTransactionCommand = new()
    {
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
                )",
        Parameters = 
        {
            new NpgsqlParameter("@customer_id", NpgsqlTypes.NpgsqlDbType.Integer),
            new NpgsqlParameter("@type", NpgsqlTypes.NpgsqlDbType.Char),
            new NpgsqlParameter("@amount", NpgsqlTypes.NpgsqlDbType.Integer),
            new NpgsqlParameter("@description", NpgsqlTypes.NpgsqlDbType.Varchar),
            new NpgsqlParameter("@created_at", NpgsqlTypes.NpgsqlDbType.TimestampTz)
        }
    };

    private static readonly NpgsqlCommand GetLastTransactionsCommand = new()
    {
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
            LIMIT 10",
        Parameters = 
        {
            new NpgsqlParameter("@customer_id", NpgsqlTypes.NpgsqlDbType.Integer)
        }
    };

    public async Task<bool> InsertAsync(Transaction transaction)
    {
        await OpenConnectionAsync();
        var command = InsertTransactionCommand.Clone();
        command.Connection = _dbConnection;
        command.Parameters["@customer_id"].Value = transaction.CustomerId;
        command.Parameters["@type"].Value = transaction.Type;
        command.Parameters["@amount"].Value = transaction.Amount;
        command.Parameters["@description"].Value = transaction.Description;
        command.Parameters["@created_at"].Value = transaction.CreatedAt;

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<ICollection<Transaction>?> GetLastTransactionsByCustomerId(int customerId)
    {
        await OpenConnectionAsync();
        var command = GetLastTransactionsCommand.Clone();
        command.Connection = _dbConnection;
        command.Parameters["@customer_id"].Value = customerId;

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