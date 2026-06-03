using Domain.Entities;

namespace Application.Abstractions.Persistence;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(
        string name,
        long? excludeProductId = null,
        CancellationToken cancellationToken = default);
    Task<bool> IsUsedInOrdersAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetByIdsForLinkAsync(
        IReadOnlyList<long> ids,
        CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    void Update(Product product);
    void Remove(Product product);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
