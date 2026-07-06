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

builder.Services.AddMudServices();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CertusDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SeedData.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
