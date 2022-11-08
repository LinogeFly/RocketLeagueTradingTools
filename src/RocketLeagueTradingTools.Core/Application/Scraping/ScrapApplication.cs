using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Exceptions;
using System.Diagnostics;

namespace RocketLeagueTradingTools.Core.Application.Scraping;

public class ScrapApplication
{
    private readonly ITradeOfferRepository tradeOfferRepository;
    private readonly IPersistenceRepository persistence;
    private readonly ILog log;
    private readonly IScrapApplicationSettings config;
    private int numberOfFailedTries;
    private IList<TradeOffer> previousScrapBuyOffers = new List<TradeOffer>();
    private IList<TradeOffer> previousScrapSellOffers = new List<TradeOffer>();

    public ScrapApplication(
        ITradeOfferRepository tradeOfferRepository,
        IPersistenceRepository persistence,
        ILog log,
        IScrapApplicationSettings config)
    {
        this.tradeOfferRepository = tradeOfferRepository;
        this.persistence = persistence;
        this.log = log;
        this.config = config;
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
            catch (PageScrapFailedAfterNumberOfRetriesException)
            {
                // End the scraping if it failed after number of retries (configured in the settings)
                return;
            }
            finally
            {
                scrapingWatch.Stop();
            }

            // Have a delay before scraping again
            await DelayBeforeNextScrap(scrapingWatch.Elapsed, cancellationToken);

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
            var newBuyOffers = latestOffers.BuyOffers.Except(previousScrapBuyOffers).ToList();
            var newSellOffers = latestOffers.SellOffers.Except(previousScrapSellOffers).ToList();

            // Not passing the cancellation token here as we don't want to terminate the persistence operation.
            // We want only to terminate the scraping when requested.
            await persistence.AddBuyOffers(newBuyOffers);
            await persistence.AddSellOffers(newSellOffers);

            if (IsNotFirstRun() && LastRunDidNotFail() && !HasOverlapWithPreviousScrap(latestOffers, newBuyOffers, newSellOffers))
                log.Warn("No offers overlap between scraps. Scraping interval can be decreased.");

            numberOfFailedTries = 0;

            previousScrapBuyOffers = latestOffers.BuyOffers;
            previousScrapSellOffers = latestOffers.SellOffers;

            log.Info($"Scraped {newBuyOffers.Count + newSellOffers.Count} new offers in {scrapingWatch.ElapsedMilliseconds} ms.");
        }
        catch (TradingDataServiceIsNotAvailableException ex)
        {
            numberOfFailedTries++;
            log.Error($"'{ex.DataServiceName}' website is not available. Retry attempt {numberOfFailedTries} of {config.ScrapRetryMaxAttempts}.");
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            numberOfFailedTries++;
            log.Error($"Scraping has timed out. Retry attempt {numberOfFailedTries} of {config.ScrapRetryMaxAttempts}.");
        }
        catch (HttpRequestException)
        {
            numberOfFailedTries++;
            log.Error($"Connectivity issue has occurred. Retry attempt {numberOfFailedTries} of {config.ScrapRetryMaxAttempts}.");
        }

        if (numberOfFailedTries == config.ScrapRetryMaxAttempts)
            throw new PageScrapFailedAfterNumberOfRetriesException();
    }

    private async Task DelayBeforeNextScrap(TimeSpan lastScrapDuration, CancellationToken cancellationToken)
    {
        var delayMin = (int)config.ScrapDelayMin.TotalMilliseconds;
        var delayMax = (int)config.ScrapDelayMax.TotalMilliseconds;

        if (delayMin == 0 || delayMax == 0)
            return;

        var scrapTimeout = new Random().Next(delayMin, delayMax);
        var delay = scrapTimeout - (int)lastScrapDuration.TotalMilliseconds;

        if (delay <= 0)
            return;

        try
        {
            await Task.Delay(delay, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            if (!cancellationToken.IsCancellationRequested)
                throw;
        }
    }

    private bool IsNotFirstRun()
    {
        if (previousScrapBuyOffers.Count != 0)
            return true;

        if (previousScrapSellOffers.Count != 0)
            return true;

        return false;
    }

    private bool LastRunDidNotFail()
    {
        return numberOfFailedTries == 0;
    }

    private bool HasOverlapWithPreviousScrap(TradeOffersPage latestOffers, List<TradeOffer> newBuyOffers, List<TradeOffer> newSellOffers)
    {
        if (newBuyOffers.Count < latestOffers.BuyOffers.Count)
            return true;

        if (newSellOffers.Count < latestOffers.SellOffers.Count)
            return true;

        return false;
    }
}