using Teste.Application.Abstractions.Persistence;
using Teste.Application.Common.Mappings;
using Teste.Application.DTOs;

namespace Teste.Application.Features.Buyers.GetAll;

public sealed class GetAllBuyersHandler(IUnitOfWork unitOfWork) : IGetAllBuyersHandler
{
    public async Task<IReadOnlyList<BuyerDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var buyers = await unitOfWork.Buyers.GetAllAsync(cancellationToken);
        return buyers.Select(b => b.ToDto()).ToList();
    }
}