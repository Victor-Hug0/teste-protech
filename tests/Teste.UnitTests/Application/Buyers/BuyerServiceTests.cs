using Application.Abstractions.Persistence;
using Application.Buyers;
using Application.Exceptions;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace Teste.UnitTests.Application.Buyers;

public sealed class BuyerServiceTests
{
    private readonly IBuyerRepository _repository = Substitute.For<IBuyerRepository>();
    private readonly BuyerService _sut;

    public BuyerServiceTests() => _sut = new BuyerService(_repository);

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedDtos()
    {
        var buyers = new List<Buyer>
        {
            Buyer.Create("Maria", "maria@example.com"),
            Buyer.Create("João", "joao@example.com")
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(buyers);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Maria");
        result[1].Email.Should().Be("joao@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WhenBuyerExists_ShouldReturnDto()
    {
        var buyer = Buyer.Create("Maria", "maria@example.com");
        _repository.GetByIdAsync(buyer.Id, Arg.Any<CancellationToken>()).Returns(buyer);

        var result = await _sut.GetByIdAsync(buyer.Id);

        result.Id.Should().Be(buyer.Id);
        result.Name.Should().Be("Maria");
        result.Email.Should().Be("maria@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WhenBuyerNotFound_ShouldThrowNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Buyer?)null);

        var act = () => _sut.GetByIdAsync(id);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Comprador com id '{id}' não encontrado.");
    }

    [Fact]
    public async Task CreateAsync_WhenEmailIsAvailable_ShouldPersistAndReturnDto()
    {
        _repository.ExistsByEmailAsync("maria@example.com", null, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _sut.CreateAsync("Maria Silva", "maria@example.com");

        result.Name.Should().Be("Maria Silva");
        result.Email.Should().Be("maria@example.com");
        await _repository.Received(1).AddAsync(
            Arg.Is<Buyer>(b => b.Email == "maria@example.com"),
            Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenEmailAlreadyExists_ShouldThrowDomainException()
    {
        _repository.ExistsByEmailAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _sut.CreateAsync("Maria Silva", "maria@example.com");

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Já existe um comprador com este e-mail.");
        await _repository.DidNotReceive().AddAsync(Arg.Any<Buyer>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithInvalidName_ShouldThrowBeforePersisting()
    {
        _repository.ExistsByEmailAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns(false);

        var act = () => _sut.CreateAsync("ab", "maria@example.com");

        await act.Should().ThrowAsync<DomainException>();
        await _repository.DidNotReceive().AddAsync(Arg.Any<Buyer>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenBuyerExists_ShouldUpdateAndPersist()
    {
        var buyer = Buyer.Create("Maria", "maria@example.com");
        _repository.GetByIdAsync(buyer.Id, Arg.Any<CancellationToken>()).Returns(buyer);
        _repository.ExistsByEmailAsync("joao@example.com", buyer.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _sut.UpdateAsync(buyer.Id, "João Santos", "joao@example.com");

        result.Name.Should().Be("João Santos");
        result.Email.Should().Be("joao@example.com");
        _repository.Received(1).Update(buyer);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenBuyerNotFound_ShouldThrowNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Buyer?)null);

        var act = () => _sut.UpdateAsync(id, "João", "joao@example.com");

        await act.Should().ThrowAsync<NotFoundException>();
        _repository.DidNotReceive().Update(Arg.Any<Buyer>());
    }

    [Fact]
    public async Task UpdateAsync_WhenEmailBelongsToAnotherBuyer_ShouldThrowDomainException()
    {
        var buyer = Buyer.Create("Maria", "maria@example.com");
        _repository.GetByIdAsync(buyer.Id, Arg.Any<CancellationToken>()).Returns(buyer);
        _repository.ExistsByEmailAsync("joao@example.com", buyer.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _sut.UpdateAsync(buyer.Id, "Maria", "joao@example.com");

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Já existe um comprador com este e-mail.");
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WhenBuyerExistsWithoutOrders_ShouldRemoveAndSave()
    {
        var buyer = Buyer.Create("Maria", "maria@example.com");
        _repository.GetByIdAsync(buyer.Id, Arg.Any<CancellationToken>()).Returns(buyer);

        await _sut.DeleteAsync(buyer.Id);

        _repository.Received(1).Remove(buyer);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WhenBuyerHasOrders_ShouldThrowDomainException()
    {
        var buyer = Buyer.Create("Maria", "maria@example.com");
        buyer.Orders.Add(new Order
        {
            BuyerId = buyer.Id,
            Status = "INICIADO",
            CreatedAt = DateTime.UtcNow
        });
        _repository.GetByIdAsync(buyer.Id, Arg.Any<CancellationToken>()).Returns(buyer);

        var act = () => _sut.DeleteAsync(buyer.Id);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Não é possível excluir um comprador que possui pedidos.");
        _repository.DidNotReceive().Remove(Arg.Any<Buyer>());
    }

    [Fact]
    public async Task DeleteAsync_WhenBuyerNotFound_ShouldThrowNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Buyer?)null);

        var act = () => _sut.DeleteAsync(id);

        await act.Should().ThrowAsync<NotFoundException>();
        _repository.DidNotReceive().Remove(Arg.Any<Buyer>());
    }
}
