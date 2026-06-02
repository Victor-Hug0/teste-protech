namespace Teste.Application.DTOs;

public sealed record BuyerDto(
    Guid Id,
    string Name,
    string Email,
    DateTime CreatedAt);