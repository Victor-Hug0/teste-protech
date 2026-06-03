using Application.Abstractions.Persistence;
using Application.Common.Mappings;
using Application.DTOs;
using Application.Exceptions;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Categories;

public sealed class CategoryService(ICategoryRepository categories) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await categories.GetAllAsync(cancellationToken);
        return list.Select(c => c.ToDto()).ToList();
    }

    public async Task<CategoryDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(
                $"Categoria com id '{id}' não encontrada.",
                BusinessRuleCodes.Category.NotFound);

        return category.ToDto();
    }

    public async Task<CategoryDto> CreateAsync(
        string name,
        string? description,
        long? parentId,
        CancellationToken cancellationToken = default)
    {
        await EnsureNameIsAvailableAsync(name, excludeCategoryId: null, cancellationToken);
        await ValidateParentAsync(parentId, categoryId: null, cancellationToken);

        var category = Category.Create(name, description, parentId);

        await categories.AddAsync(category, cancellationToken);
        await categories.SaveChangesAsync(cancellationToken);

        return category.ToDto();
    }

    public async Task<CategoryDto> UpdateAsync(
        long id,
        string name,
        string? description,
        bool active,
        long? parentId,
        CancellationToken cancellationToken = default)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(
                $"Categoria com id '{id}' não encontrada.",
                BusinessRuleCodes.Category.NotFound);

        await EnsureNameIsAvailableAsync(name, excludeCategoryId: id, cancellationToken);
        await ValidateParentAsync(parentId, categoryId: id, cancellationToken);

        category.Update(name, description, active, parentId);
        categories.Update(category);
        await categories.SaveChangesAsync(cancellationToken);

        return category.ToDto();
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(
                $"Categoria com id '{id}' não encontrada.",
                BusinessRuleCodes.Category.NotFound);

        if (category.Children.Count > 0 || await categories.HasChildrenAsync(id, cancellationToken))
            throw new DomainException(
                "Não é possível excluir uma categoria que possui subcategorias.",
                BusinessRuleCodes.Category.HasChildren);

        if (await categories.HasProductsAsync(id, cancellationToken))
            throw new DomainException(
                "Não é possível excluir uma categoria vinculada a produtos.",
                BusinessRuleCodes.Category.HasProducts);

        categories.Remove(category);
        await categories.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureNameIsAvailableAsync(
        string name,
        long? excludeCategoryId,
        CancellationToken cancellationToken)
    {
        if (await categories.ExistsByNameAsync(name, excludeCategoryId, cancellationToken))
            throw new ConflictException(
                "Já existe uma categoria com este nome.",
                BusinessRuleCodes.Category.NameAlreadyExists);
    }

    private async Task ValidateParentAsync(
        long? parentId,
        long? categoryId,
        CancellationToken cancellationToken)
    {
        if (!parentId.HasValue)
            return;

        if (categoryId.HasValue && parentId.Value == categoryId.Value)
            throw new DomainException(
                "Uma categoria não pode ser pai dela mesma.",
                BusinessRuleCodes.Category.CannotBeOwnParent);

        if (!await categories.ExistsAsync(parentId.Value, cancellationToken))
            throw new DomainException(
                "A categoria pai informada não existe.",
                BusinessRuleCodes.Category.ParentNotFound);
    }
}
