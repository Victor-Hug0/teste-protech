using Teste.Application.DTOs;

namespace Teste.Application.Features.Todos.Complete;

public interface ICompleteTodoHandler
{
    Task<TodoDto> HandleAsync(Guid id, CancellationToken cancellationToken = default);
}
