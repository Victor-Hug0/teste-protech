namespace Application.DTOs;

public sealed record ProductDto(
    long Id,
    string Name,
    string? Description,
    decimal Price,
    string Brand,
    string Color,
    bool Active,
    DateTime CreatedAt,
    IReadOnlyList<CategoryDto> Categories);
