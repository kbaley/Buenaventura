using Buenaventura.Client.Services;
using Buenaventura.Dtos;
using Buenaventura.Services;

namespace Buenaventura.Data
{
    public interface IReportRepository : IServerAppService
    {
        decimal GetNetWorthFor(DateTime date);
        decimal GetInvestmentTotalFor(DateTime date);
        IEnumerable<CategoryTotal> GetTransactionsByCategoryType(string categoryType, DateTime start, DateTime end);
        Task<IEnumerable<dynamic>> GetMonthlyTotalsForCategory(Guid categoryId, DateTime start, DateTime end);
        IEnumerable<CategoryTotal> GetInvoiceLineItemsIncomeTotals(DateTime start, DateTime end);
    }
}