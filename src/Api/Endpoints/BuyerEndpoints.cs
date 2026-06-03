using System.ComponentModel.DataAnnotations;
using Application.Buyers;
using Application.DTOs;
using Application.Orders;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Teste.Api.Contracts.Buyers;
using Teste.Api.Contracts.Orders;

namespace Teste.Api.Endpoints;

public static class BuyerEndpoints
{
    public static IEndpointRouteBuilder MapBuyerEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/buyers")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1)
            .WithTags("Buyers");

        group.MapGet("/", GetAll)
            .WithName("GetAllBuyers")
            .WithSummary("Lista todos os compradores")
            .Produces<IReadOnlyList<BuyerDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetBuyerById")
            .WithSummary("Obtém um comprador por id")
            .Produces<BuyerDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/orders", GetOrders)
            .WithName("GetBuyerOrders")
            .WithSummary("Lista os pedidos de um comprador")
            .Produces<PagedResult<OrderDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost("/", Create)
            .WithName("CreateBuyer")
            .WithSummary("Cria um novo comprador")
            .Produces<BuyerDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        return app;
    }

    private static async Task<IResult> GetAll(
        IBuyerService service,
        CancellationToken cancellationToken)
    {
        var buyers = await service.GetAllAsync(cancellationToken);
        return Results.Ok(buyers);
    }

    private static async Task<IResult> GetById(
        Guid id,
        IBuyerService service,
        CancellationToken cancellationToken)
    {
        var buyer = await service.GetByIdAsync(id, cancellationToken);
        return Results.Ok(buyer);
    }

    private static async Task<IResult> GetOrders(
        Guid id,
        [AsParameters] ListOrdersQueryParameters query,
        IOrderService orderService,
        CancellationToken cancellationToken)
    {
        var filter = OrderListFilter.Create(
            query.Status,
            query.CreatedFrom,
            query.CreatedTo,
            query.Page,
            query.PageSize,
            buyerId: id);

        var result = await orderService.ListAsync(filter, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        CreateBuyerRequest request,
        IBuyerService service,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null)
            return validationError;

        var buyer = await service.CreateAsync(request.Name, request.Email, cancellationToken);

        var version = httpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var major = version.Split('.')[0];
        return Results.Created($"/api/v{major}/buyers/{buyer.Id}", buyer);
    }

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
