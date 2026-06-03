using Domain.Common;
using Domain.Exceptions;

namespace Domain.Entities;

public sealed class OrderItem : LongEntity
{
    public Guid OrderId { get; private set; }
    public long ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;

    private OrderItem() { }

    public static OrderItem Create(long productId, int quantity, decimal unitPrice)
    {
        if (productId <= 0)
            throw new DomainException("O produto é obrigatório.", BusinessRuleCodes.Order.ItemProductRequired);

        if (quantity <= 0)
            throw new DomainException("A quantidade deve ser maior que zero.", BusinessRuleCodes.Order.ItemQuantityInvalid);

        if (unitPrice < 0)
            throw new DomainException("O preço unitário não pode ser negativo.", BusinessRuleCodes.Order.ItemPriceNegative);

        return new OrderItem
        {
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }

    internal void AssignToOrder(Guid orderId) => OrderId = orderId;

    public decimal LineTotal => Quantity * UnitPrice;
}
