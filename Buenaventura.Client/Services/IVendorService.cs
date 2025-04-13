using Buenaventura.Shared;

namespace Buenaventura.Client.Services;

public interface IVendorService : IAppService
{
    public Task<IEnumerable<VendorDto>> GetVendors();
}