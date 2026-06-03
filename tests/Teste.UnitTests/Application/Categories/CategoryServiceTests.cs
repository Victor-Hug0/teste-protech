using Application.Abstractions.Persistence;
using Application.Categories;
using Application.Exceptions;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace Teste.UnitTests.Application.Categories;

public sealed class CategoryServiceTests
{
    private readonly ICategoryRepository _repository = Substitute.For<ICategoryRepository>();
    private readonly CategoryService _sut;

    public CategoryServiceTests() => _sut = new CategoryService(_repository);

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedDtos()
    {
        var categories = new List<Category>
        {
            Category.Create("Eletrônicos", "Tech"),
            Category.Create("Moda", "Roupas")
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(categories);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Eletrônicos");
        result[1].Name.Should().Be("Moda");
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryExists_ShouldReturnDto()
    {
        var category = Category.Create("Eletrônicos", "Tech");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(category);

        var result = await _sut.GetByIdAsync(1);

        result.Name.Should().Be("Eletrônicos");
        result.Description.Should().Be("Tech");
    }

    [Fact]
    public async Task GetByIdAsync_WhenCategoryNotFound_ShouldThrowNotFoundException()
    {
        _repository.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Category?)null);

        var act = () => _sut.GetByIdAsync(99);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Categoria com id '99' não encontrada.");
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsAvailable_ShouldPersistAndReturnDto()
    {
        _repository.ExistsByNameAsync("Eletrônicos", null, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.CreateAsync("Eletrônicos", "Tech", parentId: null);

        result.Name.Should().Be("Eletrônicos");
        result.Description.Should().Be("Tech");
        await _repository.Received(1).AddAsync(
            Arg.Is<Category>(c => c.Name == "Eletrônicos"),
            Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ShouldThrowDomainException()
    {
        _repository.ExistsByNameAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = () => _sut.CreateAsync("Eletrônicos", null, null);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Já existe uma categoria com este nome.");
        await _repository.DidNotReceive().AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenParentDoesNotExist_ShouldThrowDomainException()
    {
        _repository.ExistsByNameAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>()).Returns(false);
        _repository.ExistsAsync(10, Arg.Any<CancellationToken>()).Returns(false);

        var act = () => _sut.CreateAsync("Subcategoria", null, parentId: 10);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("A categoria pai informada não existe.");
    }

    [Fact]
    public async Task CreateAsync_WhenParentExists_ShouldCreateWithParentId()
    {
        _repository.ExistsByNameAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>()).Returns(false);
        _repository.ExistsAsync(1, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.CreateAsync("Smartphones", "Celulares", parentId: 1);

        result.Name.Should().Be("Smartphones");
        result.ParentId.Should().Be(1);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithInvalidName_ShouldThrowBeforePersisting()
    {
        _repository.ExistsByNameAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>()).Returns(false);

        var act = () => _sut.CreateAsync("A", null, null);

        await act.Should().ThrowAsync<DomainException>();
        await _repository.DidNotReceive().AddAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryExists_ShouldUpdateAndPersist()
    {
        var category = Category.Create("Eletrônicos", "Tech");
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(category);
        _repository.ExistsByNameAsync("Informática", 1, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.UpdateAsync(1, "Informática", "Hardware", active: false, parentId: null);

        result.Name.Should().Be("Informática");
        result.Description.Should().Be("Hardware");
        result.Active.Should().BeFalse();
        _repository.Received(1).Update(category);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryNotFound_ShouldThrowNotFoundException()
    {
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Category?)null);

        var act = () => _sut.UpdateAsync(1, "Nome", null, true, null);

        await act.Should().ThrowAsync<NotFoundException>();
        _repository.DidNotReceive().Update(Arg.Any<Category>());
    }

    [Fact]
    public async Task UpdateAsync_WhenParentIsSelf_ShouldThrowDomainException()
    {
        var category = Category.Create("Eletrônicos", null);
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(category);
        _repository.ExistsByNameAsync(Arg.Any<string>(), 1, Arg.Any<CancellationToken>()).Returns(false);

        var act = () => _sut.UpdateAsync(1, "Eletrônicos", null, true, parentId: 1);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Uma categoria não pode ser pai dela mesma.");
    }

    [Fact]
    public async Task UpdateAsync_WhenNameBelongsToAnotherCategory_ShouldThrowDomainException()
    {
        var category = Category.Create("Eletrônicos", null);
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(category);
        _repository.ExistsByNameAsync("Moda", 1, Arg.Any<CancellationToken>()).Returns(true);

        var act = () => _sut.UpdateAsync(1, "Moda", null, true, null);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Já existe uma categoria com este nome.");
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryExistsWithoutChildren_ShouldRemoveAndSave()
    {
        var category = Category.Create("Eletrônicos", null);
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(category);
        _repository.HasChildrenAsync(1, Arg.Any<CancellationToken>()).Returns(false);

        await _sut.DeleteAsync(1);

        _repository.Received(1).Remove(category);
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryHasChildren_ShouldThrowDomainException()
    {
        var category = Category.Create("Eletrônicos", null);
        category.Children.Add(Category.Create("Smartphones"));
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(category);

        var act = () => _sut.DeleteAsync(1);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Não é possível excluir uma categoria que possui subcategorias.");
        _repository.DidNotReceive().Remove(Arg.Any<Category>());
    }

    [Fact]
    public async Task DeleteAsync_WhenCategoryNotFound_ShouldThrowNotFoundException()
    {
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Category?)null);

        var act = () => _sut.DeleteAsync(1);

        await act.Should().ThrowAsync<NotFoundException>();
        _repository.DidNotReceive().Remove(Arg.Any<Category>());
    }
}
