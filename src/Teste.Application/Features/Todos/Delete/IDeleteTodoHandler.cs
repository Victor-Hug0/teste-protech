namespace Teste.Application.Features.Todos.Delete;

public interface IDeleteTodoHandler
{
    Task HandleAsync(Guid id, CancellationToken cancellationToken = default);
}
