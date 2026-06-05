using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface IPlanningApi
{
    [Get("/api/planning/budget")]
    Task<BudgetPlanningSummary> GetBudgetPlanningSummary();
}
