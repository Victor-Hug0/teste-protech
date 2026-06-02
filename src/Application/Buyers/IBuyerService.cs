using Application.DTOs;

namespace Application.Buyers;

public interface IBuyerService
{
    Task<IReadOnlyList<BuyerDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BuyerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BuyerDto> CreateAsync(string name, string email, CancellationToken cancellationToken = default);
    Task<BuyerDto> UpdateAsync(Guid id, string name, string email, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
