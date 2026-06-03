using Domain.Exceptions;

namespace Application.Exceptions;

public sealed class NotFoundException : Exception
{
    public string Code { get; }

    public NotFoundException(string message, string code)
        : base(message)
    {
        Code = code;
    }
}
