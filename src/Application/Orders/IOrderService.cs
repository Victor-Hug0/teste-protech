using Application.DTOs;

namespace Application.Orders;

public interface IOrderService
{
    Task<PagedResult<OrderDto>> ListAsync(OrderListFilter filter, CancellationToken cancellationToken = default);
    Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateAsync(
        Guid buyerId,
        IReadOnlyList<OrderLineInput> items,
        CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateDetailsAsync(
        Guid id,
        Guid buyerId,
        IReadOnlyList<OrderLineInput> items,
        CancellationToken cancellationToken = default);
    Task<OrderDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
