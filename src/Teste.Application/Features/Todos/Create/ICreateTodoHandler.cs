using Teste.Application.DTOs;

namespace Teste.Application.Features.Todos.Create;

public interface ICreateTodoHandler
{
    Task<TodoDto> HandleAsync(CreateTodoCommand command, CancellationToken cancellationToken = default);
}
