using System.ComponentModel.DataAnnotations;
using Application.Buyers;
using Application.DTOs;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Teste.Contracts.Buyers;

namespace Teste.Endpoints;

public static class BuyerEndpoints
{
    public static IEndpointRouteBuilder MapBuyerEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        MapVersion(app, versionSet, 1, "v1");
        MapVersion(app, versionSet, 2, "v2");
        return app;
    }

    private static void MapVersion(
        IEndpointRouteBuilder app,
        ApiVersionSet versionSet,
        int majorVersion,
        string tagSuffix)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/buyers")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(majorVersion)
            .WithTags($"Buyers {tagSuffix}");

        group.MapGet("/", GetAll)
            .WithName($"GetAllBuyers{tagSuffix}")
            .WithSummary("Lista todos os compradores")
            .Produces<IReadOnlyList<BuyerDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetById)
            .WithName($"GetBuyerById{tagSuffix}")
            .WithSummary("Obtém um comprador por id")
            .Produces<BuyerDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName($"CreateBuyer{tagSuffix}")
            .WithSummary("Cria um novo comprador")
            .Produces<BuyerDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", Update)
            .WithName($"UpdateBuyer{tagSuffix}")
            .WithSummary("Atualiza um comprador")
            .Produces<BuyerDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", Delete)
            .WithName($"DeleteBuyer{tagSuffix}")
            .WithSummary("Remove um comprador")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
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

    private static async Task<IResult> Update(
        Guid id,
        UpdateBuyerRequest request,
        IBuyerService service,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null)
            return validationError;

        var buyer = await service.UpdateAsync(id, request.Name, request.Email, cancellationToken);
        return Results.Ok(buyer);
    }

    private static async Task<IResult> Delete(
        Guid id,
        IBuyerService service,
        CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Results.NoContent();
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
