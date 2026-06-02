using Application.DTOs;
using Domain.Entities;

namespace Application.Common.Mappings;

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
