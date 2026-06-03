using System.ComponentModel.DataAnnotations;
using Application.DTOs;
using Application.Orders;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Teste.Api.Contracts.Orders;

namespace Teste.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/orders")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1)
            .WithTags("Orders");

        group.MapGet("/", List)
            .WithName("ListOrders")
            .WithSummary("Lista pedidos com paginação e filtros (status, período de criação)")
            .Produces<PagedResult<OrderDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetOrderById")
            .WithSummary("Obtém um pedido por id")
            .Produces<OrderDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName("CreateOrder")
            .WithSummary("Cria um novo pedido")
            .Produces<OrderDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", UpdateStatus)
            .WithName("UpdateOrderStatus")
            .WithSummary("Avança o status do pedido (PROCESSADO ou ENVIADO)")
            .Produces<OrderDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPatch("/{id:guid}", UpdateDetails)
            .WithName("UpdateOrderDetails")
            .WithSummary("Altera comprador e itens (apenas pedido INICIADO)")
            .Produces<OrderDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost("/{id:guid}/cancel", Cancel)
            .WithName("CancelOrder")
            .WithSummary("Cancela um pedido iniciado ou processado")
            .Produces<OrderDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteOrder")
            .WithSummary("Remove um pedido iniciado")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> List(
        [AsParameters] ListOrdersQueryParameters query,
        IOrderService service,
        CancellationToken cancellationToken)
    {
        var filter = OrderListFilter.Create(
            query.Status,
            query.CreatedFrom,
            query.CreatedTo,
            query.Page,
            query.PageSize);

        var result = await service.ListAsync(filter, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(
        Guid id,
        IOrderService service,
        CancellationToken cancellationToken)
    {
        var order = await service.GetByIdAsync(id, cancellationToken);
        return Results.Ok(order);
    }

    private static async Task<IResult> Create(
        CreateOrderRequest request,
        IOrderService service,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null)
            return validationError;

        var order = await service.CreateAsync(
            request.BuyerId,
            ToLineInputs(request.Items),
            cancellationToken);

        var version = httpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var major = version.Split('.')[0];
        return Results.Created($"/api/v{major}/orders/{order.Id}", order);
    }

    private static async Task<IResult> UpdateStatus(
        Guid id,
        UpdateOrderRequest request,
        IOrderService service,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null)
            return validationError;

        var order = await service.UpdateStatusAsync(id, request.Status, cancellationToken);
        return Results.Ok(order);
    }

    private static async Task<IResult> UpdateDetails(
        Guid id,
        UpdateOrderDetailsRequest request,
        IOrderService service,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null)
            return validationError;

        var order = await service.UpdateDetailsAsync(
            id,
            request.BuyerId,
            ToLineInputs(request.Items),
            cancellationToken);

        return Results.Ok(order);
    }

    private static async Task<IResult> Cancel(
        Guid id,
        IOrderService service,
        CancellationToken cancellationToken)
    {
        var order = await service.CancelAsync(id, cancellationToken);
        return Results.Ok(order);
    }

    private static async Task<IResult> Delete(
        Guid id,
        IOrderService service,
        CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
    }

    private static IReadOnlyList<OrderLineInput> ToLineInputs(IReadOnlyList<OrderItemRequest> items) =>
        items.Select(i => new OrderLineInput(i.ProductId, i.Quantity)).ToList();

    private static IResult? Validate<T>(T instance) where T : class
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(instance);

        if (Validator.TryValidateObject(instance, context, validationResults, validateAllProperties: true))
            return null;

        var errors = validationResults
            .GroupBy(e => e.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage ?? "Valor inválido.").ToArray());

        return Results.ValidationProblem(errors);
    }
}
