namespace Application.DTOs;

public sealed record OrderDto(
    Guid Id,
    Guid BuyerId,
    string BuyerName,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<OrderItemDto> Items,
    decimal Total);
