using Teste.Application.DTOs;
using Teste.Domain.Entities;

namespace Teste.Application.Common.Mappings;

public static class TodoMapper
{
    public static TodoDto ToDto(this TodoItem todo) =>
        new(
            todo.Id,
            todo.Title,
            todo.Description,
            todo.IsCompleted,
            todo.CreatedAt,
            todo.CompletedAt);
}
