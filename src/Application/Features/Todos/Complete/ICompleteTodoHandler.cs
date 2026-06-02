using Application.DTOs;

namespace Application.Features.Todos.Complete;

public interface ICompleteTodoHandler
{
    Task<TodoDto> HandleAsync(Guid id, CancellationToken cancellationToken = default);
}
