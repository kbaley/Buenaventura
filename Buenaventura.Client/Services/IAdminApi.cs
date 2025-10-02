using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface IAdminApi
{
    [Post("/api/admin/scramble")]
    Task ScrambleDatabase(ScrambleModel model);
}