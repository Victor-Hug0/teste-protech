using Application.Abstractions.Persistence;
using Application.Common.Mappings;
using Application.DTOs;

namespace Application.Features.Todos.GetAll;

public sealed class GetAllTodosHandler(IUnitOfWork unitOfWork) : IGetAllTodosHandler
{
    public async Task<IReadOnlyList<TodoDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var todos = await unitOfWork.Todos.GetAllAsync(cancellationToken);
        return todos.Select(t => t.ToDto()).ToList();
    }
}
