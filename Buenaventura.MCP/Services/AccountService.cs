namespace Buenaventura.MCP.Services;

using Microsoft.Extensions.Options;
using Buenaventura.MCP.Configuration;
using Buenaventura.MCP.Models;
using Npgsql;
using Dapper;

public class AccountService
{
    private readonly string _connectionString;
    public AccountService(IOptions<DatabaseConfiguration> dbConfig)
    {
        _connectionString = dbConfig.Value.Buenaventura;
        Console.WriteLine($"Connection string: {_connectionString}");
    }

    public IEnumerable<Account> GetAccounts()
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            var accounts = connection.Query<Account>("SELECT account_id AS AccountId, name AS Name, currency AS Currency, vendor AS Vendor, account_type AS AccountType FROM accounts");
            return accounts;
        }
    }
}