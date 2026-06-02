using Application.Abstractions.Persistence;
using Application.Common.Mappings;
using Application.DTOs;
using Application.Exceptions;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Buyers;

public sealed class BuyerService(IBuyerRepository buyers) : IBuyerService
{
    public async Task<IReadOnlyList<BuyerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await buyers.GetAllAsync(cancellationToken);
        return list.Select(b => b.ToDto()).ToList();
    }

    public async Task<BuyerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var buyer = await buyers.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Comprador com id '{id}' não encontrado.");

        return buyer.ToDto();
    }

    public async Task<BuyerDto> CreateAsync(
        string name,
        string email,
        CancellationToken cancellationToken = default)
    {
        await EnsureEmailIsAvailableAsync(email, excludeBuyerId: null, cancellationToken);

        var buyer = Buyer.Create(name, email);

        await buyers.AddAsync(buyer, cancellationToken);
        await buyers.SaveChangesAsync(cancellationToken);

        return buyer.ToDto();
    }

    private async Task EnsureEmailIsAvailableAsync(
        string email,
        Guid? excludeBuyerId,
        CancellationToken cancellationToken)
    {
        if (await buyers.ExistsByEmailAsync(email, excludeBuyerId, cancellationToken))
            throw new DomainException("Já existe um comprador com este e-mail.");
    }
}
