using System.ComponentModel.DataAnnotations;

namespace Teste.Contracts.Orders;

public sealed record OrderItemRequest
{
    [Range(1, long.MaxValue)]
    public long ProductId { get; init; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }
}
