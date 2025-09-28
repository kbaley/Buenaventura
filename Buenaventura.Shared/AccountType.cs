namespace Buenaventura.Shared;

public static class AccountType
{
    public static readonly string CASH = "Cash";
    public static readonly string BANK_ACCOUNT = "Bank Account";
    public static readonly string MORTGAGE = "Mortgage";
    public static readonly string ASSET = "Asset";
    public static readonly string LOAN = "Loan";
    public static readonly string INVESTMENT = "Investment";
    public static readonly string CREDIT_CARD = "Credit Card";

    public static IEnumerable<string> GetAccountTypes()
    {
        var types = new List<string>
        {
            CASH,
            BANK_ACCOUNT,
            ASSET,
            LOAN,
            INVESTMENT,
            CREDIT_CARD,
            MORTGAGE,
        };

        return types;
    }
}