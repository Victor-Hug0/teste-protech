using System.Net;
using System.Text.Json;
using Teste.Application.Exceptions;
using Teste.Domain.Exceptions;

namespace Teste.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Recurso não encontrado"),
            DomainException => (HttpStatusCode.BadRequest, "Regra de negócio violada"),
            _ => (HttpStatusCode.InternalServerError, "Erro interno do servidor")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            logger.LogError(exception, "Erro não tratado: {Message}", exception.Message);
        else
            logger.LogWarning(exception, "{Title}: {Message}", title, exception.Message);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problem = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status = (int)statusCode,
            detail = exception.Message,
            instance = context.Request.Path.Value
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}
