using Teste.Application.DTOs;

namespace Teste.Application.Features.Todos.Update;

public interface IUpdateTodoHandler
{
    Task<TodoDto> HandleAsync(UpdateTodoCommand command, CancellationToken cancellationToken = default);
}
