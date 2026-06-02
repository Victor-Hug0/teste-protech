using Application.Abstractions.Persistence;
using Application.Common.Mappings;
using Application.DTOs;
using Application.Exceptions;

namespace Application.Features.Todos.Update;

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
