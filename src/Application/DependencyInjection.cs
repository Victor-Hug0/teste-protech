using Microsoft.Extensions.DependencyInjection;
using Application.Features.Buyers.Create;
using Application.Features.Buyers.GetAll;
using Application.Features.Todos.Complete;
using Application.Features.Todos.Create;
using Application.Features.Todos.Delete;
using Application.Features.Todos.GetAll;
using Application.Features.Todos.GetById;
using Application.Features.Todos.Update;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateBuyerHandler, CreateBuyerHandler>();
        services.AddScoped<IGetAllBuyersHandler, GetAllBuyersHandler>();
        services.AddScoped<ICreateTodoHandler, CreateTodoHandler>();
        services.AddScoped<IGetAllTodosHandler, GetAllTodosHandler>();
        services.AddScoped<IGetTodoByIdHandler, GetTodoByIdHandler>();
        services.AddScoped<IUpdateTodoHandler, UpdateTodoHandler>();
        services.AddScoped<ICompleteTodoHandler, CompleteTodoHandler>();
        services.AddScoped<IDeleteTodoHandler, DeleteTodoHandler>();

        return services;
    }
}
