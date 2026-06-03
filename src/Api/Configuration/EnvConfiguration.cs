namespace Teste.Api.Configuration;

public static class EnvConfiguration
{
    public static void LoadDotEnv()
    {
        DotNetEnv.Env.TraversePath().Load();
        ApplyDefaultConnectionString();
    }

    /// <summary>
    /// Monta ConnectionStrings__DefaultConnection a partir do .env quando não definida explicitamente.
    /// MSSQL_SA_PASSWORD é compartilhada com docker-compose; DB_SERVER e DB_NAME são opcionais.
    /// </summary>
    private static void ApplyDefaultConnectionString()
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")))
            return;

        var password = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD")
            ?? "YourStrong@Passw0rd";
        var server = Environment.GetEnvironmentVariable("DB_SERVER") ?? "127.0.0.1,1433";
        var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "EcommerceDb";

        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            $"Server={server};Database={database};User Id=sa;Password={password};Encrypt=False;TrustServerCertificate=True;Connect Timeout=60;");
    }
}
