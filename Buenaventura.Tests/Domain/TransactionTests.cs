using Buenaventura.Domain;
using Buenaventura.Shared;
using Buenaventura.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace Buenaventura.Tests.Domain;

public class TransactionTests
{
    [Fact]
    public void GetCategoryDisplay_RegularTransaction_ReturnsCategory()
    {
        // Arrange
        var category = TestDataFactory.CategoryFaker.Generate();
        var transaction = TestDataFactory.TransactionFaker.Generate();
        transaction.Category = category;
        transaction.TransactionType = TransactionType.REGULAR;
        
        // Act
        var result = transaction.GetCategoryDisplay();
        
        // Assert
        result.Should().Be(category.Name);
    }
    
    [Fact]
    public void GetCategoryDisplay_RegularTransactionWithoutCategory_ReturnsEmptyString()
    {
        // Arrange
        var transaction = TestDataFactory.TransactionFaker.Generate();
        transaction.Category = null;
        transaction.TransactionType = TransactionType.REGULAR;
        
        // Act
        var result = transaction.GetCategoryDisplay();
        
        // Assert
        result.Should().Be("");
    }
    
    [Fact]
    public void GetCategoryDisplay_InvoicePayment_ReturnsPaymentDescription()
    {
        // Arrange
        var transaction = TestDataFactory.TransactionFaker.Generate();
        transaction.TransactionType = TransactionType.INVOICE_PAYMENT;
        transaction.Invoice = new Invoice { InvoiceNumber = "INV-001" };
        
        // Act
        var result = transaction.GetCategoryDisplay();
        
        // Assert
        result.Should().Be("PAYMENT: INV-001");
    }
    
    [Fact]
    public void GetCategoryDisplay_InvoicePaymentWithoutInvoice_ReturnsPayment()
    {
        // Arrange
        var transaction = TestDataFactory.TransactionFaker.Generate();
        transaction.TransactionType = TransactionType.INVOICE_PAYMENT;
        transaction.Invoice = null;
        
        // Act
        var result = transaction.GetCategoryDisplay();
        
        // Assert
        result.Should().Be("PAYMENT");
    }
    
    [Fact]
    public void GetCategoryDisplay_Transfer_ReturnsTransferDescription()
    {
        // Arrange
        var account = TestDataFactory.AccountFaker.Generate();
        var rightTransaction = TestDataFactory.TransactionFaker.Generate();
        rightTransaction.Account = account;
        
        var transaction = TestDataFactory.TransactionFaker.Generate();
        transaction.TransactionType = TransactionType.TRANSFER;
        transaction.LeftTransfer = new Transfer { RightTransaction = rightTransaction };
        
        // Act
        var result = transaction.GetCategoryDisplay();
        
        // Assert
        result.Should().Be($"TRANSFER: {account.Name}");
    }
    
    [Fact]
    public void GetCategoryDisplay_Investment_ReturnsInvestment()
    {
        // Arrange
        var transaction = TestDataFactory.TransactionFaker.Generate();
        transaction.TransactionType = TransactionType.INVESTMENT;
        
        // Act
        var result = transaction.GetCategoryDisplay();
        
        // Assert
        result.Should().Be("INVESTMENT");
    }
    
    [Fact]
    public void GetCategoryDisplay_Dividend_ReturnsDividendWithCategory()
    {
        // Arrange
        var category = TestDataFactory.CategoryFaker.Generate();
        var transaction = TestDataFactory.TransactionFaker.Generate();
        transaction.Category = category;
        transaction.TransactionType = TransactionType.DIVIDEND;
        
        // Act
        var result = transaction.GetCategoryDisplay();
        
        // Assert
        result.Should().Be(category.Name);
    }
    
    [Fact]
    public void GetCategoryDisplay_DividendWithoutCategory_ReturnsDividend()
    {
        // Arrange
        var transaction = TestDataFactory.TransactionFaker.Generate();
        transaction.Category = null;
        transaction.TransactionType = TransactionType.DIVIDEND;
        
        // Act
        var result = transaction.GetCategoryDisplay();
        
        // Assert
        result.Should().Be("DIVIDEND");
    }
    
    [Fact]
    public void SetAmountInBaseCurrency_USDCurrency_SetsAmountAsIs()
    {
        // Arrange
        var transaction = TestDataFactory.TransactionFaker.Generate();
        transaction.Amount = 100m;
        
        // Act
        transaction.SetAmountInBaseCurrency("USD", 1.3m);
        
        // Assert
        transaction.AmountInBaseCurrency.Should().Be(100m);
    }
    
    [Fact]
    public void SetAmountInBaseCurrency_NonUSDCurrency_ConvertsAmount()
    {
        // Arrange
        var transaction = TestDataFactory.TransactionFaker.Generate();
        transaction.Amount = 100m;
        
        // Act
        transaction.SetAmountInBaseCurrency("CAD", 1.3m);
        
        // Assert
        transaction.AmountInBaseCurrency.Should().BeApproximately(100m / 1.3m, 0.01m);
    }
}
