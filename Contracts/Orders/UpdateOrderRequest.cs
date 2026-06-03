using System.ComponentModel.DataAnnotations;

namespace Teste.Contracts.Orders;

public sealed record UpdateOrderRequest
{
    [Required]
    [RegularExpression(
        "^(PROCESSADO|ENVIADO)$",
        ErrorMessage = "Status inválido. Use PROCESSADO ou ENVIADO.")]
    public required string Status { get; init; }
}
