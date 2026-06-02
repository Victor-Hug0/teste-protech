using Application.DTOs;

namespace Application.Features.Todos.Update;

public interface IUpdateTodoHandler
{
    Task<TodoDto> HandleAsync(UpdateTodoCommand command, CancellationToken cancellationToken = default);
}
