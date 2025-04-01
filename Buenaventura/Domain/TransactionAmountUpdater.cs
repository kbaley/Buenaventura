namespace Buenaventura.Domain;

public abstract class TransactionAmountUpdater
{
    protected readonly Transaction _transaction;
    protected readonly decimal _cadExchangeRate;

    public TransactionAmountUpdater(Transaction transaction, decimal cadExchangeRate)
    {
        _transaction = transaction;
        _cadExchangeRate = cadExchangeRate;
    }

    // Returns an updated corresponding transaction, if one exists
    public abstract Transaction UpdateAmount(decimal newAmount);
}

public class TransactionAmountUpdaterRegular : TransactionAmountUpdater
{
    public TransactionAmountUpdaterRegular(Transaction transaction, decimal cadExchangeRate) : base(transaction,
        cadExchangeRate)
    {
    }

    public override Transaction UpdateAmount(decimal newAmount)
    {
        // For regular transactions, if this is a CAD account, also update the AmountInBaseCurrency
        _transaction.Amount = newAmount;
        var accountCurrency = _transaction.Account.Currency;
        _transaction.AmountInBaseCurrency = (accountCurrency == "CAD")
            ? Math.Round(_transaction.Amount / _cadExchangeRate, 2)
            : _transaction.Amount;
        return null;
    }
}

public class TransactionAmountUpdaterTransfer : TransactionAmountUpdater
{
    public TransactionAmountUpdaterTransfer(Transaction transaction, decimal cadExchangeRate) : base(transaction,
        cadExchangeRate)
    {
    }

    public override Transaction UpdateAmount(decimal newAmount)
    {
        // If both sides are USD, update the Amount and AmountInBaseCurrency for both sides
        // If one side is CAD, we update the AmountInBaseCurrency on both sides IFF
        //     the USD side of the transaction has been updated. If the CAD side has been updated, we
        //     leave AmountInBaseCurrency as is. I.e. the USD side is the source of truth.
        // If both sides are CAD, we update the AmountInBaseCurrency on both sides
        _transaction.Amount = newAmount;
        var relatedTransaction = _transaction.LeftTransfer.RightTransaction;
        var relatedAccountCurrency = _transaction.LeftTransfer.RightTransaction.Account.Currency;
        var accountCurrency = _transaction.Account.Currency;
        if (relatedAccountCurrency == "USD" && accountCurrency == "USD")
        {
            // Update the amounts and amounts in USD on both sides of the transaction
            _transaction.AmountInBaseCurrency = _transaction.Amount;
            relatedTransaction.Amount = 0 - _transaction.Amount;
            relatedTransaction.AmountInBaseCurrency = 0 - _transaction.Amount;
        }
        else if (relatedAccountCurrency == "CAD" && accountCurrency == "CAD")
        {
            // Update the amounts and amounts in USD on both sides of the transaction
            _transaction.AmountInBaseCurrency = Math.Round(_transaction.Amount / _cadExchangeRate, 2);
            relatedTransaction.Amount = 0 - _transaction.Amount;
            relatedTransaction.AmountInBaseCurrency = 0 - _transaction.AmountInBaseCurrency;
        }
        else if (accountCurrency == "USD" && relatedAccountCurrency == "CAD")
        {
            // Update the amounts in USD on both sides of the transaction
            // Leave the CAD amount as is in the related transaction
            _transaction.AmountInBaseCurrency = _transaction.Amount;
            relatedTransaction.AmountInBaseCurrency = 0 - _transaction.AmountInBaseCurrency;
        }
        else if (accountCurrency == "CAD" && relatedAccountCurrency == "USD")
        {
            // Just update the amount on this side of the transfer
            // Leave the amounts in USD as is on both sides
            // Leave the amount as is on the related transaction
        }

        return relatedTransaction;
    }
}