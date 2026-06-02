using Application.DTOs;

namespace Application.Features.Buyers.GetAll;

public interface IGetAllBuyersHandler
{
    Task<IReadOnlyList<BuyerDto>> HandleAsync(CancellationToken cancellationToken = default);
}