using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Exceptions;
using System.Diagnostics;
using RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;

namespace RocketLeagueTradingTools.Core.Application.Scrap;

public class ScrapApplication
{
    private readonly ITradeOfferRepository tradeOfferRepository;
    private readonly ITradeOfferPersistenceRepository tradeOfferPersistence;
    private readonly ILog log;
    private readonly IScrapApplicationSettings config;
    private int numberOfFailedTries;
    private IList<ScrapedTradeOffer> previousScrapTradeOffers = new List<ScrapedTradeOffer>();

    public ScrapApplication(
        ITradeOfferRepository tradeOfferRepository,
        ITradeOfferPersistenceRepository tradeOfferPersistence,
        ILog log,
        IScrapApplicationSettings config)
    {
        this.tradeOfferRepository = tradeOfferRepository;
        this.tradeOfferPersistence = tradeOfferPersistence;
        this.log = log;
        this.config = config;

        if (config.RetryMaxAttempts < 0)
            throw new InvalidOperationException($"{nameof(config.RetryMaxAttempts)} value has to be more than 0.");
    }

    public async Task InfiniteScrap(CancellationToken cancellationToken)
    {
        while (true)
        {
            var scrapingWatch = Stopwatch.StartNew();

            try
            {
                await ScrapPage(scrapingWatch, cancellationToken);
            }
            catch (PageScrapFailedAfterNumberOfTriesException)
            {
                // End the scraping if it failed after number of retries
                return;
            }
            finally
            {
                scrapingWatch.Stop();
            }

            // Have a delay before scraping again, or retrying if the last scrap failed
            if (LastRunDidNotFail())
                await DelayBeforeNextScrap(scrapingWatch.Elapsed, cancellationToken);
            else
                await DelayBeforeNextRetry(cancellationToken);

            // End the scraping if it was requested
            if (cancellationToken.IsCancellationRequested)
                return;
        }
    }

    private async Task ScrapPage(Stopwatch scrapingWatch, CancellationToken cancellationToken)
    {
        try
        {
            log.Trace("Downloading of trade offers started.");
            var latestOffers = await tradeOfferRepository.GetTradeOffersPage(cancellationToken);
            log.Trace("Downloading of trade offers finished.");

            // Ignore trade offers that were added in the previous scrap
            var newOffers = latestOffers
                .ExceptBy(previousScrapTradeOffers.Select(p => p.TradeOffer), l => l.TradeOffer)
                .ToList();

            // Not passing the cancellation token here as we don't want to terminate the persistence operation.
            // We want only to terminate the scraping when requested.
            await tradeOfferPersistence.AddTradeOffers(newOffers);

            if (IsNotFirstRun() && LastRunDidNotFail() && newOffers.Count == latestOffers.Count)
                log.Warn("No offers overlap between scraps. Scraping interval can be decreased.");

            numberOfFailedTries = 0;
            previousScrapTradeOffers = latestOffers;

            log.Info($"Scraped {newOffers.Count} new offers in {scrapingWatch.ElapsedMilliseconds} ms.");
        }
        catch (TradingDataServiceIsNotAvailableException ex)
        {
            numberOfFailedTries++;
            LogScrapPageError($"'{ex.DataServiceName}' website is not available.");
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            numberOfFailedTries++;
            LogScrapPageError("Scraping has timed out.");
        }
        catch (HttpRequestException)
        {
            numberOfFailedTries++;
            LogScrapPageError("Connectivity issue has occurred.");
        }

        if (numberOfFailedTries > config.RetryMaxAttempts)
            throw new PageScrapFailedAfterNumberOfTriesException();
    }

    private void LogScrapPageError(string message)
    {
        var trimmedMessage = message.TrimEnd('.');

        if (config.RetryMaxAttempts == 0)
            log.Error(message);
        else
            log.Error($"{trimmedMessage}. Attempt {numberOfFailedTries} of {config.RetryMaxAttempts + 1}.");
    }

    private async Task DelayBeforeNextScrap(TimeSpan lastScrapDuration, CancellationToken cancellationToken)
    {
        var delayMin = (int)config.DelayMin.TotalMilliseconds;
        var delayMax = (int)config.DelayMax.TotalMilliseconds;

        if (delayMin == 0 || delayMax == 0)
            return;

        var delay = (new Random().Next(delayMin, delayMax)) - lastScrapDuration.TotalMilliseconds;

        if (delay <= 0)
            return;

        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            if (!cancellationToken.IsCancellationRequested)
                throw;
        }
    }

    private async Task DelayBeforeNextRetry(CancellationToken cancellationToken)
    {
        // The interval between retries grows exponentially. 
        // https://exponentialbackoffcalculator.com website can be useful for debugging/testing,
        // as it helps to calculate timeout intervals.
        var interval = (int)config.RetryInterval.TotalMilliseconds;
        var delay = interval * Math.Pow(config.RetryBackoffRate, numberOfFailedTries - 1);

        if (delay <= 0)
            return;

        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            if (!cancellationToken.IsCancellationRequested)
                throw;
        }
    }

    private bool IsNotFirstRun()
    {
        return previousScrapTradeOffers.Count != 0;
    }

    private bool LastRunDidNotFail()
    {
        return numberOfFailedTries == 0;
    }
}