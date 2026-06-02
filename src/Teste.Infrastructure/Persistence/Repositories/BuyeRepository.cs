using Microsoft.EntityFrameworkCore;
using Teste.Application.Abstractions.Persistence;
using Teste.Domain.Entities;

namespace Teste.Infrastructure.Persistence.Repositories;

public sealed class BuyerRepository(ApplicationDbContext context) : IBuyerRepository
{
    public async Task<Buyer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Buyers.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Buyer>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Buyers.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(Buyer buyer, CancellationToken cancellationToken = default) =>
        await context.Buyers.AddAsync(buyer, cancellationToken);

    public void Update(Buyer buyer) => context.Buyers.Update(buyer);

    public void Remove(Buyer buyer) => context.Buyers.Remove(buyer);
}