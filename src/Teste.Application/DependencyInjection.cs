using Microsoft.Extensions.DependencyInjection;
using Teste.Application.Features.Buyers.Create;
using Teste.Application.Features.Buyers.GetAll;
using Teste.Application.Features.Todos.Complete;
using Teste.Application.Features.Todos.Create;
using Teste.Application.Features.Todos.Delete;
using Teste.Application.Features.Todos.GetAll;
using Teste.Application.Features.Todos.GetById;
using Teste.Application.Features.Todos.Update;

namespace Teste.Application;

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
