using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Teste.Application.DTOs;
using Teste.Application.Features.Todos.Complete;
using Teste.Application.Features.Todos.Create;
using Teste.Application.Features.Todos.Delete;
using Teste.Application.Features.Todos.GetAll;
using Teste.Application.Features.Todos.GetById;
using Teste.Application.Features.Todos.Update;
using Teste.Contracts.Todos;

namespace Teste.Endpoints;

public static class TodoEndpoints
{
    public static IEndpointRouteBuilder MapTodoEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
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
        var group = app.MapGroup("/api/v{version:apiVersion}/todos")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(majorVersion)
            .WithTags($"Todos {tagSuffix}");

        group.MapGet("/", GetAll)
            .WithName($"GetAllTodos{tagSuffix}")
            .WithSummary("Lista todas as tarefas")
            .Produces<IReadOnlyList<TodoDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetById)
            .WithName($"GetTodoById{tagSuffix}")
            .WithSummary("Obtém uma tarefa por id")
            .Produces<TodoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName($"CreateTodo{tagSuffix}")
            .WithSummary("Cria uma nova tarefa")
            .Produces<TodoDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", Update)
            .WithName($"UpdateTodo{tagSuffix}")
            .WithSummary("Atualiza uma tarefa")
            .Produces<TodoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPatch("/{id:guid}/complete", Complete)
            .WithName($"CompleteTodo{tagSuffix}")
            .WithSummary("Marca uma tarefa como concluída")
            .Produces<TodoDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", Delete)
            .WithName($"DeleteTodo{tagSuffix}")
            .WithSummary("Remove uma tarefa")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAll(
        IGetAllTodosHandler handler,
        CancellationToken cancellationToken)
    {
        var todos = await handler.HandleAsync(cancellationToken);
        return Results.Ok(todos);
    }

    private static async Task<IResult> GetById(
        Guid id,
        IGetTodoByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var todo = await handler.HandleAsync(id, cancellationToken);
        return Results.Ok(todo);
    }

    private static async Task<IResult> Create(
        CreateTodoRequest request,
        ICreateTodoHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null)
            return validationError;

        var todo = await handler.HandleAsync(
            new CreateTodoCommand(request.Title, request.Description),
            cancellationToken);

        var version = httpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var major = version.Split('.')[0];
        return Results.Created($"/api/v{major}/todos/{todo.Id}", todo);
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateTodoRequest request,
        IUpdateTodoHandler handler,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null)
            return validationError;

        var todo = await handler.HandleAsync(
            new UpdateTodoCommand(id, request.Title, request.Description),
            cancellationToken);

        return Results.Ok(todo);
    }

    private static async Task<IResult> Complete(
        Guid id,
        ICompleteTodoHandler handler,
        CancellationToken cancellationToken)
    {
        var todo = await handler.HandleAsync(id, cancellationToken);
        return Results.Ok(todo);
    }

    private static async Task<IResult> Delete(
        Guid id,
        IDeleteTodoHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(id, cancellationToken);
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
