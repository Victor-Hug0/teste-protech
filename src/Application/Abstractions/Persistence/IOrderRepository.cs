using Application.Orders;
using Domain.Entities;

namespace Application.Abstractions.Persistence;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Order> Items, int TotalCount)> ListAsync(
        OrderListFilter filter,
        CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    void Remove(Order order);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
