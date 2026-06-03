using Application.DTOs;

namespace Application.Products;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductDto> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(
        string name,
        decimal price,
        string brand,
        string color,
        string? description,
        IReadOnlyList<long> categoryIds,
        CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(
        long id,
        string name,
        decimal price,
        string brand,
        string color,
        string? description,
        bool active,
        IReadOnlyList<long> categoryIds,
        CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
