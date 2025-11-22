using Buenaventura.Domain;
using Buenaventura.Dtos;
using Buenaventura.Shared;

namespace Buenaventura;

/// <summary>
/// Yeah, this is a class of manual mapping methods, you wanna make something of it?
/// </summary>
public static class Mappers
{
    public static TransactionForDisplay ToDto(this Transaction transaction)
    {
        return new TransactionForDisplay
        {
            AccountId = transaction.AccountId,
            AccountName = transaction.Account?.Name ?? "",
            Amount = transaction.Amount,
            Debit = transaction.Amount < 0 ? (0 - transaction.Amount) : null,
            Credit = transaction.Amount > 0 ? transaction.Amount : null,
            AmountInBaseCurrency = transaction.AmountInBaseCurrency,
            Category = new CategoryModel
                {
                    CategoryId = transaction.Category?.CategoryId,
                    Name = transaction.GetCategoryDisplay(),
                },
            Description = transaction.Description ?? "",
            TransactionDate = transaction.TransactionDate,
            EnteredDate = transaction.EnteredDate,
            Vendor = transaction.Vendor,
            TransactionId = transaction.TransactionId,
            TransactionType = transaction.TransactionType,
            DownloadId = transaction.DownloadId
        };
    }

    public static Investment ToInvestment(this InvestmentForUpdateDto dto)
    {
        return new Investment
        {
            CategoryId = dto.CategoryId,
            Currency = dto.Currency,
            DontRetrievePrices = dto.DontRetrievePrices,
            InvestmentId = dto.InvestmentId,
            Name = dto.Name,
            Symbol = dto.Symbol,
            PaysDividends = dto.PaysDividends,
        };
    }

    public static InvestmentModel ToModel(this Investment investment)
    {
        return new InvestmentModel
        {
            CategoryId = investment.CategoryId,
            Currency = investment.Currency,
            DontRetrievePrices = investment.DontRetrievePrices,
            InvestmentId = investment.InvestmentId,
            LastPrice = investment.LastPrice,
            Name = investment.Name,
            PaysDividends = investment.PaysDividends,
            Symbol = investment.Symbol,
            AnnualizedIrr = investment.GetAnnualizedIrr(),
            AveragePrice = Math.Round(investment.GetAveragePricePaid(), 2),
            Shares = investment.GetNumberOfShares(),
            CurrentValue = Math.Round(investment.GetCurrentValue(), 2),
            Date =  investment.LastPriceRetrievalDate,
            Price = investment.LastPrice
        };
    }
    
    public static Invoice ToInvoice(this InvoiceForPosting dto)
    {
        return new Invoice
        {
            Date = dto.Date,
            Balance = dto.Balance,
            CustomerId = dto.CustomerId,
            InvoiceId = dto.InvoiceId,
            InvoiceNumber = dto.InvoiceNumber,
            LastSentToCustomer = dto.LastSentToCustomer,
            LineItems = dto.LineItems.Select(i => i.ToLineItem()).ToList(),
        };
    }
    
    public static InvoiceLineItem ToLineItem(this InvoiceLineItemsForPosting dto)
    {
        return new InvoiceLineItem
        {
            CategoryId = dto.CategoryId,
            InvoiceId = dto.InvoiceId,
            Description = dto.Description,
            InvoiceLineItemId = dto.InvoiceLineItemId,
            Quantity = dto.Quantity,
            UnitAmount = dto.UnitAmount,
        };
    }
    
    public static InvoiceForPosting ToDto(this Invoice invoice)
    {
        return new InvoiceForPosting
        {
            Date = invoice.Date,
            Balance = invoice.Balance,
            CustomerId = invoice.CustomerId,
            InvoiceId = invoice.InvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            LastSentToCustomer = invoice.LastSentToCustomer?.DateTime,
            LineItems = invoice.LineItems.Select(i => i.ToDto()).ToList(),
            CustomerName = invoice.Customer?.Name ?? "",
            CustomerStreetAddress = invoice.Customer?.StreetAddress ?? "",
            CustomerCity = invoice.Customer?.City ?? "",
            CustomerRegion = invoice.Customer?.Region ?? "",
            CustomerEmail = invoice.Customer?.Email ?? ""
        };
    }
    
    private static InvoiceLineItemsForPosting ToDto(this InvoiceLineItem dto)
    {
        return new InvoiceLineItemsForPosting
        {
            CategoryId = dto.CategoryId ?? Guid.Empty,
            InvoiceId = dto.InvoiceId,
            Description = dto.Description,
            InvoiceLineItemId = dto.InvoiceLineItemId,
            Quantity = dto.Quantity,
            UnitAmount = dto.UnitAmount,
        };
    }

    public static InvestmentDetailDto ToDto(this Investment investment)
    {
        return new InvestmentDetailDto
        {
            CategoryId = investment.CategoryId ?? Guid.Empty,
            Currency = investment.Currency,
            DontRetrievePrices = investment.DontRetrievePrices,
            InvestmentId = investment.InvestmentId,
            LastPrice = investment.LastPrice,
            Name = investment.Name,
            Symbol = investment.Symbol,
            AveragePrice = Math.Round(investment.GetAveragePricePaid(), 2),
            TotalReturn = investment.GetTotalReturn(),
            TotalAnnualizedReturn = investment.GetAnnualizedIrr(),
            Shares = investment.GetNumberOfShares(),
            CategoryName = investment.Category?.Name ?? "",
            CategoryPercentage =  investment.Category?.Percentage ?? 0,
            TotalPaid = decimal.Round(investment.Transactions.Sum(t => t.Shares * t.Price), 2),
            CurrentValue = decimal.Round(investment.LastPrice * investment.GetNumberOfShares(), 2),
            BookValue = decimal.Round(investment.GetAveragePricePaid() * investment.GetNumberOfShares(), 2),
            Transactions = investment.Transactions.Select(t => t.ToDto()).ToList()
        };

    }

    private static InvestmentTransactionDto ToDto(this InvestmentTransaction investmentTransaction)
    {
        return new InvestmentTransactionDto
        {
            Date = investmentTransaction.Date,
            Price = investmentTransaction.Price,
            InvestmentTransactionId = investmentTransaction.InvestmentTransactionId,
            Shares = investmentTransaction.Shares,
            SourceAccountName = investmentTransaction.Transaction?.Account?.Name ?? ""
        };
    }
    
    public static InvestmentCategory ToInvestmentCategory(this InvestmentCategoryForUpdate dto)
    {
        return new InvestmentCategory
        {
            InvestmentCategoryId = dto.InvestmentCategoryId,
            Name = dto.Name,
            Percentage = dto.Percentage,
        };
    }
}