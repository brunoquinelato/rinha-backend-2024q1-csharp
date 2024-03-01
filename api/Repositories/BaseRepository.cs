using System.Data;
using Npgsql;

namespace Repositories;

public abstract class BaseRepository
{
    protected readonly NpgsqlConnection _dbConnection;

    public BaseRepository(NpgsqlConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task OpenConnectionAsync()
    {
        if (_dbConnection.State != ConnectionState.Open)
            await _dbConnection.OpenAsync();
    }
}
