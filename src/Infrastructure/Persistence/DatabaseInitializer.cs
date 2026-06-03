using Infrastructure.Persistence.Seed;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public static class DatabaseInitializer
{
    private const int MaxAttempts = 20;
    private static readonly TimeSpan DelayBetweenAttempts = TimeSpan.FromSeconds(5);

    public static async Task MigrateAndSeedAsync(
        ApplicationDbContext db,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync(cancellationToken);
                await CategorySeeder.SeedAsync(db, cancellationToken);
                logger.LogInformation("Banco de dados migrado e seed aplicado com sucesso.");
                return;
            }
            catch (Exception ex) when (IsDatabaseAlreadyExists(ex))
            {
                lastException = ex;
                logger.LogDebug(
                    "Banco já existe (tentativa {Attempt}); reaplicando migrações...",
                    attempt);
            }
            catch (Exception ex) when (IsTransient(ex))
            {
                lastException = ex;

                if (attempt >= MaxAttempts)
                    break;

                logger.LogWarning(
                    "SQL ainda inicializando ou rede instável (tentativa {Attempt}/{MaxAttempts}). Nova tentativa em {DelaySeconds}s...",
                    attempt,
                    MaxAttempts,
                    DelayBetweenAttempts.TotalSeconds);

                await Task.Delay(DelayBetweenAttempts, cancellationToken);
            }
        }

        throw new InvalidOperationException(
            """
            Não foi possível conectar ao SQL Server em 127.0.0.1:1433.

            Verifique:
            1. docker compose up -d sqlserver
            2. docker compose ps  (status healthy)
            3. Arquivo .env com MSSQL_SA_PASSWORD (cp .env.example .env) igual à senha do container

            Se o erro persistir, recrie o volume:
            docker compose down && docker volume rm teste_sqlserver_data && docker compose up -d sqlserver
            """,
            lastException);
    }

    private static bool IsDatabaseAlreadyExists(Exception exception) =>
        FindSqlException(exception)?.Number == 1801;

    private static bool IsTransient(Exception exception) =>
        exception switch
        {
            SqlException sql => IsTransientSqlException(sql),
            IOException => true,
            TimeoutException => true,
            _ when exception.InnerException is Exception inner => IsTransient(inner),
            _ => false
        };

    private static bool IsTransientSqlException(SqlException sql)
    {
        if (sql.Errors.Cast<SqlError>().Any(e => e.Number == 1801))
            return false;

        // Senha incorreta (não confundir com avaliação de senha durante o boot do SQL)
        if (sql.Errors.Cast<SqlError>().Any(e => e.Number == 18456 && e.State != 7))
            return false;

        if (sql.Errors.Cast<SqlError>().Any(e => TransientSqlErrorNumbers.Contains(e.Number)))
            return true;

        if (sql.Errors.Cast<SqlError>().Any(e => e.Number is 18456 or 17187))
            return true;

        return sql.InnerException is IOException or System.Net.Sockets.SocketException;
    }

    private static SqlException? FindSqlException(Exception exception)
    {
        for (var ex = exception; ex is not null; ex = ex.InnerException)
        {
            if (ex is SqlException sql)
                return sql;
        }

        return null;
    }

    private static readonly HashSet<int> TransientSqlErrorNumbers =
    [
        -2,    // timeout
        64,    // connection closed
        233,   // connection init
        17187, // SQL Server is not ready to accept new client connections
        10053, 10054,
        40197, 40501, 40613,
    ];
}
