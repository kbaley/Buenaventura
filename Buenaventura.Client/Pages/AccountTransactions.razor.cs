using Buenaventura.Client.Services;
using Buenaventura.Dtos;
using Buenaventura.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;

namespace Buenaventura.Client.Pages;

public partial class AccountTransactions(
    IAccountService accountService,
    ICategoryService categoryService,
    IVendorService vendorService,
    AccountSyncService accountSyncService,
    IInvoiceService invoiceService,
    IJSRuntime jsRuntime)
{
    [Parameter] public Guid AccountId { get; set; }
    private AccountWithBalance Account { get; set; } = new();
    private TransactionListModel transactions = new();
    private string searchString = string.Empty;
    private Guid previousAccountId;
    private bool loading = true;
    private MudTable<TransactionForDisplay>? transactionTable;
    private TransactionForDisplay? editingTransaction { get; set; }
    private TransactionForDisplay? transactionBackup { get; set; }
    private readonly Variant textVariant = Variant.Text;
    private MudTextField<string>? transactionDateField;
    private List<CategoryDto> categories { get; set; } = [];
    // List of categories not including the accounts for transfers
    private IEnumerable<CategoryDto> masterCategoryList { get; set; } = [];
    private IEnumerable<VendorDto> vendors { get; set; } = [];
    [CascadingParameter] IEnumerable<AccountWithBalance> accounts { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        masterCategoryList = (await categoryService.GetCategories()).ToList();
        var invoices = await invoiceService.GetInvoicesForTransactionCategories();
        foreach (var invoice in invoices)
        {
            masterCategoryList = masterCategoryList.Append(new CategoryDto
            {
                CategoryId = Guid.Empty,
                Name = $"PAYMENT: {invoice.InvoiceNumber} ({invoice.CustomerName} - ${invoice.Balance:N2}",
                InvoiceId = invoice.InvoiceId,
                TransactionType = TransactionType.INVOICE_PAYMENT
            });
        }

        categories = masterCategoryList.ToList();
        vendors = await vendorService.GetVendors();
    }

    private TransactionForDisplay newTransaction { get; set; } = new()
    {
        TransactionDate = DateTime.Today,
        TransactionDateForEdit = DateTime.Today.ToString("MM/dd/yyyy")
    };

    protected override async Task OnParametersSetAsync()
    {
        if (AccountId != previousAccountId)
        {
            loading = true;
            Account = await accountService.GetAccount(AccountId);
            searchString = string.Empty;
            await ReloadTransactions();
            categories = [];
            categories.AddRange(masterCategoryList);
            foreach (var account in accounts.Where(
                         a => a.AccountId != AccountId && a.IsHidden == false))
            {
                categories.Add(new CategoryDto
                {
                    CategoryId = Guid.Empty,
                    Name = $"TRANSFER: {account.Name}",
                    TransactionType = TransactionType.TRANSFER,
                    TransferAccountId = account.AccountId,
                    Type = "Transfer"
                });
            }

            loading = false;
            previousAccountId = AccountId;
        }
    }

    private void BeginEdit(TableRowClickEventArgs<TransactionForDisplay> args)
    {
        var transaction = args.Item;
        if (transaction == null)
        {
            return;
        }

        editingTransaction = transaction;
        editingTransaction.DebitForEdit = transaction.Debit?.ToString("N2") ?? "";
        editingTransaction.CreditForEdit = transaction.Credit?.ToString("N2") ?? "";
        editingTransaction.TransactionDateForEdit = transaction.TransactionDate.ToString("MM/dd/yyyy");
        transactionBackup = new TransactionForDisplay
        {
            TransactionDate = transaction.TransactionDate,
            Vendor = transaction.Vendor,
            CategoryDisplay = transaction.CategoryDisplay,
            Description = transaction.Description,
            Debit = transaction.Debit,
            Credit = transaction.Credit,
            RunningTotal = transaction.RunningTotal,
            TransactionId = transaction.TransactionId,
        };
    }

    private void CancelEdit(TransactionForDisplay transaction)
    {
        if (transactionBackup != null)
        {
            transaction.TransactionDate = transactionBackup.TransactionDate;
            transaction.TransactionDateForEdit = transactionBackup.TransactionDate.ToString("MM/dd/yyyy");
            transaction.Vendor = transactionBackup.Vendor;
            transaction.CategoryDisplay = transactionBackup.CategoryDisplay;
            transaction.Description = transactionBackup.Description;
            transaction.Debit = transactionBackup.Debit;
            transaction.Credit = transactionBackup.Credit;
            transaction.RunningTotal = transactionBackup.RunningTotal;
            transaction.TransactionId = transactionBackup.TransactionId;

            editingTransaction = null;
            transactionBackup = null;
        }
    }

    private async Task OnSearchChanged(string text)
    {
        searchString = text;
        await ReloadTransactions();
    }

    private async Task<TableData<TransactionForDisplay>> ServerReload(TableState state, CancellationToken token)
    {
        transactions = await accountService.GetTransactions(AccountId, searchString, state.Page, state.PageSize);
        return new TableData<TransactionForDisplay>
        {
            TotalItems = transactions.TotalTransactionCount,
            Items = transactions.Transactions.ToList().Prepend(newTransaction)
        };
    }

    private async Task HandleKeyDown(KeyboardEventArgs args, TransactionForDisplay context, bool isDateField = false)
    {
        if (args.Key == "Enter")
        {
            await AddEditTransaction(context);
            return;
        }

        if (isDateField && args.Key == "ArrowUp")
        {
            if (DateTime.TryParse(context.TransactionDateForEdit, out var date))
            {
                context.TransactionDateForEdit = date.AddDays(1).ToString("MM/dd/yyyy");
            }
        }
        else if (isDateField && args.Key == "ArrowDown")
        {
            if (DateTime.TryParse(context.TransactionDateForEdit, out var date))
            {
                context.TransactionDateForEdit = date.AddDays(-1).ToString("MM/dd/yyyy");
            }
        }
    }

    private async Task AddEditTransaction(TransactionForDisplay transaction)
    {
        if ((string.IsNullOrEmpty(transaction.CreditForEdit) && string.IsNullOrEmpty(transaction.DebitForEdit))
            || string.IsNullOrEmpty(transaction.TransactionDateForEdit)
           )
        {
            return;
        }

        if (decimal.TryParse(transaction.CreditForEdit, out var credit))
        {
            transaction.Credit = credit;
        }
        else
        {
            transaction.Credit = null;
        }

        if (decimal.TryParse(transaction.DebitForEdit, out var debit))
        {
            transaction.Debit = debit;
        }
        else
        {
            transaction.Debit = null;
        }

        if (DateTime.TryParse(transaction.TransactionDateForEdit, out var date))
        {
            transaction.TransactionDate = date;
        }
        else
        {
            return;
        }

        if (transaction == newTransaction)
        {
            await accountService.AddTransaction(AccountId, transaction);
            newTransaction = new TransactionForDisplay
            {
                TransactionDate = transaction.TransactionDate,
                TransactionDateForEdit = transaction.TransactionDate.ToString("MM/dd/yyyy")
            };
            await ReloadTransactions();
            await accountSyncService.RefreshAccounts();
            await SetFocus();
            return;
        }

        editingTransaction = null;
        transactionBackup = null;
        await accountService.UpdateTransaction(transaction);
        await ReloadTransactions();
        await accountSyncService.RefreshAccounts();
    }

    private async Task SetFocus()
    {
        if (transactionDateField != null)
        {
            await transactionDateField.FocusAsync();
        }
    }

    private async Task ReloadTransactions()
    {
        if (transactionTable != null)
        {
            await transactionTable.ReloadServerData();
        }
    }

    private Task<IEnumerable<string>> SearchCategories(string? search, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
        }

        var searchLower = search.ToLower();
        return Task.FromResult(categories
            .Where(c => c.Name.ToLower().Contains(searchLower, StringComparison.CurrentCultureIgnoreCase))
            .Select(c => c.Name));
    }

    private Task<IEnumerable<string>> SearchVendors(string? search, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
        }

        var searchLower = search.ToLower();
        return Task.FromResult(vendors
            .Where(c => c.Name.ToLower().Contains(searchLower, StringComparison.CurrentCultureIgnoreCase))
            .Select(c => c.Name));
    }

    private async Task DeleteTransaction(TransactionForDisplay context)
    {
        await accountService.DeleteTransaction(context.TransactionId);
        await ReloadTransactions();
        await accountSyncService.RefreshAccounts();
    }

    private async Task CopyBalance(TransactionForDisplay context)
    {
        await jsRuntime.InvokeVoidAsync("copyToClipboard", context.RunningTotal.ToString("N2"));
    }

    private void FindCategoryForVendor(string? vendor, TransactionForDisplay context)
    {
        if (string.IsNullOrWhiteSpace(vendor))
        {
            context.Vendor = "";
            return;
        }

        context.Vendor = vendor;
        var selectedVendor =
            vendors.FirstOrDefault(v => string.Equals(v.Name, vendor, StringComparison.CurrentCultureIgnoreCase));
        if (selectedVendor == null)
        {
            return;
        }

        var category = categories.FirstOrDefault(c => c.CategoryId == selectedVendor.LastTransactionCategoryId);
        if (category == null)
        {
            return;
        }

        context.CategoryDisplay = category.Name;
    }
}