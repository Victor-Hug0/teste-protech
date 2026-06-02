using Teste.Domain.Common;

namespace Teste.Domain.Entities;

public sealed class Category : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
}
