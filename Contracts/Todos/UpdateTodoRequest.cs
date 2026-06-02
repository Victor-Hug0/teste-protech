using System.ComponentModel.DataAnnotations;

namespace Teste.Contracts.Todos;

public sealed record UpdateTodoRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(200)]
    public required string Title { get; init; }

    [MaxLength(1000)]
    public string? Description { get; init; }
}
