using Buenaventura.Data;
using Buenaventura.Shared;
using Microsoft.EntityFrameworkCore;

namespace Buenaventura.Services;

public interface IVendorService : IAppService
{
    public Task<IEnumerable<VendorModel>> GetVendors();
    public Task<PaginatedResults<VendorModel>> GetVendors(int page, int pageSize, string? sortBy, bool sortDescending);
    public Task<VendorModel> GetVendor(Guid id);
    public Task UpdateVendor(VendorModel vendorModel);
    public Task DeleteVendor(Guid id);
    public Task<int> DeleteVendorsLastPostedBefore(DateTime date);
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

    public async Task<PaginatedResults<VendorModel>> GetVendors(
        int page,
        int pageSize,
        string? sortBy,
        bool sortDescending)
    {
        var query = context.Vendors.Select(v => new VendorModel
        {
            VendorId = v.VendorId,
            Name = v.Name,
            LastTransactionCategoryId = v.LastTransactionCategoryId,
            CategoryName = context.Categories
                .Where(c => c.CategoryId == v.LastTransactionCategoryId)
                .Select(c => c.Name)
                .FirstOrDefault() ?? "",
            LastTransactionDate = context.Transactions
                .Where(t => t.Vendor != null && t.Vendor.ToLower() == v.Name.ToLower())
                .Max(t => (DateTime?)t.TransactionDate)
        });

        query = (sortBy, sortDescending) switch
        {
            ("lastPosted", true) => query
                .OrderByDescending(v => v.LastTransactionDate)
                .ThenBy(v => v.Name),
            ("lastPosted", false) => query
                .OrderBy(v => v.LastTransactionDate)
                .ThenBy(v => v.Name),
            (_, true) => query.OrderByDescending(v => v.Name),
            _ => query.OrderBy(v => v.Name)
        };

        var totalCount = await query.CountAsync();
        var vendors = await query
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResults<VendorModel>
        {
            Items = vendors,
            TotalCount = totalCount
        };
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

    public async Task<int> DeleteVendorsLastPostedBefore(DateTime date)
    {
        var cutoff = date.Date;
        var vendorIds = await context.Vendors
            .Select(v => new
            {
                v.VendorId,
                LastTransactionDate = context.Transactions
                    .Where(t => t.Vendor != null && t.Vendor.ToLower() == v.Name.ToLower())
                    .Max(t => (DateTime?)t.TransactionDate)
            })
            .Where(v => v.LastTransactionDate.HasValue && v.LastTransactionDate < cutoff)
            .Select(v => v.VendorId)
            .ToListAsync();

        if (!vendorIds.Any())
        {
            return 0;
        }

        var vendors = await context.Vendors
            .Where(v => vendorIds.Contains(v.VendorId))
            .ToListAsync();

        context.Vendors.RemoveRange(vendors);
        await context.SaveChangesAsync();
        return vendors.Count;
    }
}
