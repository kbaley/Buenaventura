using Buenaventura.Shared.Retirement;
using Refit;

namespace Buenaventura.Client.Services;

public interface IRetirementApi
{
    [Post("/api/retirement/ask")]
    Task<RetirementQueryResponse> Ask([Body] RetirementQueryRequest request);
}
