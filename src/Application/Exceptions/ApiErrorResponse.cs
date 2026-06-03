namespace Application.Exceptions;

public sealed record ApiErrorResponse(
    string Type,
    string Title,
    int Status,
    string Detail,
    string? Instance,
    string Code);
