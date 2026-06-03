namespace Application.DTOs;

public sealed record OrderItemDto(
    long Id,
    long ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
