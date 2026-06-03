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
        {
            logger.LogError(
                exception,
                "Unhandled error {ErrorCode} {StatusCode} {RequestMethod} {RequestPath}",
                response.Code,
                response.Status,
                context.Request.Method,
                context.Request.Path.Value);
        }
        else
        {
            logger.LogWarning(
                exception,
                "Business error {ErrorCode} {ErrorTitle} {StatusCode} {RequestMethod} {RequestPath}",
                response.Code,
                response.Title,
                response.Status,
                context.Request.Method,
                context.Request.Path.Value);
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = response.Status;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
