using System.Text.Json;
using Application.Exceptions;
using Domain.Exceptions;

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
        var (statusCode, response) = ExceptionResponseMapper.Map(
            exception,
            context.Request.Path.Value);

        if (statusCode == System.Net.HttpStatusCode.InternalServerError)
            logger.LogError(exception, "Erro não tratado: {Code} - {Message}", response.Code, exception.Message);
        else
            logger.LogWarning(
                exception,
                "{Title} [{Code}]: {Message}",
                response.Title,
                response.Code,
                exception.Message);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = response.Status;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
