using Buenaventura.Data;
using FastEndpoints;

namespace Buenaventura.Api;

internal class GetAvailableTransactionTags(ITransactionRepository transactionRepository) : EndpointWithoutRequest<List<string>>
{
    public override void Configure()
    {
        Get("/api/transactions/available-tags");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(await transactionRepository.GetAvailableTags(), cancellation: ct);
    }
}
