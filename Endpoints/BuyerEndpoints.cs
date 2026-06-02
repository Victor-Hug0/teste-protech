using Asp.Versioning;
using Asp.Versioning.Builder;
using Teste.Application.Features.Buyers.GetAll;
using Teste.Application.DTOs;
using Teste.Application.Features.Buyers.Create;
using System.ComponentModel.DataAnnotations;
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

        group.MapPost("/", Create)
            .WithName($"CreateBuyer{tagSuffix}")
            .WithSummary("Cria um novo comprador")
            .Produces<BuyerDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> GetAll(
        IGetAllBuyersHandler handler,
        CancellationToken cancellationToken)
    {
        var buyers = await handler.HandleAsync(cancellationToken);
        return Results.Ok(buyers);
    }
    
    private static async Task<IResult> Create(
        CreateBuyerRequest request,
        ICreateBuyerHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null)
            return validationError;

        var buyer = await handler.HandleAsync(
            new CreateBuyerCommand(request.Name, request.Email),
            cancellationToken);

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