using Application.DTOs;
using Domain.Entities;

namespace Application.Common.Mappings;

public static class OrderMapper
{
    public static OrderDto ToDto(this Order order) =>
        new(
            order.Id,
            order.BuyerId,
            order.Buyer?.Name ?? string.Empty,
            order.Status,
            order.CreatedAt,
            order.UpdatedAt,
            order.Items.Select(i => i.ToDto()).ToList(),
            order.Total);

    public static OrderItemDto ToDto(this OrderItem item) =>
        new(
            item.Id,
            item.ProductId,
            item.Product?.Name ?? string.Empty,
            item.Quantity,
            item.UnitPrice,
            item.LineTotal);
}
