using System.ComponentModel.DataAnnotations;
using Application.Categories;
using Application.DTOs;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Teste.Contracts.Categories;

namespace Teste.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/categories")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1)
            .WithTags("Categories");

        group.MapGet("/", GetAll)
            .WithName("GetAllCategories")
            .WithSummary("Lista todas as categorias")
            .Produces<IReadOnlyList<CategoryDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:long}", GetById)
            .WithName("GetCategoryById")
            .WithSummary("Obtém uma categoria por id")
            .Produces<CategoryDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName("CreateCategory")
            .WithSummary("Cria uma nova categoria")
            .Produces<CategoryDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:long}", Update)
            .WithName("UpdateCategory")
            .WithSummary("Atualiza uma categoria")
            .Produces<CategoryDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:long}", Delete)
            .WithName("DeleteCategory")
            .WithSummary("Remove uma categoria")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetAll(
        ICategoryService service,
        CancellationToken cancellationToken)
    {
        var categories = await service.GetAllAsync(cancellationToken);
        return Results.Ok(categories);
    }

    private static async Task<IResult> GetById(
        long id,
        ICategoryService service,
        CancellationToken cancellationToken)
    {
        var category = await service.GetByIdAsync(id, cancellationToken);
        return Results.Ok(category);
    }

    private static async Task<IResult> Create(
        CreateCategoryRequest request,
        ICategoryService service,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null)
            return validationError;

        var category = await service.CreateAsync(
            request.Name,
            request.Description,
            request.ParentId,
            cancellationToken);

        var version = httpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var major = version.Split('.')[0];
        return Results.Created($"/api/v{major}/categories/{category.Id}", category);
    }

    private static async Task<IResult> Update(
        long id,
        UpdateCategoryRequest request,
        ICategoryService service,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null)
            return validationError;

        var category = await service.UpdateAsync(
            id,
            request.Name,
            request.Description,
            request.Active,
            request.ParentId,
            cancellationToken);

        return Results.Ok(category);
    }

    private static async Task<IResult> Delete(
        long id,
        ICategoryService service,
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
