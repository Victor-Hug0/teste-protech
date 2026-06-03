using Application.Abstractions.Persistence;
using Application.Common.Mappings;
using Application.DTOs;
using Application.Exceptions;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Orders;

public sealed class OrderService(
    IOrderRepository orders,
    IBuyerRepository buyers,
    IProductRepository products) : IOrderService
{
    public async Task<PagedResult<OrderDto>> ListAsync(
        OrderListFilter filter,
        CancellationToken cancellationToken = default)
    {
        if (filter.BuyerId.HasValue && !await buyers.ExistsAsync(filter.BuyerId.Value, cancellationToken))
            throw new NotFoundException(
                $"Comprador com id '{filter.BuyerId}' não encontrado.",
                BusinessRuleCodes.Buyer.NotFound);

        var (items, totalCount) = await orders.ListAsync(filter, cancellationToken);

        var dtos = items.Select(o => o.ToDto()).ToList();
        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)filter.PageSize);

        return new PagedResult<OrderDto>(
            dtos,
            filter.Page,
            filter.PageSize,
            totalCount,
            totalPages);
    }

    public async Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(
                $"Pedido com id '{id}' não encontrado.",
                BusinessRuleCodes.Order.NotFound);

        return order.ToDto();
    }

    public async Task<OrderDto> CreateAsync(
        Guid buyerId,
        IReadOnlyList<OrderLineInput> items,
        CancellationToken cancellationToken = default)
    {
        await EnsureBuyerExistsAsync(buyerId, cancellationToken);

        var orderItems = await BuildOrderItemsAsync(items, cancellationToken);
        var order = Order.Create(buyerId, orderItems);

        await orders.AddAsync(order, cancellationToken);
        await orders.SaveChangesAsync(cancellationToken);

        return (await orders.GetByIdAsync(order.Id, cancellationToken) ?? order).ToDto();
    }

    public async Task<OrderDto> UpdateStatusAsync(
        Guid id,
        string status,
        CancellationToken cancellationToken = default)
    {
        var order = await orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(
                $"Pedido com id '{id}' não encontrado.",
                BusinessRuleCodes.Order.NotFound);

        ApplyStatusTransition(order, status);

        await orders.SaveChangesAsync(cancellationToken);

        return (await orders.GetByIdAsync(id, cancellationToken) ?? order).ToDto();
    }

    public async Task<OrderDto> UpdateDetailsAsync(
        Guid id,
        Guid buyerId,
        IReadOnlyList<OrderLineInput> items,
        CancellationToken cancellationToken = default)
    {
        var order = await orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(
                $"Pedido com id '{id}' não encontrado.",
                BusinessRuleCodes.Order.NotFound);

        await EnsureBuyerExistsAsync(buyerId, cancellationToken);

        var orderItems = await BuildOrderItemsAsync(items, cancellationToken);
        order.UpdateDetails(buyerId, orderItems);

        await orders.SaveChangesAsync(cancellationToken);

        return (await orders.GetByIdAsync(id, cancellationToken) ?? order).ToDto();
    }

    public async Task<OrderDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(
                $"Pedido com id '{id}' não encontrado.",
                BusinessRuleCodes.Order.NotFound);

        order.Cancel();
        await orders.SaveChangesAsync(cancellationToken);

        return order.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(
                $"Pedido com id '{id}' não encontrado.",
                BusinessRuleCodes.Order.NotFound);

        order.EnsureCanDelete();
        orders.Remove(order);
        await orders.SaveChangesAsync(cancellationToken);
    }

    private static void ApplyStatusTransition(Order order, string status)
    {
        var normalized = status.Trim().ToUpperInvariant();

        switch (normalized)
        {
            case OrderStatus.Processado:
                order.Process();
                break;
            case OrderStatus.Enviado:
                order.Ship();
                break;
            default:
                throw new DomainException(
                    $"Transição de status inválida. Use '{OrderStatus.Processado}' ou '{OrderStatus.Enviado}'.",
                    BusinessRuleCodes.Order.InvalidStatusTransition);
        }
    }

    private async Task<IReadOnlyList<OrderItem>> BuildOrderItemsAsync(
        IReadOnlyList<OrderLineInput> lines,
        CancellationToken cancellationToken)
    {
        if (lines.Count == 0)
            throw new DomainException(
                "O pedido deve conter pelo menos um produto.",
                BusinessRuleCodes.Order.ItemsRequired);

        var distinctProductIds = lines.Select(l => l.ProductId).Distinct().ToList();

        if (distinctProductIds.Count != lines.Count)
            throw new DomainException(
                "O pedido não pode conter o mesmo produto mais de uma vez.",
                BusinessRuleCodes.Order.DuplicateProduct);

        var foundProducts = await products.GetByIdsForLinkAsync(distinctProductIds, cancellationToken);

        if (foundProducts.Count != distinctProductIds.Count)
            throw new DomainException(
                "Um ou mais produtos informados não existem.",
                BusinessRuleCodes.Order.ProductsNotFound);

        var productById = foundProducts.ToDictionary(p => p.Id);

        return lines
            .Select(line =>
            {
                var product = productById[line.ProductId];

                if (!product.Active)
                    throw new DomainException(
                        $"O produto '{product.Name}' está inativo.",
                        BusinessRuleCodes.Product.Inactive);

                return OrderItem.Create(line.ProductId, line.Quantity, product.Price);
            })
            .ToList();
    }

    private async Task EnsureBuyerExistsAsync(Guid buyerId, CancellationToken cancellationToken)
    {
        if (!await buyers.ExistsAsync(buyerId, cancellationToken))
            throw new DomainException(
                "O comprador informado não existe.",
                BusinessRuleCodes.Order.BuyerNotFound);
    }
}
