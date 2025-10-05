namespace Buenaventura.MCP.Tools;

using System.ComponentModel;
using Buenaventura.MCP.Models;
using Buenaventura.MCP.Services;
using Dapper;
using ModelContextProtocol.Server;
using Npgsql;

[McpServerToolType]
public static class TransactionTool
{
    [
        McpServerTool(Name = "GetTransactions"),
        Description("Get a list of transactions for a specific account")
    ]
    public static IEnumerable<Transaction> GetTransactions(DateTime startDate, DateTime endDate, Guid accountId, TransactionService transactionService)
    {
        var transactions = transactionService.GetTransactions(startDate, endDate, accountId);
        return transactions;
    }

   

    [
        McpServerTool(Name = "AddTransaction"),
        Description("Add a new transaction")
    ]
    public static void AddTransaction(Transaction transaction, TransactionService transactionService)
    {
        transactionService.AddTransaction(transaction);
    }
}

