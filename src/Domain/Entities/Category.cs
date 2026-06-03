using Domain.Common;
using Domain.Exceptions;

namespace Domain.Entities;

public sealed class Category : LongEntity
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 500;

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public long? ParentId { get; private set; }
    public bool Active { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();

    private Category() { }

    public static Category Create(string name, string? description = null, long? parentId = null) =>
        new()
        {
            Name = ValidateName(name),
            Description = NormalizeDescription(description),
            ParentId = parentId,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

    public void Update(string name, string? description, bool active, long? parentId)
    {
        Name = ValidateName(name);
        Description = NormalizeDescription(description);
        Active = active;
        ParentId = parentId;
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome da categoria é obrigatório.");

        var trimmed = name.Trim();

        if (trimmed.Length < NameMinLength)
            throw new DomainException($"O nome deve ter no mínimo {NameMinLength} caracteres.");

        if (trimmed.Length > NameMaxLength)
            throw new DomainException($"O nome deve ter no máximo {NameMaxLength} caracteres.");

        return trimmed;
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        var trimmed = description.Trim();

        if (trimmed.Length > DescriptionMaxLength)
            throw new DomainException($"A descrição deve ter no máximo {DescriptionMaxLength} caracteres.");

        return trimmed;
    }
}
