using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface IVendorsApi
{
    [Get("/api/vendors")]
    public Task<IEnumerable<VendorModel>> GetVendors();

    [Get("/api/vendors/{id}")]
    public Task<VendorModel> GetVendor(Guid id);

    [Put("/api/vendors")]
    public Task UpdateVendor(VendorModel vendorModel);

    [Delete("/api/vendors/{id}")]
    public Task DeleteVendor(Guid id);
}
