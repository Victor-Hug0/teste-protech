using Teste.Application.Abstractions.Persistence;
using Teste.Application.Common.Mappings;
using Teste.Application.DTOs;
using Teste.Domain.Entities;

namespace Teste.Application.Features.Buyers.Create;

public sealed class CreateBuyerHandler(IUnitOfWork unitOfWork) : ICreateBuyerHandler
{
    public async Task<BuyerDto> HandleAsync(CreateBuyerCommand command, CancellationToken cancellationToken = default)
    {
        var buyer = Buyer.Create(command.Name, command.Email);

        await unitOfWork.Buyers.AddAsync(buyer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return buyer.ToDto();
    }
}