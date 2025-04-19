using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface IAdminService : IAppService
{
    Task ScrambleDatabase(ScrambleModel model);
}