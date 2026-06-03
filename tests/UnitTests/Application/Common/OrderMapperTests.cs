using System.Reflection;
using Application.Common.Mappings;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;

namespace Teste.UnitTests.Application.Common;

public sealed class OrderMapperTests
{
    [Fact]
    public void ToDto_ShouldMapOrderWithItemsAndTotal()
    {
        var buyer = Buyer.Create("João", "joao@example.com");
        var product = Product.Create("Mouse", 50m, "Marca", "Preto");
        product.SetCategories([Category.Create("Periféricos")]);
        typeof(LongEntity).GetProperty(nameof(LongEntity.Id))!.SetValue(product, 1L);

        var order = Order.Create(buyer.Id, [OrderItem.Create(1, 2, 50m)]);
        order.Buyer = buyer;
        order.Items.First().Product = product;
        order.Process();

        var dto = order.ToDto();

        dto.BuyerName.Should().Be("João");
        dto.Status.Should().Be(OrderStatus.Processado);
        dto.Items.Should().HaveCount(1);
        dto.Items[0].ProductName.Should().Be("Mouse");
        dto.Items[0].LineTotal.Should().Be(100m);
        dto.Total.Should().Be(100m);
    }
}
