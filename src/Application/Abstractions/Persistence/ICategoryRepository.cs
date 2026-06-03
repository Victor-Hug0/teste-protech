using Domain.Entities;

namespace Application.Abstractions.Persistence;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(
        string name,
        long? excludeCategoryId = null,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> HasChildrenAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> HasProductsAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetByIdsForLinkAsync(
        IReadOnlyList<long> ids,
        CancellationToken cancellationToken = default);
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
    void Update(Category category);
    void Remove(Category category);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
