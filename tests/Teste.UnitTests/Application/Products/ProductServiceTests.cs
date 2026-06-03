using Application.Abstractions.Persistence;
using Application.Exceptions;
using Application.Products;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace Teste.UnitTests.Application.Products;

public sealed class ProductServiceTests
{
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly ICategoryRepository _categories = Substitute.For<ICategoryRepository>();
    private readonly ProductService _sut;

    public ProductServiceTests() => _sut = new ProductService(_products, _categories);

    private static readonly IReadOnlyList<long> DefaultCategoryIds = new[] { 1L };

    private void SetupCategories(params long[] ids)
    {
        var list = ids.Select(id => Category.Create($"Categoria {id}")).ToList();
        _categories.GetByIdsForLinkAsync(Arg.Any<IReadOnlyList<long>>(), Arg.Any<CancellationToken>())
            .Returns(list);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedDtos()
    {
        var product = Product.Create("Notebook", 1000m, "Marca A", "Preto");
        product.SetCategories([Category.Create("Eletrônicos")]);
        _products.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<Product> { product });

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Notebook");
        result[0].Categories.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ShouldReturnDto()
    {
        var product = Product.Create("Notebook", 1000m, "Marca A", "Preto");
        product.SetCategories([Category.Create("Eletrônicos")]);
        _products.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(product);

        var result = await _sut.GetByIdAsync(1);

        result.Name.Should().Be("Notebook");
        result.Categories.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        _products.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var act = () => _sut.GetByIdAsync(99);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Produto com id '99' não encontrado.");
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsAvailable_ShouldPersistWithCategories()
    {
        SetupCategories(1);
        _products.ExistsByNameAsync("Notebook", null, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.CreateAsync(
            "Notebook", 1999.99m, "TechBrand", "Prata", "Descrição", DefaultCategoryIds);

        result.Name.Should().Be("Notebook");
        await _products.Received(1).AddAsync(
            Arg.Is<Product>(p => p.Categories.Count == 1),
            Arg.Any<CancellationToken>());
        await _products.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithoutCategories_ShouldThrowDomainException()
    {
        _products.ExistsByNameAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>()).Returns(false);

        var act = () => _sut.CreateAsync("Notebook", 10m, "Marca", "Azul", null, []);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("O produto deve pertencer a pelo menos uma categoria.");
    }

    [Fact]
    public async Task CreateAsync_WhenCategoryDoesNotExist_ShouldThrowDomainException()
    {
        _products.ExistsByNameAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>()).Returns(false);
        _categories.GetByIdsForLinkAsync(Arg.Any<IReadOnlyList<long>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Category>());

        var act = () => _sut.CreateAsync("Notebook", 10m, "Marca", "Azul", null, [1L, 2L]);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Uma ou mais categorias informadas não existem.");
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ShouldThrowDomainException()
    {
        _products.ExistsByNameAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _sut.CreateAsync("Notebook", 10m, "Marca", "Azul", null, DefaultCategoryIds);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Já existe um produto com este nome.");
    }

    [Fact]
    public async Task CreateAsync_WithNegativePrice_ShouldThrowBeforePersisting()
    {
        SetupCategories(1);
        _products.ExistsByNameAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>()).Returns(false);

        var act = () => _sut.CreateAsync("Notebook", -1m, "Marca", "Azul", null, DefaultCategoryIds);

        await act.Should().ThrowAsync<DomainException>();
        await _products.DidNotReceive().AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenProductExists_ShouldUpdateCategories()
    {
        SetupCategories(1, 2);
        var product = Product.Create("Notebook", 1000m, "Marca", "Azul");
        product.SetCategories([Category.Create("Antiga")]);
        _products.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(product);
        _products.ExistsByNameAsync("Notebook Pro", 1, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.UpdateAsync(
            1, "Notebook Pro", 1500m, "Nova Marca", "Preto", null, active: false, [1L, 2L]);

        result.Name.Should().Be("Notebook Pro");
        product.Categories.Should().HaveCount(2);
        _products.Received(1).Update(product);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        _products.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var act = () => _sut.UpdateAsync(1, "Nome", 10m, "Marca", "Azul", null, true, DefaultCategoryIds);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenProductExistsWithoutOrders_ShouldRemoveAndSave()
    {
        var product = Product.Create("Notebook", 1000m, "Marca", "Azul");
        product.SetCategories([Category.Create("Eletrônicos")]);
        _products.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(product);
        _products.IsUsedInOrdersAsync(1, Arg.Any<CancellationToken>()).Returns(false);

        await _sut.DeleteAsync(1);

        _products.Received(1).Remove(product);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductIsUsedInOrders_ShouldThrowDomainException()
    {
        var product = Product.Create("Notebook", 1000m, "Marca", "Azul");
        product.OrderItems.Add(new OrderItem { ProductId = 1, OrderId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10m });
        _products.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(product);

        var act = () => _sut.DeleteAsync(1);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Não é possível excluir um produto vinculado a pedidos.");
    }
}
