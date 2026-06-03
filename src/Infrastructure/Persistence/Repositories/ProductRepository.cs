using Application.Abstractions.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(ApplicationDbContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        await context.Products
            .Include(p => p.OrderItems)
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Products
            .AsNoTracking()
            .Include(p => p.Categories)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsByNameAsync(
        string name,
        long? excludeProductId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();
        var query = context.Products.AsNoTracking()
            .Where(p => p.Name.ToLower() == normalized);

        if (excludeProductId.HasValue)
            query = query.Where(p => p.Id != excludeProductId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> IsUsedInOrdersAsync(long id, CancellationToken cancellationToken = default) =>
        await context.OrderItems.AsNoTracking().AnyAsync(i => i.ProductId == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetByIdsForLinkAsync(
        IReadOnlyList<long> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return [];

        return await context.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default) =>
        await context.Products.AddAsync(product, cancellationToken);

    public void Update(Product product) => context.Products.Update(product);

    public void Remove(Product product) => context.Products.Remove(product);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
