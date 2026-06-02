using Application.Abstractions.Persistence;
using Application.Common.Mappings;
using Application.DTOs;
using Application.Exceptions;

namespace Application.Features.Todos.Complete;

public sealed class CompleteTodoHandler(IUnitOfWork unitOfWork) : ICompleteTodoHandler
{
    public async Task<TodoDto> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var todo = await unitOfWork.Todos.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Tarefa com id '{id}' não encontrada.");

        todo.Complete();

        unitOfWork.Todos.Update(todo);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return todo.ToDto();
    }
}
