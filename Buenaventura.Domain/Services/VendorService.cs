using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public interface IVendorService : IAppService
{
    public Task<IEnumerable<VendorModel>> GetVendors();
}

public class VendorService(
    BuenaventuraDbContext context
) : IVendorService
{
    public async Task<IEnumerable<VendorModel>> GetVendors()
    {
        var vendors = await context.Vendors.ToListAsync();
        return vendors.Select(v => new VendorModel
        {
            VendorId = v.VendorId,
            Name = v.Name,
            LastTransactionCategoryId = v.LastTransactionCategoryId
        });
    }
}