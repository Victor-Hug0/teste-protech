using Teste.Application.DTOs;

namespace Teste.Application.Features.Buyers.Create;

public interface ICreateBuyerHandler
{
    Task<BuyerDto> HandleAsync(CreateBuyerCommand command, CancellationToken cancellationToken = default);
}