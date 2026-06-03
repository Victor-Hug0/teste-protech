using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public sealed class Order : Entity
{
    public Guid BuyerId { get; private set; }
    public string Status { get; private set; } = OrderStatus.Iniciado;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Buyer Buyer { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    private Order() { }

    public static Order Create(Guid buyerId, IReadOnlyList<OrderItem> items)
    {
        if (buyerId == Guid.Empty)
            throw new DomainException("O comprador é obrigatório.");

        ValidateItems(items);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            BuyerId = buyerId,
            Status = OrderStatus.Iniciado,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in items)
        {
            item.AssignToOrder(order.Id);
            order.Items.Add(item);
        }

        return order;
    }

    public void UpdateDetails(Guid buyerId, IReadOnlyList<OrderItem> items)
    {
        EnsureCanUpdateDetails();

        if (buyerId == Guid.Empty)
            throw new DomainException("O comprador é obrigatório.");

        ValidateItems(items);

        BuyerId = buyerId;
        Items.Clear();

        foreach (var item in items)
        {
            item.AssignToOrder(Id);
            Items.Add(item);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void Process()
    {
        if (Status != OrderStatus.Iniciado)
            throw new DomainException("Apenas pedidos iniciados podem ser processados.");

        Status = OrderStatus.Processado;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Processado)
            throw new DomainException("Apenas pedidos processados podem ser enviados.");

        Status = OrderStatus.Enviado;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is not (OrderStatus.Iniciado or OrderStatus.Processado))
            throw new DomainException("Apenas pedidos iniciados ou processados podem ser cancelados.");

        Status = OrderStatus.Cancelado;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnsureCanDelete()
    {
        if (Status != OrderStatus.Iniciado)
            throw new DomainException("Apenas pedidos iniciados podem ser excluídos.");
    }

    public decimal Total => Items.Sum(i => i.LineTotal);

    private static void ValidateItems(IReadOnlyList<OrderItem> items)
    {
        if (items.Count == 0)
            throw new DomainException("O pedido deve conter pelo menos um produto.");

        if (items.GroupBy(i => i.ProductId).Any(g => g.Count() > 1))
            throw new DomainException("O pedido não pode conter o mesmo produto mais de uma vez.");
    }

    private void EnsureCanUpdateDetails()
    {
        if (Status != OrderStatus.Iniciado)
            throw new DomainException("Apenas pedidos não processados podem ser alterados.");
    }
}
