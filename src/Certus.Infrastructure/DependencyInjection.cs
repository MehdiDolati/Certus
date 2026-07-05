using Certus.Domain.RiskAndPortfolio.Repositories;
using Certus.Domain.Strategy.Repositories;
using Certus.Domain.Execution.Repositories;
using Certus.Domain.Market.Repositories;
using Certus.Domain.Evaluation.Repositories;
using Certus.Domain.Coordination.Repositories;
using Certus.Domain.Platform.Repositories;
using Certus.Domain.Platform.Interfaces;
using Certus.Domain.SharedKernel;
using Certus.Infrastructure;
using Certus.Infrastructure.Persistence;
using Certus.Infrastructure.Persistence.Repositories;
using Certus.Infrastructure.Persistence.Repositories.RiskAndPortfolio;
using Certus.Infrastructure.Persistence.Repositories.Strategy;
using Certus.Infrastructure.Persistence.Repositories.Execution;
using Certus.Infrastructure.Persistence.Repositories.Market;
using Certus.Infrastructure.Persistence.Repositories.Evaluation;
using Certus.Infrastructure.Persistence.Repositories.Coordination;
using Certus.Infrastructure.Persistence.Repositories.Platform;
using Certus.Infrastructure.Platform;
using Certus.Infrastructure.Platform.Plugins.MetaTrader4;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Certus.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CertusDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection") ?? "Data Source=certus.db"));

        // Shared
        services.AddScoped<IDomainEventDispatcher, InMemoryDomainEventDispatcher>();

        // RiskAndPortfolio
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<IRiskRepository, RiskRepository>();

        // Strategy
        services.AddScoped<IStrategyDefinitionRepository, StrategyDefinitionRepository>();
        services.AddScoped<IBacktestRepository, BacktestRepository>();

        // Execution
        services.AddScoped<ITradeRepository, TradeRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IExecutionLogRepository, ExecutionLogRepository>();

        // Market
        services.AddScoped<IMarketDataRepository, MarketDataRepository>();
        services.AddScoped<ISignalRepository, SignalRepository>();

        // Evaluation
        services.AddScoped<IPerformanceReportRepository, PerformanceReportRepository>();
        services.AddScoped<IPerformanceSnapshotRepository, PerformanceSnapshotRepository>();
        services.AddScoped<IStrategyEvaluationRepository, StrategyEvaluationRepository>();

        // Coordination
        services.AddScoped<IAgentTaskRepository, AgentTaskRepository>();
        services.AddScoped<ISystemHealthRepository, SystemHealthRepository>();

        // Platform
        services.AddScoped<IPlatformConnectionRepository, PlatformConnectionRepository>();
        services.AddScoped<IImportedTradeRepository, ImportedTradeRepository>();
        services.AddSingleton<IFileImportService, FileImportService>();
        services.AddSingleton<PluginLoader>();
        services.AddSingleton<IPlatformPluginLoader>(sp => sp.GetRequiredService<PluginLoader>());
        services.AddSingleton<IPlatformPlugin, Mt4Plugin>();

        return services;
    }
}
