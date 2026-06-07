using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public interface IVendorService : IAppService
{
    public Task<IEnumerable<VendorModel>> GetVendors();
    public Task<VendorModel> GetVendor(Guid id);
    public Task UpdateVendor(VendorModel vendorModel);
    public Task DeleteVendor(Guid id);
}

public class VendorService(
    BuenaventuraDbContext context
) : IVendorService
{
    public async Task<IEnumerable<VendorModel>> GetVendors()
    {
        var vendors = await context.Vendors.ToListAsync();
        var categories = await context.Categories.ToDictionaryAsync(c => c.CategoryId);
        var lastTransactionDates = await context.Transactions
            .Where(t => !string.IsNullOrWhiteSpace(t.Vendor))
            .GroupBy(t => t.Vendor!.ToLower())
            .Select(g => new
            {
                VendorName = g.Key,
                LastTransactionDate = g.Max(t => t.TransactionDate)
            })
            .ToDictionaryAsync(t => t.VendorName, t => t.LastTransactionDate);

        return vendors
            .Select(v =>
            {
                categories.TryGetValue(v.LastTransactionCategoryId, out var category);
                lastTransactionDates.TryGetValue(v.Name.ToLower(), out var lastTransactionDate);

                return new VendorModel
                {
                    VendorId = v.VendorId,
                    Name = v.Name,
                    LastTransactionCategoryId = v.LastTransactionCategoryId,
                    CategoryName = category?.Name ?? "",
                    LastTransactionDate = lastTransactionDate == default ? null : lastTransactionDate
                };
            })
            .OrderBy(v => v.Name)
            .ToList();
    }

    public async Task<VendorModel> GetVendor(Guid id)
    {
        var vendor = await context.Vendors.FindAsync(id);
        if (vendor == null)
        {
            throw new Exception("Vendor not found");
        }

        return new VendorModel
        {
            VendorId = vendor.VendorId,
            Name = vendor.Name,
            LastTransactionCategoryId = vendor.LastTransactionCategoryId
        };
    }

    public async Task UpdateVendor(VendorModel vendorModel)
    {
        var vendor = await context.Vendors.FindAsync(vendorModel.VendorId);
        if (vendor == null)
        {
            throw new Exception("Vendor not found");
        }

        var originalName = vendor.Name;
        vendor.Name = vendorModel.Name;
        vendor.LastTransactionCategoryId = vendorModel.LastTransactionCategoryId;

        var matchingTransactions = await context.Transactions
            .Where(t => t.Vendor != null && t.Vendor.ToLower() == originalName.ToLower())
            .ToListAsync();
        foreach (var transaction in matchingTransactions)
        {
            transaction.Vendor = vendorModel.Name;
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteVendor(Guid id)
    {
        await context.Vendors.RemoveByIdAsync(id);
        await context.SaveChangesAsync();
    }
}
