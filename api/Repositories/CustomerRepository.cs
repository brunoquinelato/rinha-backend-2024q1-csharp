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

    public async Task<Customer> GetCustomerFromCacheAsync(int customerId)
    {
        if (!_customersCache.TryGetValue("customersCache", out ICollection<Customer>? customers))
        {
            customers = await GetAllCustomersAsync();
            _customersCache.Set("customersCache", customers, TimeSpan.FromHours(24));
        }

        return customers!.Single(c => c.Id == customerId);
    }

    public async Task<bool> CustomerExistsByIdAsync(int customerId)
    {
        if (!_customersCache.TryGetValue("customersCache", out ICollection<Customer>? customers))
        {
            customers = await GetAllCustomersAsync();
            _customersCache.Set("customersCache", customers, TimeSpan.FromHours(24));
        }

        return customers is not null && customers.Any(c => c.Id == customerId);
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

    public async Task<Customer?> GetCustomerByIdAsync(int customerId)
    {
        using var _dbConnection = new NpgsqlConnection(_configuration.GetConnectionString("DbConnection"));
        await _dbConnection.OpenAsync();
        var command = new NpgsqlCommand
        {
            Connection = _dbConnection,
            CommandText = @"
                SELECT 
                    id,
                    limit_amount, 
                    balance, 
                    modified_at 
                FROM customers WHERE Id = @Id"
        };
        command.Parameters.AddWithValue("@Id", customerId);

        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return new Customer(
            reader.GetInt32(0),
            reader.GetInt32(1),
            reader.GetInt32(2),
            reader.GetDateTime(3),
            reader.GetInt32(2));
    }

    public async Task<bool> UpdateAsync(Customer customer)
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
                    balance = @balance, 
                    modified_at = @new_modified_at                
                WHERE 
                    Id = @id AND 
                    modified_at = @modified_at "
        };
        command.Parameters.AddWithValue("@id", customer.Id);
        command.Parameters.AddWithValue("@balance", customer.Balance);
        command.Parameters.AddWithValue("@new_modified_at", DateTime.UtcNow);
        command.Parameters.AddWithValue("@modified_at", customer.ModifiedAt);
        // command.Parameters.AddWithValue("@original_balance", customer.OriginalBalance);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected != 0;
    }
}