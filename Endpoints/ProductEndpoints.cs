using System.ComponentModel.DataAnnotations;
using Application.DTOs;
using Application.Products;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Teste.Contracts.Products;

namespace Teste.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/products")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1)
            .WithTags("Products");

        group.MapGet("/", GetAll)
            .WithName("GetAllProducts")
            .WithSummary("Lista todos os produtos")
            .Produces<IReadOnlyList<ProductDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:long}", GetById)
            .WithName("GetProductById")
            .WithSummary("Obtém um produto por id")
            .Produces<ProductDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName("CreateProduct")
            .WithSummary("Cria um novo produto")
            .Produces<ProductDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:long}", Update)
            .WithName("UpdateProduct")
            .WithSummary("Atualiza um produto")
            .Produces<ProductDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:long}", Delete)
            .WithName("DeleteProduct")
            .WithSummary("Remove um produto")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetAll(
        IProductService service,
        CancellationToken cancellationToken)
    {
        var products = await service.GetAllAsync(cancellationToken);
        return Results.Ok(products);
    }

    private static async Task<IResult> GetById(
        long id,
        IProductService service,
        CancellationToken cancellationToken)
    {
        var product = await service.GetByIdAsync(id, cancellationToken);
        return Results.Ok(product);
    }

    private static async Task<IResult> Create(
        CreateProductRequest request,
        IProductService service,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null)
            return validationError;

        var product = await service.CreateAsync(
            request.Name,
            request.Price,
            request.Brand,
            request.Color,
            request.Description,
            request.CategoryIds,
            cancellationToken);

        var version = httpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var major = version.Split('.')[0];
        return Results.Created($"/api/v{major}/products/{product.Id}", product);
    }

    private static async Task<IResult> Update(
        long id,
        UpdateProductRequest request,
        IProductService service,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null)
            return validationError;

        var product = await service.UpdateAsync(
            id,
            request.Name,
            request.Price,
            request.Brand,
            request.Color,
            request.Description,
            request.Active,
            request.CategoryIds,
            cancellationToken);

        return Results.Ok(product);
    }

    private static async Task<IResult> Delete(
        long id,
        IProductService service,
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
