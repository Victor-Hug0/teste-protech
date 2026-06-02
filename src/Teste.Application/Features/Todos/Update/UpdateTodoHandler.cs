using Teste.Application.Abstractions.Persistence;
using Teste.Application.Common.Mappings;
using Teste.Application.DTOs;
using Teste.Application.Exceptions;

namespace Teste.Application.Features.Todos.Update;

public sealed class UpdateTodoHandler(IUnitOfWork unitOfWork) : IUpdateTodoHandler
{
    public async Task<TodoDto> HandleAsync(
        UpdateTodoCommand command,
        CancellationToken cancellationToken = default)
    {
        var todo = await unitOfWork.Todos.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException($"Tarefa com id '{command.Id}' não encontrada.");

        todo.Update(command.Title, command.Description);

        unitOfWork.Todos.Update(todo);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return todo.ToDto();
    }
}
