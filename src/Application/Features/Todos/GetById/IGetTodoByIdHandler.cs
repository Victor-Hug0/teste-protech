using Application.DTOs;

namespace Application.Features.Todos.GetById;

public interface IGetTodoByIdHandler
{
    Task<TodoDto> HandleAsync(Guid id, CancellationToken cancellationToken = default);
}
