namespace Application.DTOs;

public sealed record CategoryDto(
    long Id,
    string Name,
    string? Description,
    long? ParentId,
    bool Active,
    DateTime CreatedAt);
