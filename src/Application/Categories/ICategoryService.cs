using Application.DTOs;

namespace Application.Categories;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(
        string name,
        string? description,
        long? parentId,
        CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateAsync(
        long id,
        string name,
        string? description,
        bool active,
        long? parentId,
        CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
