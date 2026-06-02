using Application.Abstractions.Persistence;
using Application.Common.Mappings;
using Application.DTOs;
using Domain.Entities;

namespace Application.Features.Todos.Create;

public sealed class CreateTodoHandler(IUnitOfWork unitOfWork) : ICreateTodoHandler
{
    public async Task<TodoDto> HandleAsync(
        CreateTodoCommand command,
        CancellationToken cancellationToken = default)
    {
        var todo = TodoItem.Create(command.Title, command.Description);

        await unitOfWork.Todos.AddAsync(todo, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return todo.ToDto();
    }
}
