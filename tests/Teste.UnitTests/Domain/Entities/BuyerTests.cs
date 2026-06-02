using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;

namespace Teste.UnitTests.Domain.Entities;

public sealed class BuyerTests
{
    [Fact]
    public void Create_WithValidData_ShouldSetProperties()
    {
        var buyer = Buyer.Create("  Maria Silva  ", "  maria@example.com  ");

        buyer.Id.Should().NotBeEmpty();
        buyer.Name.Should().Be("Maria Silva");
        buyer.Email.Should().Be("maria@example.com");
        buyer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        buyer.Orders.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrow(string? name)
    {
        var act = () => Buyer.Create(name!, "email@test.com");

        act.Should().Throw<DomainException>().WithMessage("O nome é obrigatório.");
    }

    [Fact]
    public void Create_WithNameShorterThanMinimum_ShouldThrow()
    {
        var act = () => Buyer.Create("ab", "email@test.com");

        act.Should().Throw<DomainException>()
            .WithMessage($"O nome deve ter no mínimo {Buyer.NameMinLength} caracteres.");
    }

    [Fact]
    public void Create_WithNameLongerThanMaximum_ShouldThrow()
    {
        var act = () => Buyer.Create(new string('a', Buyer.NameMaxLength + 1), "email@test.com");

        act.Should().Throw<DomainException>()
            .WithMessage($"O nome deve ter no máximo {Buyer.NameMaxLength} caracteres.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid")]
    [InlineData("@test.com")]
    [InlineData("test@")]
    public void Create_WithInvalidEmail_ShouldThrow(string? email)
    {
        var act = () => Buyer.Create("Maria Silva", email!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithEmailLongerThanMaximum_ShouldThrow()
    {
        var email = $"{new string('a', Buyer.EmailMaxLength)}@t.com";

        var act = () => Buyer.Create("Maria Silva", email);

        act.Should().Throw<DomainException>()
            .WithMessage($"O e-mail deve ter no máximo {Buyer.EmailMaxLength} caracteres.");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateProperties()
    {
        var buyer = Buyer.Create("Maria Silva", "maria@example.com");

        buyer.Update("  João Santos  ", "  joao@example.com  ");

        buyer.Name.Should().Be("João Santos");
        buyer.Email.Should().Be("joao@example.com");
    }

    [Fact]
    public void Update_WithInvalidName_ShouldThrowWithoutChangingEmail()
    {
        var buyer = Buyer.Create("Maria Silva", "maria@example.com");

        var act = () => buyer.Update("ab", "maria@example.com");

        act.Should().Throw<DomainException>();
        buyer.Name.Should().Be("Maria Silva");
        buyer.Email.Should().Be("maria@example.com");
    }
}
