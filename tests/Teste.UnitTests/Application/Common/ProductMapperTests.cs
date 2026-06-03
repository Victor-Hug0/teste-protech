using Application.Common.Mappings;
using Domain.Entities;
using FluentAssertions;

namespace Teste.UnitTests.Application.Common;

public sealed class ProductMapperTests
{
    [Fact]
    public void ToDto_ShouldMapAllPropertiesIncludingCategories()
    {
        var product = Product.Create("Notebook", 1999.99m, "TechBrand", "Prata", "Descrição");
        product.SetCategories([Category.Create("Eletrônicos"), Category.Create("Informática")]);
        product.Update("Notebook", 2499.99m, "TechBrand", "Preto", "Descrição", active: false);

        var dto = product.ToDto();

        dto.Name.Should().Be("Notebook");
        dto.Price.Should().Be(2499.99m);
        dto.Active.Should().BeFalse();
        dto.Categories.Should().HaveCount(2);
        dto.Categories.Select(c => c.Name).Should().BeEquivalentTo("Eletrônicos", "Informática");
    }
}
