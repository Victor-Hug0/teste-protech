using Application.DTOs;
using Domain.Entities;

namespace Application.Common.Mappings;

public static class CategoryMapper
{
    public static CategoryDto ToDto(this Category category) =>
        new(
            category.Id,
            category.Name,
            category.Description,
            category.ParentId,
            category.Active,
            category.CreatedAt);
}
