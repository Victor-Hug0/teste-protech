using Teste.Application.Abstractions.Persistence;
using Teste.Application.Common.Mappings;
using Teste.Application.DTOs;

namespace Teste.Application.Features.Todos.GetAll;

public sealed class GetAllTodosHandler(IUnitOfWork unitOfWork) : IGetAllTodosHandler
{
    public async Task<IReadOnlyList<TodoDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var todos = await unitOfWork.Todos.GetAllAsync(cancellationToken);
        return todos.Select(t => t.ToDto()).ToList();
    }
}
