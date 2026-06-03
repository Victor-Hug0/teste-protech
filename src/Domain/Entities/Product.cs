using Domain.Common;
using Domain.Exceptions;

namespace Domain.Entities;

public sealed class Product : LongEntity
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 150;
    public const int BrandMaxLength = 150;
    public const int ColorMaxLength = 150;
    public const int DescriptionMaxLength = 1000;

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public string Brand { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;
    public bool Active { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();

    private Product() { }

    public void SetCategories(IReadOnlyCollection<Category> categories)
    {
        ValidateCategories(categories);
        Categories.Clear();
        foreach (var category in categories)
            Categories.Add(category);
    }

    public static Product Create(
        string name,
        decimal price,
        string brand,
        string color,
        string? description = null) =>
        new()
        {
            Name = ValidateName(name),
            Description = NormalizeDescription(description),
            Price = ValidatePrice(price),
            Brand = ValidateBrand(brand),
            Color = ValidateColor(color),
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

    public void Update(
        string name,
        decimal price,
        string brand,
        string color,
        string? description,
        bool active)
    {
        Name = ValidateName(name);
        Description = NormalizeDescription(description);
        Price = ValidatePrice(price);
        Brand = ValidateBrand(brand);
        Color = ValidateColor(color);
        Active = active;
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome do produto é obrigatório.");

        var trimmed = name.Trim();

        if (trimmed.Length < NameMinLength)
            throw new DomainException($"O nome deve ter no mínimo {NameMinLength} caracteres.");

        if (trimmed.Length > NameMaxLength)
            throw new DomainException($"O nome deve ter no máximo {NameMaxLength} caracteres.");

        return trimmed;
    }

    private static string ValidateBrand(string brand)
    {
        if (string.IsNullOrWhiteSpace(brand))
            throw new DomainException("A marca é obrigatória.");

        var trimmed = brand.Trim();

        if (trimmed.Length > BrandMaxLength)
            throw new DomainException($"A marca deve ter no máximo {BrandMaxLength} caracteres.");

        return trimmed;
    }

    private static string ValidateColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            throw new DomainException("A cor é obrigatória.");

        var trimmed = color.Trim();

        if (trimmed.Length > ColorMaxLength)
            throw new DomainException($"A cor deve ter no máximo {ColorMaxLength} caracteres.");

        return trimmed;
    }

    private static decimal ValidatePrice(decimal price)
    {
        if (price < 0)
            throw new DomainException("O preço não pode ser negativo.");

        return price;
    }

    private static void ValidateCategories(IReadOnlyCollection<Category> categories)
    {
        if (categories.Count == 0)
            throw new DomainException("O produto deve pertencer a pelo menos uma categoria.");
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
