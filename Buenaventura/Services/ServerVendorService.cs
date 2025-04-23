using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerVendorService(
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