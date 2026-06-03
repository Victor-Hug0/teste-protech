namespace Domain.Exceptions;

public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string message, string code = BusinessRuleCodes.Common.BusinessRuleViolation)
        : base(message)
    {
        Code = code;
    }
}
