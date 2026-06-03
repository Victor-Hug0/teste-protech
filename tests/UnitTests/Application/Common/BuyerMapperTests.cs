using Application.Common.Mappings;
using Domain.Entities;
using FluentAssertions;

namespace Teste.UnitTests.Application.Common;

public sealed class BuyerMapperTests
{
    [Fact]
    public void ToDto_ShouldMapAllProperties()
    {
        var buyer = Buyer.Create("Maria Silva", "maria@example.com");

        var dto = buyer.ToDto();

        dto.Id.Should().Be(buyer.Id);
        dto.Name.Should().Be("Maria Silva");
        dto.Email.Should().Be("maria@example.com");
        dto.CreatedAt.Should().Be(buyer.CreatedAt);
    }
}
