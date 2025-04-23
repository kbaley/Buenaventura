using System.Data;
using Npgsql;

namespace Buenaventura.Data;

public abstract class BaseRepository
{
    private readonly string _connectionString;

    protected BaseRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Buenaventura") ?? "";
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    internal IDbConnection Connection => new NpgsqlConnection(_connectionString);
}