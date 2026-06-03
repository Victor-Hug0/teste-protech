using System.ComponentModel.DataAnnotations;

namespace Teste.Contracts.Orders;

public sealed record CreateOrderRequest
{
    [Required]
    public Guid BuyerId { get; init; }

    [Required]
    [MinLength(1, ErrorMessage = "O pedido deve conter pelo menos um produto.")]
    public required IReadOnlyList<OrderItemRequest> Items { get; init; }
}
