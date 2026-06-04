using Buenaventura.Domain;
using Buenaventura.Services;
using Buenaventura.Shared;
using Buenaventura.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace Buenaventura.Tests.Services;

public class ReconciliationServiceTests
{
    [Fact]
    public async Task GetWorkspace_ReturnsUnreconciledTransactionsUpToAsOfDate()
    {
        await using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var account = CreateAccount();
        var reconciled = CreateTransaction(account, new DateTime(2026, 1, 1), 100m, true);
        var unreconciled = CreateTransaction(account, new DateTime(2026, 1, 5), -25m);
        var future = CreateTransaction(account, new DateTime(2026, 2, 1), -10m);
        context.Accounts.Add(account);
        context.Transactions.AddRange(reconciled, unreconciled, future);
        await context.SaveChangesAsync();

        var service = new ReconciliationService(context);

        var workspace = await service.GetWorkspace(account.AccountId, new DateTime(2026, 1, 31));

        workspace.ReconciledBalance.Should().Be(100m);
        workspace.CurrentBalance.Should().Be(65m);
        workspace.Transactions.Should().ContainSingle(t => t.TransactionId == unreconciled.TransactionId);
        workspace.Transactions.Should().NotContain(t => t.TransactionId == reconciled.TransactionId);
        workspace.Transactions.Should().NotContain(t => t.TransactionId == future.TransactionId);
    }

    [Fact]
    public async Task Complete_MarksSelectedAccountTransactionsAsReconciled()
    {
        await using var context = TestDbContextFactory.CreateInMemoryDbContext();
        var account = CreateAccount();
        var selected = CreateTransaction(account, new DateTime(2026, 1, 5), -25m);
        var notSelected = CreateTransaction(account, new DateTime(2026, 1, 6), -10m);
        context.Accounts.Add(account);
        context.Transactions.AddRange(selected, notSelected);
        await context.SaveChangesAsync();

        var service = new ReconciliationService(context);

        await service.Complete(new CompleteReconciliationRequest
        {
            AccountId = account.AccountId,
            AsOfDate = new DateTime(2026, 1, 31),
            TargetBalance = 75m,
            TransactionIds = [selected.TransactionId]
        });

        context.Transactions.Single(t => t.TransactionId == selected.TransactionId).IsReconciled.Should().BeTrue();
        context.Transactions.Single(t => t.TransactionId == notSelected.TransactionId).IsReconciled.Should().BeFalse();
    }

    private static Account CreateAccount()
    {
        return new Account
        {
            AccountId = Guid.NewGuid(),
            Name = "Checking",
            Currency = "USD",
            Vendor = "Bank",
            AccountType = AccountType.BANK_ACCOUNT,
            MortgageType = ""
        };
    }

    private static Transaction CreateTransaction(
        Account account,
        DateTime date,
        decimal amount,
        bool isReconciled = false)
    {
        return new Transaction
        {
            TransactionId = Guid.NewGuid(),
            AccountId = account.AccountId,
            Account = account,
            TransactionDate = date,
            Amount = amount,
            AmountInBaseCurrency = amount,
            Description = "Test transaction",
            EnteredDate = date,
            IsReconciled = isReconciled,
            TransactionType = TransactionType.REGULAR
        };
    }
}
