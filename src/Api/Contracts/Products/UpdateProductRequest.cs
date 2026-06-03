using System.ComponentModel.DataAnnotations;

namespace Teste.Api.Contracts.Products;

public sealed record UpdateProductRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(150)]
    public required string Name { get; init; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; init; }

    [Required]
    [MaxLength(150)]
    public required string Brand { get; init; }

    [Required]
    [MaxLength(150)]
    public required string Color { get; init; }

    [MaxLength(1000)]
    public string? Description { get; init; }

    public bool Active { get; init; } = true;

    [Required]
    [MinLength(1, ErrorMessage = "O produto deve pertencer a pelo menos uma categoria.")]
    public required IReadOnlyList<long> CategoryIds { get; init; }
}
