using Microsoft.EntityFrameworkCore;
using RocketLeagueTradingTools.Core.Application;
using RocketLeagueTradingTools.Core.Application.Contracts;
using RocketLeagueTradingTools.Application.Common;
using RocketLeagueTradingTools.Infrastructure.Common;
using RocketLeagueTradingTools.Infrastructure.Persistence;
using RocketLeagueTradingTools.Infrastructure.TradeOffers;

static class HostConfiguration
{
    public static IHost Configure(this IHostBuilder hostBuilder)
    {
        var host = hostBuilder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton(typeof(ILogger), typeof(Logger<Program>));
                services.AddSingleton(typeof(SessionStorage.ScrapApplication));
                services.AddSingleton<ILog, Log>();
                services.AddSingleton<IHttp, Http>();
                services.AddSingleton<IDateTime, SystemDateTime>();
                services.AddSingleton<RocketLeagueTradingTools.Core.Application.Contracts.IConfiguration, Configuration>();

                services.AddSQLiteDbContext();

                services.AddScoped<ScrapApplication>();
                services.AddScoped<RlgDataSource>();
                services.AddScoped<ITradeOfferRepository, TradeOfferRepository>();
                services.AddScoped<IPersistenceRepository, PersistenceRepository>();
            })
            .ConfigureLogging(logBuilder =>
            {
                logBuilder.ClearProviders();
                logBuilder.AddSimpleConsole(opt =>
                {
                    opt.TimestampFormat = "HH:mm:ss ";
                });
            })
            .UseConsoleLifetime()
            .Build();

        var config = host.Services.GetRequiredService<RocketLeagueTradingTools.Core.Application.Contracts.IConfiguration>();

        // Apply Entity Framework migrations
        using (var scope = host.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();
            context.Database.Migrate();
        }

        return host;
    }
}
