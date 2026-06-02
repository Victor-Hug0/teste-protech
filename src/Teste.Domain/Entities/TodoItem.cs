using Teste.Domain.Common;
using Teste.Domain.Exceptions;

namespace Teste.Domain.Entities;

public sealed class TodoItem : Entity
{
    public const int TitleMinLength = 3;
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 1000;

    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private TodoItem() { }

    private TodoItem(Guid id, string title, string? description, DateTime createdAt)
        : base(id)
    {
        Title = title;
        Description = description;
        CreatedAt = createdAt;
    }

    public static TodoItem Create(string title, string? description = null)
    {
        var normalizedTitle = ValidateTitle(title);
        var normalizedDescription = NormalizeDescription(description);

        return new TodoItem(
            Guid.NewGuid(),
            normalizedTitle,
            normalizedDescription,
            DateTime.UtcNow);
    }

    public void Update(string title, string? description)
    {
        if (IsCompleted)
            throw new DomainException("Não é possível alterar uma tarefa já concluída.");

        Title = ValidateTitle(title);
        Description = NormalizeDescription(description);
    }

    public void Complete()
    {
        if (IsCompleted)
            throw new DomainException("A tarefa já está concluída.");

        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        if (!IsCompleted)
            throw new DomainException("A tarefa já está em aberto.");

        IsCompleted = false;
        CompletedAt = null;
    }

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("O título é obrigatório.");

        var trimmed = title.Trim();

        if (trimmed.Length < TitleMinLength)
            throw new DomainException($"O título deve ter no mínimo {TitleMinLength} caracteres.");

        if (trimmed.Length > TitleMaxLength)
            throw new DomainException($"O título deve ter no máximo {TitleMaxLength} caracteres.");

        return trimmed;
    }

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        var trimmed = description.Trim();

        if (trimmed.Length > DescriptionMaxLength)
            throw new DomainException($"A descrição deve ter no máximo {DescriptionMaxLength} caracteres.");

        return trimmed;
    }
}
