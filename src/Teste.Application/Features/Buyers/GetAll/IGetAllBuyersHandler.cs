using Teste.Application.DTOs;

namespace Teste.Application.Features.Buyers.GetAll;

public interface IGetAllBuyersHandler
{
    Task<IReadOnlyList<BuyerDto>> HandleAsync(CancellationToken cancellationToken = default);
}