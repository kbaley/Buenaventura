using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface IReconciliationApi
{
    [Get("/api/accounts/{accountId}/reconciliation?asOfDate={asOfDate}")]
    Task<ReconciliationWorkspace> GetWorkspace(Guid accountId, DateTime asOfDate);

    [Post("/api/accounts/{accountId}/reconciliation/complete")]
    Task Complete(Guid accountId, CompleteReconciliationRequest request);
}
