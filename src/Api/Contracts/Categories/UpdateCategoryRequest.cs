using System.ComponentModel.DataAnnotations;

namespace Teste.Api.Contracts.Categories;

public sealed record UpdateCategoryRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public required string Name { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    public long? ParentId { get; init; }

    public bool Active { get; init; } = true;
}
