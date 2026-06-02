using Teste.Application.Abstractions.Persistence;
using Teste.Application.Exceptions;

namespace Teste.Application.Features.Todos.Delete;

public sealed class DeleteTodoHandler(IUnitOfWork unitOfWork) : IDeleteTodoHandler
{
    public async Task HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var todo = await unitOfWork.Todos.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Tarefa com id '{id}' não encontrada.");

        unitOfWork.Todos.Remove(todo);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
