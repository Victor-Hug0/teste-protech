using Microsoft.EntityFrameworkCore;
using Teste.Application.Abstractions.Persistence;
using Teste.Domain.Entities;

namespace Teste.Infrastructure.Persistence.Repositories;

public sealed class TodoRepository(ApplicationDbContext context) : ITodoRepository
{
    public async Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.TodoItems.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.TodoItems
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(TodoItem todo, CancellationToken cancellationToken = default) =>
        await context.TodoItems.AddAsync(todo, cancellationToken);

    public void Update(TodoItem todo) => context.TodoItems.Update(todo);

    public void Remove(TodoItem todo) => context.TodoItems.Remove(todo);
}
