using Application.Abstractions.Persistence;
using Application.Common.Mappings;
using Application.DTOs;
using Application.Exceptions;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Products;

public sealed class ProductService(
    IProductRepository products,
    ICategoryRepository categories) : IProductService
{
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await products.GetAllAsync(cancellationToken);
        return list.Select(p => p.ToDto()).ToList();
    }

    public async Task<ProductDto> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var product = await products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(
                $"Produto com id '{id}' não encontrado.",
                BusinessRuleCodes.Product.NotFound);

        return product.ToDto();
    }

    public async Task<ProductDto> CreateAsync(
        string name,
        decimal price,
        string brand,
        string color,
        string? description,
        IReadOnlyList<long> categoryIds,
        CancellationToken cancellationToken = default)
    {
        await EnsureNameIsAvailableAsync(name, excludeProductId: null, cancellationToken);

        var product = Product.Create(name, price, brand, color, description);
        var resolvedCategories = await ResolveCategoriesAsync(categoryIds, cancellationToken);
        product.SetCategories(resolvedCategories);

        await products.AddAsync(product, cancellationToken);
        await products.SaveChangesAsync(cancellationToken);

        return product.ToDto();
    }

    public async Task<ProductDto> UpdateAsync(
        long id,
        string name,
        decimal price,
        string brand,
        string color,
        string? description,
        bool active,
        IReadOnlyList<long> categoryIds,
        CancellationToken cancellationToken = default)
    {
        var product = await products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(
                $"Produto com id '{id}' não encontrado.",
                BusinessRuleCodes.Product.NotFound);

        await EnsureNameIsAvailableAsync(name, excludeProductId: id, cancellationToken);

        product.Update(name, price, brand, color, description, active);
        var resolvedCategories = await ResolveCategoriesAsync(categoryIds, cancellationToken);
        product.SetCategories(resolvedCategories);

        products.Update(product);
        await products.SaveChangesAsync(cancellationToken);

        return product.ToDto();
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var product = await products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(
                $"Produto com id '{id}' não encontrado.",
                BusinessRuleCodes.Product.NotFound);

        if (product.OrderItems.Count > 0 || await products.IsUsedInOrdersAsync(id, cancellationToken))
            throw new DomainException(
                "Não é possível excluir um produto vinculado a pedidos.",
                BusinessRuleCodes.Product.InUseByOrders);

        products.Remove(product);
        await products.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<Category>> ResolveCategoriesAsync(
        IReadOnlyList<long> categoryIds,
        CancellationToken cancellationToken)
    {
        var distinctIds = categoryIds.Distinct().ToList();

        if (distinctIds.Count == 0)
            throw new DomainException(
                "O produto deve pertencer a pelo menos uma categoria.",
                BusinessRuleCodes.Product.CategoriesRequired);

        var found = await categories.GetByIdsForLinkAsync(distinctIds, cancellationToken);

        if (found.Count != distinctIds.Count)
            throw new DomainException(
                "Uma ou mais categorias informadas não existem.",
                BusinessRuleCodes.Product.CategoriesNotFound);

        return found;
    }

    private async Task EnsureNameIsAvailableAsync(
        string name,
        long? excludeProductId,
        CancellationToken cancellationToken)
    {
        if (await products.ExistsByNameAsync(name, excludeProductId, cancellationToken))
            throw new ConflictException(
                "Já existe um produto com este nome.",
                BusinessRuleCodes.Product.NameAlreadyExists);
    }
}
