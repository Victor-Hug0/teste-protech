using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Teste.Endpoints;

public static class HealthEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static IEndpointRouteBuilder MapHealthEndpoints(
        this IEndpointRouteBuilder app,
        ApiVersionSet versionSet)
    {
        app.MapHealthChecks("/health", new()
        {
            ResponseWriter = WriteHealthResponse
        })
        .WithName("HealthCheck")
        .WithTags("Health")
        .WithSummary("Verifica a saúde da aplicação e do banco de dados")
        .AllowAnonymous()
        .ExcludeFromDescription();

        app.MapGroup("/api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1)
            .MapGet("/health", GetHealth)
            .WithName("HealthCheckVersioned")
            .WithTags("Health")
            .WithSummary("Health check versionado (mesmo diagnóstico de /health)")
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status503ServiceUnavailable);

        return app;
    }

    private static async Task<IResult> GetHealth(
        HealthCheckService healthCheckService,
        CancellationToken cancellationToken)
    {
        var report = await healthCheckService.CheckHealthAsync(cancellationToken);
        return ToHealthResult(report);
    }

    private static Task WriteHealthResponse(HttpContext context, HealthReport report)
    {
        var result = ToHealthResult(report);
        return result.ExecuteAsync(context);
    }

    private static IResult ToHealthResult(HealthReport report)
    {
        var body = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                data = e.Value.Data
            })
        };

        var statusCode = report.Status == HealthStatus.Healthy
            ? StatusCodes.Status200OK
            : StatusCodes.Status503ServiceUnavailable;

        return Results.Json(body, JsonOptions, statusCode: statusCode);
    }
}
