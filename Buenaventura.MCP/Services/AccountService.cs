namespace Buenaventura.MCP.Services;

using Buenaventura.MCP.Configuration;
using Buenaventura.MCP.Models;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

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
            var accounts = connection.Query<Account>(
                "SELECT account_id AS AccountId, name AS Name, currency AS Currency, vendor AS Vendor, account_type AS AccountType FROM accounts"
            );
            return accounts;
        }
    }

    public AccountBalance GetAccountBalance(Guid accountId)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            var balance = connection.QuerySingleOrDefault<decimal>(
                "SELECT sum(amount) from transactions where account_id = @AccountId",
                new { AccountId = accountId }
            );
            return new AccountBalance { AccountId = accountId, Balance = balance };
        }
    }
}
