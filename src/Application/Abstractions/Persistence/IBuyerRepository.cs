using Domain.Entities;

namespace Application.Abstractions.Persistence;

public interface IBuyerRepository
{
    Task<Buyer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Buyer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(
        string email,
        Guid? excludeBuyerId = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(Buyer buyer, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
