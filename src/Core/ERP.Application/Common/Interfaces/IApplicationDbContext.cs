using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Product> Products { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<InventoryTransaction> InventoryTransactions { get; }
    DbSet<Sale> Sales { get; }
    DbSet<SaleItem> SaleItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
