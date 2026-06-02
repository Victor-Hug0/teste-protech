namespace Application.Features.Todos.Create;

public sealed record CreateTodoCommand(string Title, string? Description);
