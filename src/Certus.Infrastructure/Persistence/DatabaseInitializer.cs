using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Certus.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(CertusDbContext db)
    {
        await EnsureDatabaseExistsAsync(db);
        await BootstrapMigrationHistoryAsync(db);
        await db.Database.MigrateAsync();
    }

    private static async Task EnsureDatabaseExistsAsync(CertusDbContext db)
    {
        var connectionString = db.Database.GetConnectionString();
        if (string.IsNullOrEmpty(connectionString))
            return;

        var sb = new SqlConnectionStringBuilder(connectionString);
        var databaseName = sb.InitialCatalog;
        sb.InitialCatalog = "master";

        using var masterConn = new SqlConnection(sb.ConnectionString);
        await masterConn.OpenAsync();

        using var checkCmd = masterConn.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = @name";
        checkCmd.Parameters.AddWithValue("@name", databaseName);
        var dbExists = (int)checkCmd.ExecuteScalar()! > 0;

        if (!dbExists)
        {
            using var createCmd = masterConn.CreateCommand();
            createCmd.CommandText = $"CREATE DATABASE [{databaseName}]";
            await createCmd.ExecuteNonQueryAsync();
        }

        await masterConn.CloseAsync();
    }

    private static async Task BootstrapMigrationHistoryAsync(CertusDbContext db)
    {
        var connection = db.Database.GetDbConnection();
        await connection.OpenAsync();

        // Check if domain tables exist (from EnsureCreated) but migration history is missing
        bool domainTablesExist;
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Portfolios'";
            domainTablesExist = (int)cmd.ExecuteScalar()! > 0;
        }

        bool initialCreateApplied = false;
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory'";
            var historyExists = (int)cmd.ExecuteScalar()! > 0;
            if (historyExists)
            {
                using var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260714194959_InitialCreate'";
                initialCreateApplied = (int)checkCmd.ExecuteScalar()! > 0;
            }
        }

        // Only bootstrap when tables exist from EnsureCreated but migration not recorded
        if (domainTablesExist && !initialCreateApplied)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory'";
                if ((int)cmd.ExecuteScalar()! == 0)
                {
                    using var createCmd = connection.CreateCommand();
                    createCmd.CommandText = @"
                        CREATE TABLE [__EFMigrationsHistory] (
                            [MigrationId] nvarchar(150) NOT NULL,
                            [ProductVersion] nvarchar(32) NOT NULL,
                            CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                        )";
                    createCmd.ExecuteNonQuery();
                }
            }

            var version = typeof(DbContext).Assembly.GetName().Version?.ToString() ?? "9.0.0";
            using var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = "INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (@id, @ver)";
            insertCmd.Parameters.Add(new SqlParameter("@id", "20260714194959_InitialCreate"));
            insertCmd.Parameters.Add(new SqlParameter("@ver", version));
            insertCmd.ExecuteNonQuery();
        }

        await connection.CloseAsync();
    }
}
