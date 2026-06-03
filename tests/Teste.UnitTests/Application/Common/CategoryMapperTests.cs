using Application.Common.Mappings;
using Domain.Entities;
using FluentAssertions;

namespace Teste.UnitTests.Application.Common;

public sealed class CategoryMapperTests
{
    [Fact]
    public void ToDto_ShouldMapAllProperties()
    {
        var category = Category.Create("Eletrônicos", "Smartphones", parentId: 5);
        category.Update("Eletrônicos", "Smartphones", active: false, parentId: 5);

        var dto = category.ToDto();

        dto.Id.Should().Be(category.Id);
        dto.Name.Should().Be("Eletrônicos");
        dto.Description.Should().Be("Smartphones");
        dto.ParentId.Should().Be(5);
        dto.Active.Should().BeFalse();
        dto.CreatedAt.Should().Be(category.CreatedAt);
    }
}
