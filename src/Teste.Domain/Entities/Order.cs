using Teste.Domain.Common;

namespace Teste.Domain.Entities;

public sealed class Order : Entity
{
    public Guid BuyerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Buyer Buyer { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
