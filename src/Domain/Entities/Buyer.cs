using Domain.Common;
using Domain.Exceptions;

namespace Domain.Entities;

public sealed class Buyer : Entity
{
    public const int NameMinLength = 3;
    public const int NameMaxLength = 150;
    public const int EmailMaxLength = 150;

    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();

    private Buyer() { }

    private Buyer(Guid id, string name, string email, DateTime createdAt)
        : base(id)
    {
        Name = name;
        Email = email;
        CreatedAt = createdAt;
    }

    public static Buyer Create(string name, string email) =>
        new(Guid.NewGuid(), ValidateName(name), ValidateEmail(email), DateTime.UtcNow);

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome é obrigatório.", BusinessRuleCodes.Buyer.NameRequired);

        var trimmed = name.Trim();

        if (trimmed.Length < NameMinLength)
            throw new DomainException($"O nome deve ter no mínimo {NameMinLength} caracteres.", BusinessRuleCodes.Buyer.NameTooShort);

        if (trimmed.Length > NameMaxLength)
            throw new DomainException($"O nome deve ter no máximo {NameMaxLength} caracteres.", BusinessRuleCodes.Buyer.NameTooLong);

        return trimmed;
    }

    private static string ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("O e-mail é obrigatório.", BusinessRuleCodes.Buyer.EmailRequired);

        var trimmed = email.Trim();

        if (trimmed.Length > EmailMaxLength)
            throw new DomainException($"O e-mail deve ter no máximo {EmailMaxLength} caracteres.", BusinessRuleCodes.Buyer.EmailTooLong);

        if (!trimmed.Contains('@') || trimmed.StartsWith('@') || trimmed.EndsWith('@'))
            throw new DomainException("O e-mail informado é inválido.", BusinessRuleCodes.Buyer.EmailInvalid);

        return trimmed;
    }
}
