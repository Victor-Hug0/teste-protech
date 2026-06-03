using System.Net;
using Application.Exceptions;
using Domain.Exceptions;
using FluentAssertions;

namespace Teste.UnitTests.Application.Exceptions;

public sealed class ExceptionResponseMapperTests
{
    [Fact]
    public void Map_NotFoundException_ShouldReturn404WithCode()
    {
        var ex = new NotFoundException("Recurso ausente", BusinessRuleCodes.Order.NotFound);

        var (status, response) = ExceptionResponseMapper.Map(ex, "/api/v1/orders/1");

        status.Should().Be(HttpStatusCode.NotFound);
        response.Code.Should().Be(BusinessRuleCodes.Order.NotFound);
        response.Detail.Should().Be("Recurso ausente");
        response.Status.Should().Be(404);
    }

    [Fact]
    public void Map_ConflictException_ShouldReturn409WithCode()
    {
        var ex = new ConflictException(
            "E-mail duplicado",
            BusinessRuleCodes.Buyer.EmailAlreadyExists);

        var (status, response) = ExceptionResponseMapper.Map(ex, "/api/v1/buyers");

        status.Should().Be(HttpStatusCode.Conflict);
        response.Code.Should().Be(BusinessRuleCodes.Buyer.EmailAlreadyExists);
        response.Status.Should().Be(409);
    }

    [Fact]
    public void Map_DomainException_ShouldReturn400WithCode()
    {
        var ex = new DomainException(
            "Apenas pedidos iniciados podem ser processados.",
            BusinessRuleCodes.Order.CannotProcess);

        var (status, response) = ExceptionResponseMapper.Map(ex, "/api/v1/orders/1");

        status.Should().Be(HttpStatusCode.BadRequest);
        response.Code.Should().Be(BusinessRuleCodes.Order.CannotProcess);
        response.Title.Should().Be("Regra de negócio violada");
    }

    [Fact]
    public void Map_UnknownException_ShouldReturn500()
    {
        var (status, response) = ExceptionResponseMapper.Map(
            new InvalidOperationException("falha"),
            "/api/v1/orders");

        status.Should().Be(HttpStatusCode.InternalServerError);
        response.Code.Should().Be("INTERNAL_SERVER_ERROR");
    }
}
