namespace Buenaventura.MCP.Services;

using Microsoft.Extensions.Options;
using Buenaventura.MCP.Configuration;
using Buenaventura.MCP.Models;
using Npgsql;
using Dapper;

public class TransactionService
{
    private readonly string _connectionString;
    public TransactionService(IOptions<DatabaseConfiguration> dbConfig)
    {
        _connectionString = dbConfig.Value.Buenaventura;
    }

    public IEnumerable<Transaction> GetTransactions(DateTime startDate, DateTime endDate, Guid accountId)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            var transactions = connection.Query<Transaction>("SELECT transaction_id AS TransactionId, account_id AS AccountId, amount AS Amount, transaction_date AS TransactionDate FROM transactions WHERE transaction_date BETWEEN @StartDate AND @EndDate AND account_id = @AccountId",
                new { StartDate = startDate, EndDate = endDate, AccountId = accountId });
            return transactions;
        }
    }

    public void AddTransaction(Transaction transaction)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            var sql = "INSERT INTO transactions (transaction_id, account_id, amount, transaction_date, vendor, description, category_id) VALUES (@TransactionId, @AccountId, @Amount, @TransactionDate, @Vendor, @Description, @CategoryId)";
            connection.Execute(sql, transaction);
        }
    }   
   
}