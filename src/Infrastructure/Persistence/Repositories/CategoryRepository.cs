using Application.Abstractions.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(ApplicationDbContext context) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        await context.Categories
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsByNameAsync(
        string name,
        long? excludeCategoryId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();
        var query = context.Categories.AsNoTracking()
            .Where(c => c.Name.ToLower() == normalized);

        if (excludeCategoryId.HasValue)
            query = query.Where(c => c.Id != excludeCategoryId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default) =>
        await context.Categories.AsNoTracking().AnyAsync(c => c.Id == id, cancellationToken);

    public async Task<bool> HasChildrenAsync(long id, CancellationToken cancellationToken = default) =>
        await context.Categories.AsNoTracking().AnyAsync(c => c.ParentId == id, cancellationToken);

    public async Task<bool> HasProductsAsync(long id, CancellationToken cancellationToken = default) =>
        await context.Categories.AsNoTracking()
            .AnyAsync(c => c.Id == id && c.Products.Any(), cancellationToken);

    public async Task<IReadOnlyList<Category>> GetByIdsForLinkAsync(
        IReadOnlyList<long> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return [];

        return await context.Categories
            .Where(c => ids.Contains(c.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default) =>
        await context.Categories.AddAsync(category, cancellationToken);

    public void Update(Category category) => context.Categories.Update(category);

    public void Remove(Category category) => context.Categories.Remove(category);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
