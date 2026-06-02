using Application.DTOs;

namespace Application.Features.Buyers.Create;

public interface ICreateBuyerHandler
{
    Task<BuyerDto> HandleAsync(CreateBuyerCommand command, CancellationToken cancellationToken = default);
}