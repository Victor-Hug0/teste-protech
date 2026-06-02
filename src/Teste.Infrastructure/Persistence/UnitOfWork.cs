using Teste.Application.Abstractions.Persistence;

namespace Teste.Infrastructure.Persistence;

public sealed class UnitOfWork(
    ApplicationDbContext context,
    ITodoRepository todoRepository,
    IBuyerRepository buyerRepository) : IUnitOfWork
{
    public ITodoRepository Todos { get; } = todoRepository;

    public IBuyerRepository Buyers { get; } = buyerRepository;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
