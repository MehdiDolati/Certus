using Certus.Dashboard.Components;
using Certus.Application.Portfolios;
using Certus.Application.Strategies;
using Certus.Application.Trading;
using Certus.Application.Platform;
using Certus.Infrastructure;
using Certus.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<IPortfolioService, Certus.Application.Portfolios.PortfolioService>();
builder.Services.AddScoped<IStrategyService, Certus.Application.Strategies.StrategyService>();
builder.Services.AddScoped<ITradeService, Certus.Application.Trading.TradeService>();
builder.Services.AddScoped<IPlatformService, Certus.Application.Platform.PlatformService>();
builder.Services.AddScoped<IPortfolioDeploymentService, Certus.Application.Platform.PortfolioDeploymentService>();

builder.Services.AddMudServices();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CertusDbContext>();

    // Skip migrations and seeding in test environment (tests use EnsureCreated with SQLite)
    if (!app.Environment.IsEnvironment("Testing"))
    {
        await MigrateWithBootstrapAsync(db);
        await SeedData.SeedAsync(db);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static async Task MigrateWithBootstrapAsync(CertusDbContext db)
{
    var connection = db.Database.GetDbConnection();
    await connection.OpenAsync();

    // Check if InitialCreate migration is already recorded
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

    if (!initialCreateApplied)
    {
        // Tables exist from EnsureCreated but migration not recorded — create history table if needed and mark as applied
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
        insertCmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@id", "20260714194959_InitialCreate"));
        insertCmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@ver", version));
        insertCmd.ExecuteNonQuery();
    }

    await connection.CloseAsync();
    await db.Database.MigrateAsync();
}
