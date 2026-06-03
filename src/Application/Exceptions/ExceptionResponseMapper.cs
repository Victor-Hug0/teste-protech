using System.Net;
using Domain.Exceptions;

namespace Application.Exceptions;

public static class ExceptionResponseMapper
{
    public static (HttpStatusCode StatusCode, ApiErrorResponse Response) Map(
        Exception exception,
        string? instance)
    {
        return exception switch
        {
            NotFoundException notFound => (
                HttpStatusCode.NotFound,
                Create(
                    HttpStatusCode.NotFound,
                    "Recurso não encontrado",
                    notFound.Message,
                    instance,
                    notFound.Code)),

            ConflictException conflict => (
                HttpStatusCode.Conflict,
                Create(
                    HttpStatusCode.Conflict,
                    "Conflito de regra de negócio",
                    conflict.Message,
                    instance,
                    conflict.Code)),

            DomainException domain => (
                HttpStatusCode.BadRequest,
                Create(
                    HttpStatusCode.BadRequest,
                    "Regra de negócio violada",
                    domain.Message,
                    instance,
                    domain.Code)),

            _ => (
                HttpStatusCode.InternalServerError,
                Create(
                    HttpStatusCode.InternalServerError,
                    "Erro interno do servidor",
                    "Ocorreu um erro inesperado.",
                    instance,
                    "INTERNAL_SERVER_ERROR"))
        };
    }

    private static ApiErrorResponse Create(
        HttpStatusCode statusCode,
        string title,
        string detail,
        string? instance,
        string code) =>
        new(
            $"https://httpstatuses.com/{(int)statusCode}",
            title,
            (int)statusCode,
            detail,
            instance,
            code);
}
