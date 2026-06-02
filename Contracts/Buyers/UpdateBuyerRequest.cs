using System.ComponentModel.DataAnnotations;

namespace Teste.Contracts.Buyers;

public sealed record UpdateBuyerRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(150)]
    public required string Name { get; init; }

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public required string Email { get; init; }
}
