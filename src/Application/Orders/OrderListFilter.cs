using Domain.Enums;
using Domain.Exceptions;

namespace Application.Orders;

public sealed record OrderListFilter
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    public Guid? BuyerId { get; init; }
    public IReadOnlyList<string>? Statuses { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }

    public static OrderListFilter Create(
        string? status,
        DateTime? createdFrom,
        DateTime? createdTo,
        int page,
        int pageSize,
        Guid? buyerId = null)
    {
        var normalizedPage = page <= 0 ? DefaultPage : page;
        var normalizedPageSize = pageSize <= 0 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        if (createdFrom.HasValue && createdTo.HasValue && createdFrom > createdTo)
            throw new DomainException("'createdFrom' não pode ser maior que 'createdTo'.");

        return new OrderListFilter
        {
            BuyerId = buyerId,
            Statuses = ParseStatuses(status),
            CreatedFrom = createdFrom,
            CreatedTo = NormalizeCreatedTo(createdTo),
            Page = normalizedPage,
            PageSize = normalizedPageSize
        };
    }

    private static IReadOnlyList<string>? ParseStatuses(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        var statuses = status
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => s.ToUpperInvariant())
            .Distinct()
            .ToList();

        foreach (var value in statuses)
        {
            if (!OrderStatus.All.Contains(value))
                throw new DomainException(
                    $"Status '{value}' inválido. Valores permitidos: INICIADO, PROCESSADO, ENVIADO, CANCELADO.");
        }

        return statuses;
    }

    private static DateTime? NormalizeCreatedTo(DateTime? createdTo)
    {
        if (!createdTo.HasValue)
            return null;

        return createdTo.Value.TimeOfDay == TimeSpan.Zero
            ? createdTo.Value.Date.AddDays(1).AddTicks(-1)
            : createdTo.Value;
    }
}
