using Teste.Domain.Entities;

namespace Teste.Application.Abstractions.Persistence;

public interface IBuyerRepository
{
    Task<Buyer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Buyer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Buyer buyer, CancellationToken cancellationToken = default);
    void Update(Buyer buyer);
    void Remove(Buyer buyer);
}