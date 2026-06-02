using Domain.Common;

namespace Domain.Entities;

public sealed class Buyer : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();

    private Buyer() { }

    private Buyer(Guid id, string name, string email, DateTime createdAt)
        : base(id)
    {
        Name = name;
        Email = email;
        CreatedAt = createdAt;
    }
    
    public static Buyer Create(string name, string email)
    {
        return new Buyer(Guid.NewGuid(), name, email, DateTime.UtcNow);
    }
}
