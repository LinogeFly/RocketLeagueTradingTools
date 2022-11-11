using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Scraper;

using var host = Host
    .CreateDefaultBuilder(args)
    .Configure();

var log = host.Services.GetRequiredService<ILog>();
var tokenSource = new CancellationTokenSource();
var token = tokenSource.Token;

// Waiting for exit button keypress in a separate background thread. Once the exit button is pressed,
// the thread will initiate the cancellation request to the cancellation token.
// The thread is a background thread and will be terminated automatically once the main thread finishes.
var waitForExitKeyPressTask = Task.Run(() =>
{
    // ReSharper disable once AccessToDisposedClosure
    Jobs.ExitKeyPressWaiting(tokenSource);
});

try
{
    Console.Write("Cleaning old data...");
    await Jobs.DeleteOldData(host);
    Console.WriteLine("done.");

    Console.WriteLine("Scraping has started. Press Q to stop.");
    await Jobs.InfiniteScrap(host, token);
}
catch (Exception ex)
{
    log.Error(ex.ToString());
}
finally
{
    tokenSource.Dispose();
}
