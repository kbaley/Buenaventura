namespace Buenaventura.MCP.Tools;

using System.ComponentModel;
using Buenaventura.MCP.Models;
using Buenaventura.MCP.Services;
using Dapper;
using ModelContextProtocol.Server;
using Npgsql;

[McpServerToolType]
public static class AccountsTool
{
    [
        McpServerTool(Name = "GetAccounts"),
        Description(
            "Get a list of accounts and their ids which are used for querying details of the accounts"
        )
    ]
    public static IEnumerable<AccountName> GetAccounts(AccountService accountService)
    {
        var accounts = accountService.GetAccounts();
        return accounts.Select(x => new AccountName { AccountId = x.AccountId, Name = x.Name });
    }

    [
        McpServerTool(Name = "GetAccountDetails"),
        Description("Get details for a specific account by id")
    ]
    public static Account GetAccountInfo(Guid accountId, AccountService accountService)
    {
        var account = accountService.GetAccounts().Where(x => x.AccountId == accountId);
        return account.First();
    }

    [
        McpServerTool(Name = "GetAccountBalance"),
        Description("Get the balance for a specific account by id")
    ]
    public static AccountBalance GetAccountBalance(
        Guid accountId,
        AccountService accountService
    )
    {
        var balance = accountService.GetAccountBalance(accountId);
        return balance;
    }
}
