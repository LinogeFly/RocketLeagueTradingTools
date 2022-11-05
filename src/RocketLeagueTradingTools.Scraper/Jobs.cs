using System.Diagnostics;
using RocketLeagueTradingTools.Infrastructure.Common;
using RocketLeagueTradingTools.Core.Common;
using RocketLeagueTradingTools.Core.Application;
using RocketLeagueTradingTools.Core.Application.Scraping;
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
        var config = host.Services.GetRequiredService<IConfiguration>();
        var scrapIntervalMin = config.GetRequiredValue<string>("ScrapIntervalMin").ToTimeSpan();
        var scrapIntervalMax = config.GetRequiredValue<string>("ScrapIntervalMax").ToTimeSpan();

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
                var scrapTimeout = new Random().Next((int)scrapIntervalMin.TotalMilliseconds, (int)scrapIntervalMax.TotalMilliseconds);
                var delay = scrapTimeout - (int)scrapingWatch.ElapsedMilliseconds;
                await WaitFor(delay, token);

                // End the scraping if it was requested
                if (token.IsCancellationRequested)
                    return;
            }
        }
    }

    public static async Task DeleteOldData(IHost host)
    {
        var config = host.Services.GetRequiredService<IConfiguration>();
        var offersMaxAge = config.GetRequiredValue<string>("DataRetentionRules:DeleteTradeOffersAfter").ToTimeSpan();
        var notificationsMaxAge = GetNotificationsMaxAge(config);

        using (var scope = host.Services.CreateScope())
        {
            var retentionApp = scope.ServiceProvider.GetRequiredService<DataRetentionApplication>();

            await retentionApp.DeleteOldTradeOffers(offersMaxAge);

            // Notifications retention policy rule is optional. If it's not set, we don't clean notifications.
            if (notificationsMaxAge != null)
                await retentionApp.DeleteOldNotifications(notificationsMaxAge.Value);
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

    private static TimeSpan? GetNotificationsMaxAge(IConfiguration config)
    {
        var maxAgeValue = config.GetValue<string>("DataRetentionRules:DeleteNotificationsAfter", "");

        if (string.IsNullOrEmpty(maxAgeValue))
            return null;

        return maxAgeValue.ToTimeSpan();
    }
}