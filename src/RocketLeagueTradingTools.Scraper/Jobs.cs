using System.Diagnostics;
using RocketLeagueTradingTools.Core.Application;
using RocketLeagueTradingTools.Core.Domain.Exceptions;

class Jobs
{
    public static void ExitKeyPressWaiting(CancellationTokenSource tokenSource)
    {
        while (Console.ReadKey(true).Key != ConsoleKey.Q)
        {
        }

        tokenSource.Cancel();
    }

    public static async Task ContinuousScraping(IHost host, CancellationToken token)
    {
        var config = host.Services.GetRequiredService<RocketLeagueTradingTools.Core.Application.Contracts.IConfiguration>();

        while (true)
        {
            using (var scope = host.Services.CreateScope())
            {
                var scraper = scope.ServiceProvider.GetRequiredService<ScrapApplication>();
                var scrapingWatch = Stopwatch.StartNew();

                try
                {
                    await scraper.ScrapPageAsync(token);
                }
                catch (PageScrapFailedAfterNumberOfRetriesException)
                {
                    // End the scraping if it failed after number of retries (configured in the settings)
                    return;
                }
                finally
                {
                    scrapingWatch.Stop();
                }

                // Wait before scraping again
                var scrapTimeout = new Random().Next(config.ScrapIntervalInSecondsMin * 1000, config.ScrapIntervalInSecondsMax * 1000);
                var delay = scrapTimeout - (int)scrapingWatch.ElapsedMilliseconds;
                await WaitFor(delay, token);

                // End the scraping if it was requested
                if (token.IsCancellationRequested)
                    return;
            }
        }
    }

    private static async Task WaitFor(int delay, CancellationToken token)
    {
        if (delay <= 0)
            return;

        try
        {
            await Task.Delay(delay, token);
        }
        catch (OperationCanceledException)
        {
            if (!token.IsCancellationRequested)
                throw;
        }
    }
}