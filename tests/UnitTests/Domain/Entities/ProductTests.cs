using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;

namespace Teste.UnitTests.Domain.Entities;

public sealed class ProductTests
{
    [Fact]
    public void Create_WithValidData_ShouldSetProperties()
    {
        var product = Product.Create(
            "  Notebook Pro  ",
            1999.99m,
            "  TechBrand  ",
            "  Prata  ",
            "  Descrição do produto  ");

        product.Name.Should().Be("Notebook Pro");
        product.Description.Should().Be("Descrição do produto");
        product.Price.Should().Be(1999.99m);
        product.Brand.Should().Be("TechBrand");
        product.Color.Should().Be("Prata");
        product.Active.Should().BeTrue();
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrow(string? name)
    {
        var act = () => Product.Create(name!, 10m, "Marca", "Azul");

        act.Should().Throw<DomainException>()
            .WithMessage("O nome do produto é obrigatório.");
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldThrow()
    {
        var act = () => Product.Create("Notebook", -1m, "Marca", "Azul");

        act.Should().Throw<DomainException>()
            .WithMessage("O preço não pode ser negativo.");
    }

    [Theory]
    [InlineData(null, "Azul")]
    [InlineData("", "Azul")]
    [InlineData("Marca", null)]
    [InlineData("Marca", "")]
    public void Create_WithInvalidBrandOrColor_ShouldThrow(string? brand, string? color)
    {
        var act = () => Product.Create("Notebook", 10m, brand!, color!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateProperties()
    {
        var product = Product.Create("Notebook", 1000m, "Marca", "Azul");

        product.Update("Notebook Pro", 1500m, "Nova Marca", "Preto", "Nova descrição", active: false);

        product.Name.Should().Be("Notebook Pro");
        product.Price.Should().Be(1500m);
        product.Brand.Should().Be("Nova Marca");
        product.Color.Should().Be("Preto");
        product.Description.Should().Be("Nova descrição");
        product.Active.Should().BeFalse();
    }

    [Fact]
    public void Update_WithNegativePrice_ShouldThrow()
    {
        var product = Product.Create("Notebook", 1000m, "Marca", "Azul");

        var act = () => product.Update("Notebook", -10m, "Marca", "Azul", null, true);

        act.Should().Throw<DomainException>()
            .WithMessage("O preço não pode ser negativo.");
        product.Price.Should().Be(1000m);
    }

    [Fact]
    public void SetCategories_WithoutCategories_ShouldThrow()
    {
        var product = Product.Create("Notebook", 1000m, "Marca", "Azul");

        var act = () => product.SetCategories([]);

        act.Should().Throw<DomainException>()
            .WithMessage("O produto deve pertencer a pelo menos uma categoria.");
    }

    [Fact]
    public void SetCategories_WithCategories_ShouldAssign()
    {
        var product = Product.Create("Notebook", 1000m, "Marca", "Azul");
        var categories = new[] { Category.Create("Eletrônicos"), Category.Create("Informática") };

        product.SetCategories(categories);

        product.Categories.Should().HaveCount(2);
    }
}
