using Application.Orders;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;

namespace Teste.UnitTests.Application.Orders;

public sealed class OrderListFilterTests
{
    [Fact]
    public void Create_WithDefaults_ShouldUseDefaultPagination()
    {
        var filter = OrderListFilter.Create(null, null, null, 0, 0);

        filter.Page.Should().Be(OrderListFilter.DefaultPage);
        filter.PageSize.Should().Be(OrderListFilter.DefaultPageSize);
        filter.Statuses.Should().BeNull();
    }

    [Fact]
    public void Create_WithMultipleStatuses_ShouldParse()
    {
        var filter = OrderListFilter.Create("INICIADO, PROCESSADO", null, null, 1, 20);

        filter.Statuses.Should().BeEquivalentTo(OrderStatus.Iniciado, OrderStatus.Processado);
        filter.PageSize.Should().Be(20);
    }

    [Fact]
    public void Create_WithInvalidStatus_ShouldThrow()
    {
        var act = () => OrderListFilter.Create("INVALIDO", null, null, 1, 10);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WhenCreatedFromAfterCreatedTo_ShouldThrow()
    {
        var act = () => OrderListFilter.Create(
            null,
            new DateTime(2026, 6, 10),
            new DateTime(2026, 6, 1),
            1,
            10);

        act.Should().Throw<DomainException>()
            .WithMessage("'createdFrom' não pode ser maior que 'createdTo'.");
    }

    [Fact]
    public void Create_WithPageSizeAboveMax_ShouldCapAtMax()
    {
        var filter = OrderListFilter.Create(null, null, null, 1, 500);

        filter.PageSize.Should().Be(OrderListFilter.MaxPageSize);
    }
}
