using Buenaventura.Client.Services;
using Buenaventura.Shared;

namespace Buenaventura.Services;

public interface IVendorService : IAppService
{
    public Task<IEnumerable<VendorModel>> GetVendors();
}