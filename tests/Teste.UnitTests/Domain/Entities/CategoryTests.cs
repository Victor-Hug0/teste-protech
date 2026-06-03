using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;

namespace Teste.UnitTests.Domain.Entities;

public sealed class CategoryTests
{
    [Fact]
    public void Create_WithValidData_ShouldSetProperties()
    {
        var category = Category.Create("  Eletrônicos  ", "  Smartphones e TVs  ", parentId: null);

        category.Name.Should().Be("Eletrônicos");
        category.Description.Should().Be("Smartphones e TVs");
        category.ParentId.Should().BeNull();
        category.Active.Should().BeTrue();
        category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrow(string? name)
    {
        var act = () => Category.Create(name!, "Descrição");

        act.Should().Throw<DomainException>()
            .WithMessage("O nome da categoria é obrigatório.");
    }

    [Fact]
    public void Create_WithNameShorterThanMinimum_ShouldThrow()
    {
        var act = () => Category.Create("A", "Descrição");

        act.Should().Throw<DomainException>()
            .WithMessage($"O nome deve ter no mínimo {Category.NameMinLength} caracteres.");
    }

    [Fact]
    public void Create_WithNameLongerThanMaximum_ShouldThrow()
    {
        var act = () => Category.Create(new string('a', Category.NameMaxLength + 1));

        act.Should().Throw<DomainException>()
            .WithMessage($"O nome deve ter no máximo {Category.NameMaxLength} caracteres.");
    }

    [Fact]
    public void Create_WithDescriptionLongerThanMaximum_ShouldThrow()
    {
        var act = () => Category.Create("Eletrônicos", new string('a', Category.DescriptionMaxLength + 1));

        act.Should().Throw<DomainException>()
            .WithMessage($"A descrição deve ter no máximo {Category.DescriptionMaxLength} caracteres.");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateProperties()
    {
        var category = Category.Create("Eletrônicos", "Descrição antiga");

        category.Update("  Informática  ", "  Nova descrição  ", active: false, parentId: 10);

        category.Name.Should().Be("Informática");
        category.Description.Should().Be("Nova descrição");
        category.Active.Should().BeFalse();
        category.ParentId.Should().Be(10);
    }

    [Fact]
    public void Update_WithInvalidName_ShouldThrowWithoutChangingActive()
    {
        var category = Category.Create("Eletrônicos", "Descrição");

        var act = () => category.Update("A", "Descrição", active: false, parentId: null);

        act.Should().Throw<DomainException>();
        category.Name.Should().Be("Eletrônicos");
        category.Active.Should().BeTrue();
    }
}
