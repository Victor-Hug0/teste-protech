using Teste.Application.DTOs;

namespace Teste.Application.Features.Todos.GetAll;

public interface IGetAllTodosHandler
{
    Task<IReadOnlyList<TodoDto>> HandleAsync(CancellationToken cancellationToken = default);
}
