using Entities;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;

namespace Repositories;

public class CustomerRepository
{
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _customersCache;

    public CustomerRepository(
        IConfiguration configuration,
        IMemoryCache customersCache)
    {
        _configuration = configuration;        
        _customersCache = customersCache;
    }

    public async Task<Customer?> GetCustomerFromCacheAsync(int customerId)
    {
        if (!_customersCache.TryGetValue("customersCache", out ICollection<Customer>? customers))
        {
            customers = await GetAllCustomersAsync();
            _customersCache.Set("customersCache", customers, TimeSpan.FromHours(24));
        }

        return customers!.SingleOrDefault(c => c.Id == customerId);
    }

    public async Task<ICollection<Customer>> GetAllCustomersAsync()
    {
        using var _dbConnection = new NpgsqlConnection(_configuration.GetConnectionString("DbConnection"));
        await _dbConnection.OpenAsync();
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
        using var _dbConnection = new NpgsqlConnection(_configuration.GetConnectionString("DbConnection"));
        await _dbConnection.OpenAsync();
        var command = new NpgsqlCommand
        {
            Connection = _dbConnection,
            CommandText = @"
                SELECT 
                    balance 
                FROM customers WHERE Id = @Id"
        };
        command.Parameters.AddWithValue("@Id", customerId);

        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return reader.GetInt32(0);
    }

    public async Task<int?> UpdateBalanceAsync(int customerId, int amount)
    {
        using var _dbConnection = new NpgsqlConnection(_configuration.GetConnectionString("DbConnection"));
        await _dbConnection.OpenAsync();

        var command = new NpgsqlCommand
        {
            Connection = _dbConnection,
            CommandText = @"
                UPDATE 
                    customers
                SET
                    balance = balance + @transaction_amount            
                WHERE 
                    id = @customer_id AND 
                    limit_amount >= (balance + @transaction_amount) * -1
                RETURNING balance as new_balance "
        };

        command.Parameters.AddWithValue("@customer_id", customerId);
        command.Parameters.AddWithValue("@transaction_amount", amount);

        return (int?)await command.ExecuteScalarAsync();
    }
}