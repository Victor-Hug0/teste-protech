using Teste.Application.DTOs;

namespace Teste.Application.Features.Todos.GetById;

public interface IGetTodoByIdHandler
{
    Task<TodoDto> HandleAsync(Guid id, CancellationToken cancellationToken = default);
}
