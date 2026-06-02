using Teste.Application.DTOs;
using Teste.Domain.Entities;

namespace Teste.Application.Common.Mappings;

public static class BuyerMapper
{
    public static BuyerDto ToDto(this Buyer buyer) =>
        new(
            buyer.Id,
            buyer.Name,
            buyer.Email,
            buyer.CreatedAt);
}