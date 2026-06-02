using Application.Abstractions.Persistence;
using Application.Common.Mappings;
using Application.DTOs;
using Domain.Entities;

namespace Application.Features.Buyers.Create;

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