using Buenaventura.Shared;
using Buenaventura.Services;

namespace Buenaventura.Data
{
    public interface IReportRepository : IServerAppService
    {
        Task<decimal> GetNetWorthFor(DateTime date);
        Task<decimal> GetInvestmentTotalFor(DateTime date);
        Task<IEnumerable<CategoryTotal>> GetTransactionsByCategoryType(string categoryType, DateTime start, DateTime end);
        Task<IEnumerable<dynamic>> GetMonthlyTotalsForCategory(Guid categoryId, DateTime start, DateTime end);
        IEnumerable<CategoryTotal> GetInvoiceLineItemsIncomeTotals(DateTime start, DateTime end);
    }
}