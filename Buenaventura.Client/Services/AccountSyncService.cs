namespace Buenaventura.Client.Services;

public class AccountSyncService
{
    public event Func<Task>? OnAccountsUpdated;

    public async Task RefreshAccounts()
    {
        if (OnAccountsUpdated is not null)
        {
            await OnAccountsUpdated();
        }
    }
}