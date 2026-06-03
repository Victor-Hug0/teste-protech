using Application.Abstractions.Persistence;
using Application.Orders;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(ApplicationDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Orders
            .Include(o => o.Buyer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> ListAsync(
        OrderListFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = context.Orders
            .AsNoTracking()
            .Include(o => o.Buyer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .AsQueryable();

        if (filter.BuyerId.HasValue)
            query = query.Where(o => o.BuyerId == filter.BuyerId.Value);

        if (filter.Statuses is { Count: > 0 })
            query = query.Where(o => filter.Statuses.Contains(o.Status));

        if (filter.CreatedFrom.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.CreatedFrom.Value);

        if (filter.CreatedTo.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.CreatedTo.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default) =>
        await context.Orders.AddAsync(order, cancellationToken);

    public void Remove(Order order) => context.Orders.Remove(order);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
