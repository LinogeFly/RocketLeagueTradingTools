using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using RocketLeagueTradingTools.Core.Application;
using RocketLeagueTradingTools.Core.Application.Contracts;
using RocketLeagueTradingTools.Core.Domain.Exceptions;
using RocketLeagueTradingTools.Infrastructure.Common;
using RocketLeagueTradingTools.Infrastructure.Persistence.SQLite;
using RocketLeagueTradingTools.Infrastructure.TradeOffers;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton(typeof(ILogger), typeof(Logger<Program>));
        services.AddSingleton<ILog, Log>();
        services.AddSingleton<IHttp, Http>();
        services.AddSingleton<RocketLeagueTradingTools.Core.Application.Contracts.IConfiguration, Configuration>();

        services.UseSQLiteDbContext();

        services.AddScoped<ScrapApplication>();
        services.AddScoped<RlgDataSource>();
        services.AddScoped<ITradeOfferRepository, TradeOfferRepository>();
        services.AddScoped<IPersistenceRepository, SQLiteDbContext>();
    })
    .ConfigureLogging(logBuilder =>
    {
        logBuilder.ClearProviders();
        logBuilder.AddConsole();
    })
    .UseConsoleLifetime()
    .Build();

var config = host.Services.GetRequiredService<RocketLeagueTradingTools.Core.Application.Contracts.IConfiguration>();
var log = host.Services.GetRequiredService<ILog>();

// Apply Entity Framework migrations
using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SQLiteDbContext>();
    context.Database.Migrate();
}

// Configure HTTP client
host.Services.GetRequiredService<IHttp>()
    .SetTimeout(TimeSpan.FromSeconds(config.HttpTimeoutInSeconds));

Console.WriteLine("Scraping has started. Press Q to stop.");

// Starting new thread with the infinite scraping job
var tokenSource = new CancellationTokenSource();
var token = tokenSource.Token;
var scrapingTask = Task.Run(async () =>
{
    while (true)
    {
        using (var scope = host.Services.CreateScope())
        {
            var scraper = scope.ServiceProvider.GetRequiredService<ScrapApplication>();
            var scrapingWatch = Stopwatch.StartNew();

            try
            {
                await scraper.ScrapPageAsync(token);

                log.Info($"Scraped one page in {scrapingWatch.ElapsedMilliseconds} ms.");
            }
            catch (PageScrapFailedAfterNumberOfRetriesException)
            {
                // Terminate if the scraping failed after number of retries (configured in the settings)
                // Todo: It doesn't stop here
                break;
            }
            finally
            {
                scrapingWatch.Stop();
            }

            // Wait before scraping again
            var scrapTimeout = new Random().Next(config.ScrapIntervalInSecondsMin * 1000, config.ScrapIntervalInSecondsMax * 1000);
            var waitFor = scrapTimeout - (int)scrapingWatch.ElapsedMilliseconds;
            if (waitFor > 0)
                await Task.Delay(waitFor, token);
        }
    }
}, token);

// Waiting for "Q" keypress
while (true)
{
    var input = Console.ReadKey(true);

    if (input.Key == ConsoleKey.Q)
        break;
}

// Initiate a cancellation for the scraping job
tokenSource.Cancel();

// Wait for the scraping job to complete after the cancellation has been requested
try
{
    await scrapingTask;
}
catch (OperationCanceledException)
{
}
finally
{
    tokenSource.Dispose();
}
