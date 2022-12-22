using RocketLeagueTradingTools.Core.Application.DataRetention;
using RocketLeagueTradingTools.Core.Application.Scrap;

namespace RocketLeagueTradingTools.Scraper;

internal static class Jobs
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
        using var scope = host.Services.CreateScope();
        var retentionApp = scope.ServiceProvider.GetRequiredService<DataRetentionApplication>();

        await retentionApp.DeleteOldData();
    }
}