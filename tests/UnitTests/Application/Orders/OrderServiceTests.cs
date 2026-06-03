using System.Reflection;
using Application.Abstractions.Persistence;
using Application.Exceptions;
using Application.Orders;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace Teste.UnitTests.Application.Orders;

public sealed class OrderServiceTests
{
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly IBuyerRepository _buyers = Substitute.For<IBuyerRepository>();
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly OrderService _sut;

    private readonly Guid _buyerId = Guid.NewGuid();

    public OrderServiceTests() => _sut = new OrderService(_orders, _buyers, _products);

    private static readonly IReadOnlyList<OrderLineInput> DefaultLines =
        [new OrderLineInput(1, 2)];

    private void SetupBuyer(bool exists = true) =>
        _buyers.ExistsAsync(_buyerId, Arg.Any<CancellationToken>()).Returns(exists);

    private void SetupProducts(params (long id, decimal price, bool active)[] items)
    {
        var catalog = items.Select(i =>
        {
            var p = Product.Create($"Produto {i.id}", i.price, "Marca", "Azul");
            SetEntityId(p, i.id);
            if (!i.active)
                p.Update($"Produto {i.id}", i.price, "Marca", "Azul", null, active: false);
            return p;
        }).ToList();

        _products.GetByIdsForLinkAsync(Arg.Any<IReadOnlyList<long>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var ids = callInfo.Arg<IReadOnlyList<long>>();
                return (IReadOnlyList<Product>)catalog.Where(p => ids.Contains(p.Id)).ToList();
            });
    }

    private static void SetEntityId(LongEntity entity, long id) =>
        typeof(LongEntity).GetProperty(nameof(LongEntity.Id))!.SetValue(entity, id);

    [Fact]
    public async Task CreateAsync_WhenValid_ShouldPersistIniciadoOrder()
    {
        SetupBuyer();
        SetupProducts((1, 100m, true));

        var result = await _sut.CreateAsync(_buyerId, DefaultLines);

        result.Status.Should().Be(OrderStatus.Iniciado);
        result.Items.Should().HaveCount(1);
        await _orders.Received(1).AddAsync(
            Arg.Is<Order>(o => o.Status == OrderStatus.Iniciado && o.Items.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenBuyerDoesNotExist_ShouldThrow()
    {
        SetupBuyer(exists: false);

        var act = () => _sut.CreateAsync(_buyerId, DefaultLines);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("O comprador informado não existe.");
    }

    [Fact]
    public async Task CreateAsync_WhenProductInactive_ShouldThrow()
    {
        SetupBuyer();
        SetupProducts((1, 100m, false));

        var act = () => _sut.CreateAsync(_buyerId, DefaultLines);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("O produto 'Produto 1' está inativo.");
    }

    [Fact]
    public async Task UpdateDetailsAsync_WhenIniciado_ShouldUpdateDetails()
    {
        SetupBuyer();
        SetupProducts((1, 10m, true), (2, 20m, true));

        var order = Order.Create(_buyerId, [OrderItem.Create(1, 1, 10m)]);
        _orders.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        var result = await _sut.UpdateDetailsAsync(
            order.Id, _buyerId, [new OrderLineInput(2, 3)]);

        order.Items.Should().HaveCount(1);
        order.Items.First().ProductId.Should().Be(2);
        result.Status.Should().Be(OrderStatus.Iniciado);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithProcessado_ShouldProcessOrder()
    {
        var order = Order.Create(_buyerId, [OrderItem.Create(1, 1, 10m)]);
        _orders.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        var result = await _sut.UpdateStatusAsync(order.Id, OrderStatus.Processado);

        result.Status.Should().Be(OrderStatus.Processado);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithEnviado_ShouldShipOrder()
    {
        var order = Order.Create(_buyerId, [OrderItem.Create(1, 1, 10m)]);
        order.Process();
        _orders.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        var result = await _sut.UpdateStatusAsync(order.Id, OrderStatus.Enviado);

        result.Status.Should().Be(OrderStatus.Enviado);
    }

    [Fact]
    public async Task UpdateDetailsAsync_WhenProcessado_ShouldThrow()
    {
        SetupBuyer();
        SetupProducts((1, 10m, true));

        var order = Order.Create(_buyerId, [OrderItem.Create(1, 1, 10m)]);
        order.Process();
        _orders.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        var act = () => _sut.UpdateDetailsAsync(order.Id, _buyerId, DefaultLines);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Apenas pedidos não processados podem ser alterados.");
    }

    [Fact]
    public async Task CancelAsync_WhenIniciado_ShouldCancel()
    {
        var order = Order.Create(_buyerId, [OrderItem.Create(1, 1, 10m)]);
        _orders.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        var result = await _sut.CancelAsync(order.Id);

        result.Status.Should().Be(OrderStatus.Cancelado);
    }

    [Fact]
    public async Task ListAsync_WhenBuyerExists_ShouldReturnPagedOrders()
    {
        SetupBuyer();
        var order = Order.Create(_buyerId, [OrderItem.Create(1, 1, 10m)]);
        var filter = OrderListFilter.Create(null, null, null, 1, 10, _buyerId);
        _orders.ListAsync(filter, Arg.Any<CancellationToken>())
            .Returns((new List<Order> { order }, 1));

        var result = await _sut.ListAsync(filter);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task ListAsync_WhenBuyerNotFound_ShouldThrow()
    {
        SetupBuyer(exists: false);
        var filter = OrderListFilter.Create(null, null, null, 1, 10, _buyerId);

        var act = () => _sut.ListAsync(filter);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Comprador com id '{_buyerId}' não encontrado.");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ShouldThrow()
    {
        _orders.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var act = () => _sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenIniciado_ShouldRemove()
    {
        var order = Order.Create(_buyerId, [OrderItem.Create(1, 1, 10m)]);
        _orders.GetByIdAsync(order.Id, Arg.Any<CancellationToken>()).Returns(order);

        await _sut.DeleteAsync(order.Id);

        _orders.Received(1).Remove(order);
    }
}
