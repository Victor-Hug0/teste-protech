using Application.DTOs;

namespace Application.Features.Todos.Create;

public interface ICreateTodoHandler
{
    Task<TodoDto> HandleAsync(CreateTodoCommand command, CancellationToken cancellationToken = default);
}
