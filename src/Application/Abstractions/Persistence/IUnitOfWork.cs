namespace Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    ITodoRepository Todos { get; }
    IBuyerRepository Buyers { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
