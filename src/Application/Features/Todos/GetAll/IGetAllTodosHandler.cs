using Application.DTOs;

namespace Application.Features.Todos.GetAll;

public interface IGetAllTodosHandler
{
    Task<IReadOnlyList<TodoDto>> HandleAsync(CancellationToken cancellationToken = default);
}
