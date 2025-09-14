namespace Buenaventura.MCP.Tools;

using Dapper;
using Npgsql;
using ModelContextProtocol.Server;
using System.ComponentModel;
using Buenaventura.MCP.Models;
using Buenaventura.MCP.Services;


[McpServerToolType]
public static class AccountsTool
{
    [
        McpServerTool(Name = "GetAccounts"),
        Description("Get a list of accounts and their ids which are used for querying details of the accounts")
    ]
    public static IEnumerable<Account> GetAccounts(AccountService accountService)
    {
        var accounts = accountService.GetAccounts();
        return accounts;
    }

    [McpServerTool]
    public static string GetAccountInfo(string accountId)
    {
        return "Account info";
    }
}
