using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface IVendorsApi
{
    [Get("/api/vendors")]
    public Task<IEnumerable<VendorModel>> GetVendors();
}