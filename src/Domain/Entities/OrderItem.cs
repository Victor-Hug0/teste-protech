using Domain.Common;

namespace Domain.Entities;

public sealed class OrderItem : LongEntity
{
    public Guid OrderId { get; set; }
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
