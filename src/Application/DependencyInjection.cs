using Application.Buyers;
using Application.Categories;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IBuyerService, BuyerService>();
        services.AddScoped<ICategoryService, CategoryService>();
        return services;
    }
}
