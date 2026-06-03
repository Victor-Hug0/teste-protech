using Application.DTOs;
using Domain.Entities;

namespace Application.Common.Mappings;

public static class ProductMapper
{
    public static ProductDto ToDto(this Product product) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Brand,
            product.Color,
            product.Active,
            product.CreatedAt,
            product.Categories.Select(c => c.ToDto()).ToList());
}
