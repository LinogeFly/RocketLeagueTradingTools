using RocketLeagueTradingTools.Infrastructure.Common;
using RocketLeagueTradingTools.Common;
using RocketLeagueTradingTools.Core.Application;
using RocketLeagueTradingTools.Core.Application.Scraping;

class Jobs
{
    public static void ExitKeyPressWaiting(CancellationTokenSource tokenSource)
    {
        while (Console.ReadKey(true).Key != ConsoleKey.Q)
        {
        }

        tokenSource.Cancel();
    }

    public static async Task InfiniteScrap(IHost host, CancellationToken token)
    {
        using var scope = host.Services.CreateScope();
        var scraper = scope.ServiceProvider.GetRequiredService<ScrapApplication>();

        await scraper.InfiniteScrap(token);
    }

    public static async Task DeleteOldData(IHost host)
    {
        var config = host.Services.GetRequiredService<IConfiguration>();
        var offersMaxAge = config.GetRequiredValue<string>("DataRetentionRules:DeleteTradeOffersAfter").ToTimeSpan();
        var notificationsMaxAge = GetNotificationsMaxAge(config);
        using var scope = host.Services.CreateScope();
        var retentionApp = scope.ServiceProvider.GetRequiredService<DataRetentionApplication>();

        await retentionApp.DeleteOldTradeOffers(offersMaxAge);

        // Notifications retention policy rule is optional. If it's not set, we don't clean notifications.
        if (notificationsMaxAge != null)
            await retentionApp.DeleteOldNotifications(notificationsMaxAge.Value);
    }

    private static TimeSpan? GetNotificationsMaxAge(IConfiguration config)
    {
        var maxAgeValue = config.GetValue("DataRetentionRules:DeleteNotificationsAfter", "");

        if (maxAgeValue.IsEmpty())
            return null;

        return maxAgeValue.ToTimeSpan();
    }
}