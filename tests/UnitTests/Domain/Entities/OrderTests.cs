using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;

namespace Teste.UnitTests.Domain.Entities;

public sealed class OrderTests
{
    private static OrderItem Line(long productId = 1, int qty = 1, decimal price = 10m) =>
        OrderItem.Create(productId, qty, price);

    [Fact]
    public void Create_WithValidData_ShouldStartAsIniciado()
    {
        var buyerId = Guid.NewGuid();
        var order = Order.Create(buyerId, [Line()]);

        order.BuyerId.Should().Be(buyerId);
        order.Status.Should().Be(OrderStatus.Iniciado);
        order.Items.Should().HaveCount(1);
        order.Total.Should().Be(10m);
    }

    [Fact]
    public void Create_WithoutItems_ShouldThrow()
    {
        var act = () => Order.Create(Guid.NewGuid(), []);

        act.Should().Throw<DomainException>()
            .WithMessage("O pedido deve conter pelo menos um produto.");
    }

    [Fact]
    public void Create_WithDuplicateProducts_ShouldThrow()
    {
        var act = () => Order.Create(Guid.NewGuid(), [Line(1), Line(1)]);

        act.Should().Throw<DomainException>()
            .WithMessage("O pedido não pode conter o mesmo produto mais de uma vez.");
    }

    [Fact]
    public void UpdateDetails_WhenIniciado_ShouldReplaceItems()
    {
        var order = Order.Create(Guid.NewGuid(), [Line(1, 2, 5m)]);
        var newBuyer = Guid.NewGuid();

        order.UpdateDetails(newBuyer, [Line(2, 1, 20m)]);

        order.BuyerId.Should().Be(newBuyer);
        order.Items.Should().HaveCount(1);
        order.Items.First().ProductId.Should().Be(2);
        order.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateDetails_WhenProcessado_ShouldThrow()
    {
        var order = Order.Create(Guid.NewGuid(), [Line()]);
        order.Process();

        var act = () => order.UpdateDetails(Guid.NewGuid(), [Line(2)]);

        act.Should().Throw<DomainException>()
            .WithMessage("Apenas pedidos não processados podem ser alterados.");
    }

    [Fact]
    public void Process_WhenIniciado_ShouldSetProcessado()
    {
        var order = Order.Create(Guid.NewGuid(), [Line()]);
        order.Process();
        order.Status.Should().Be(OrderStatus.Processado);
    }

    [Fact]
    public void Process_WhenNotIniciado_ShouldThrow()
    {
        var order = Order.Create(Guid.NewGuid(), [Line()]);
        order.Process();

        var act = () => order.Process();

        act.Should().Throw<DomainException>()
            .WithMessage("Apenas pedidos iniciados podem ser processados.");
    }

    [Fact]
    public void Ship_WhenProcessado_ShouldSetEnviado()
    {
        var order = Order.Create(Guid.NewGuid(), [Line()]);
        order.Process();
        order.Ship();
        order.Status.Should().Be(OrderStatus.Enviado);
    }

    [Fact]
    public void Ship_WhenIniciado_ShouldThrow()
    {
        var order = Order.Create(Guid.NewGuid(), [Line()]);

        var act = () => order.Ship();

        act.Should().Throw<DomainException>()
            .WithMessage("Apenas pedidos processados podem ser enviados.");
    }

    [Fact]
    public void Cancel_WhenIniciadoOrProcessado_ShouldSetCancelado()
    {
        var iniciado = Order.Create(Guid.NewGuid(), [Line()]);
        iniciado.Cancel();
        iniciado.Status.Should().Be(OrderStatus.Cancelado);

        var processado = Order.Create(Guid.NewGuid(), [Line()]);
        processado.Process();
        processado.Cancel();
        processado.Status.Should().Be(OrderStatus.Cancelado);
    }

    [Fact]
    public void Cancel_WhenEnviado_ShouldThrow()
    {
        var order = Order.Create(Guid.NewGuid(), [Line()]);
        order.Process();
        order.Ship();

        var act = () => order.Cancel();

        act.Should().Throw<DomainException>()
            .WithMessage("Apenas pedidos iniciados ou processados podem ser cancelados.");
    }

    [Fact]
    public void EnsureCanDelete_WhenNotIniciado_ShouldThrow()
    {
        var order = Order.Create(Guid.NewGuid(), [Line()]);
        order.Process();

        var act = () => order.EnsureCanDelete();

        act.Should().Throw<DomainException>()
            .WithMessage("Apenas pedidos iniciados podem ser excluídos.");
    }
}
