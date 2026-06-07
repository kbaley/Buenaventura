using Buenaventura.Shared;
using Refit;

namespace Buenaventura.Client.Services;

public interface IVendorsApi
{
    [Get("/api/vendors")]
    public Task<IEnumerable<VendorModel>> GetVendors();

    [Get("/api/vendors/page?page={page}&pageSize={pageSize}&sortBy={sortBy}&sortDescending={sortDescending}")]
    public Task<PaginatedResults<VendorModel>> GetVendors(
        int page = 0,
        int pageSize = 10,
        string sortBy = "name",
        bool sortDescending = false);

    [Get("/api/vendors/{id}")]
    public Task<VendorModel> GetVendor(Guid id);

    [Put("/api/vendors")]
    public Task UpdateVendor(VendorModel vendorModel);

    [Delete("/api/vendors/{id}")]
    public Task DeleteVendor(Guid id);

    [Delete("/api/vendors?lastPostedBefore={lastPostedBefore}")]
    public Task<int> DeleteVendorsLastPostedBefore(DateTime lastPostedBefore);
}
