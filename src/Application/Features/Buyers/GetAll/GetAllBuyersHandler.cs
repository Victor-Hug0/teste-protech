using Application.Abstractions.Persistence;
using Application.Common.Mappings;
using Application.DTOs;

namespace Application.Features.Buyers.GetAll;

public sealed class GetAllBuyersHandler(IUnitOfWork unitOfWork) : IGetAllBuyersHandler
{
    public async Task<IReadOnlyList<BuyerDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var buyers = await unitOfWork.Buyers.GetAllAsync(cancellationToken);
        return buyers.Select(b => b.ToDto()).ToList();
    }
}