using Application.Abstractions.Persistence;
using Application.Common.Mappings;
using Application.DTOs;
using Application.Exceptions;

namespace Application.Features.Todos.GetById;

public sealed class GetTodoByIdHandler(IUnitOfWork unitOfWork) : IGetTodoByIdHandler
{
    public async Task<TodoDto> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var todo = await unitOfWork.Todos.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Tarefa com id '{id}' não encontrada.");

        return todo.ToDto();
    }
}
