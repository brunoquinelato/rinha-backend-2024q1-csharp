using Entities;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;

namespace Repositories;

public class CustomerRepository(
    NpgsqlConnection dbConnection,
    IMemoryCache customersCache) : BaseRepository(dbConnection)
{
    private static readonly NpgsqlCommand UpdateBalanceCommand = new()
    {
        CommandText = @"
            UPDATE 
                customers
            SET
                balance = balance + @transaction_amount            
            WHERE 
                id = @customer_id AND 
                limit_amount >= (balance + @transaction_amount) * -1
            RETURNING balance as new_balance ",
        Parameters =
        {
            new NpgsqlParameter("@customer_id", NpgsqlTypes.NpgsqlDbType.Integer),
            new NpgsqlParameter("@transaction_amount", NpgsqlTypes.NpgsqlDbType.Integer)
        }
    };

    private static readonly NpgsqlCommand GetBalanceCommand = new()
    {
        CommandText = @"
            SELECT 
                balance 
            FROM customers WHERE id = @customer_id",
        Parameters =
        {
            new NpgsqlParameter("@customer_id", NpgsqlTypes.NpgsqlDbType.Integer)
        }
    };

    private readonly IMemoryCache _customersCache = customersCache;

    public async Task<Customer?> GetCustomerFromCacheAsync(int customerId)
    {
        if (!_customersCache.TryGetValue("customersCache", out ICollection<Customer>? customers))
        {
            customers = await GetAllCustomersAsync();
            _customersCache.Set("customersCache", customers, TimeSpan.FromHours(24));
        }

        return customers!.SingleOrDefault(c => c.Id == customerId);
    }

    private async Task<ICollection<Customer>> GetAllCustomersAsync()
    {
        await OpenConnectionAsync();
        var command = new NpgsqlCommand
        {
            Connection = _dbConnection,
            CommandText = @"
                SELECT 
                    id,
                    limit_amount
                FROM customers"
        };

        var customers = new List<Customer>();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            customers.Add(new Customer(
            reader.GetInt32(0),
            reader.GetInt32(1)));

        return customers;
    }

    public async Task<int?> GetCustomerBalanceByIdAsync(int customerId)
    {
        await OpenConnectionAsync();
        var command = GetBalanceCommand.Clone();
        command.Connection = _dbConnection;
        command.Parameters["@customer_id"].Value = customerId;

        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return reader.GetInt32(0);
    }

    public async Task<int?> UpdateBalanceAsync(int customerId, int amount)
    {
        await OpenConnectionAsync();
        var command = UpdateBalanceCommand.Clone();
        command.Connection = _dbConnection;
        command.Parameters["@customer_id"].Value = customerId;
        command.Parameters["@transaction_amount"].Value = amount;

        return (int?)await command.ExecuteScalarAsync();
    }
}