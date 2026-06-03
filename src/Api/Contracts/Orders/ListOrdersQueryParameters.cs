namespace Teste.Api.Contracts.Orders;

public sealed class ListOrdersQueryParameters
{
    /// <summary>
    /// Um ou mais status separados por vírgula (ex.: INICIADO,PROCESSADO).
    /// </summary>
    public string? Status { get; init; }

    public DateTime? CreatedFrom { get; init; }

    public DateTime? CreatedTo { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}
