using Certus.Dashboard.Components;
using Certus.Application.Portfolios;
using Certus.Application.Strategies;
using Certus.Application.Trading;
using Certus.Application.Platform;
using Certus.Infrastructure;
using Certus.Infrastructure.Persistence;
using MudBlazor.Services;
using Validator.Application.Web;
using Validator.Infrastructure.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

// Data-validation integration (spec 006): the transport-neutral validator
// boundary consumed as a local NuGet package, with its durable storage root
// under the application's data folder (spec 006, task T035).
var validatorStorageRoot = builder.Configuration["DataValidation:StorageRoot"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "AppData", "data-validation");
builder.Services.AddValidatorWebIntegration(validatorStorageRoot);

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
        await DatabaseInitializer.InitializeAsync(db);
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
