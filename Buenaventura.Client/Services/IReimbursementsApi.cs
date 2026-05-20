using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface IReimbursementsApi
{
    [Get("/api/reimbursements/summary")]
    Task<ReimbursementSummary> GetSummary();

    [Get("/api/reimbursements/report")]
    Task<ReimbursementReport> GetReport();

    [Post("/api/reimbursements/settlements")]
    Task CreateSettlement([Body] CreateReimbursementSettlementRequest request);
}
