namespace Domain.Enums;

public static class OrderStatus
{
    public const string Iniciado = "INICIADO";
    public const string Processado = "PROCESSADO";
    public const string Enviado = "ENVIADO";
    public const string Cancelado = "CANCELADO";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Iniciado,
            Processado,
            Enviado,
            Cancelado
        };
}
