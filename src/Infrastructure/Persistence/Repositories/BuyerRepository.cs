using Application.Abstractions.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class BuyerRepository(ApplicationDbContext context) : IBuyerRepository
{
    public async Task<Buyer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Buyers
            .Include(b => b.Orders)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Buyer>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Buyers
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsByEmailAsync(
        string email,
        Guid? excludeBuyerId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var query = context.Buyers.AsNoTracking()
            .Where(b => b.Email.ToLower() == normalized);

        if (excludeBuyerId.HasValue)
            query = query.Where(b => b.Id != excludeBuyerId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Buyer buyer, CancellationToken cancellationToken = default) =>
        await context.Buyers.AddAsync(buyer, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
