using Buenaventura.Client.Services;
using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public class ServerVendorService(
    IDbContextFactory<CoronadoDbContext> dbContextFactory
) : IVendorService
{
    public async Task<IEnumerable<VendorModel>> GetVendors()
    {
        var context = await dbContextFactory.CreateDbContextAsync();
        var vendors = await context.Vendors.ToListAsync();
        return vendors.Select(v => new VendorModel
        {
            VendorId = v.VendorId,
            Name = v.Name,
            LastTransactionCategoryId = v.LastTransactionCategoryId
        });
    }
}