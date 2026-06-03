using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;

namespace Teste.UnitTests.Domain.Entities;

public sealed class OrderItemTests
{
    [Fact]
    public void Create_WithValidData_ShouldCalculateLineTotal()
    {
        var item = OrderItem.Create(1, 3, 12.5m);

        item.ProductId.Should().Be(1);
        item.Quantity.Should().Be(3);
        item.UnitPrice.Should().Be(12.5m);
        item.LineTotal.Should().Be(37.5m);
    }

    [Fact]
    public void Create_WithInvalidQuantity_ShouldThrow()
    {
        var act = () => OrderItem.Create(1, 0, 10m);

        act.Should().Throw<DomainException>()
            .WithMessage("A quantidade deve ser maior que zero.");
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldThrow()
    {
        var act = () => OrderItem.Create(1, 1, -1m);

        act.Should().Throw<DomainException>()
            .WithMessage("O preço unitário não pode ser negativo.");
    }
}
