using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Application.Contracts;
using RocketLeagueTradingTools.Core.Domain.Exceptions;
using RocketLeagueTradingTools.Application.Common;
using System.Diagnostics;

namespace RocketLeagueTradingTools.Core.Application;

public class ScrapApplication
{
    private readonly ITradeOfferRepository tradeOfferRepository;
    private readonly IPersistenceRepository persistence;
    private readonly SessionStorage.ScrapApplication session;
    private readonly ILog log;
    private readonly IConfiguration config;

    public ScrapApplication(
        ITradeOfferRepository tradeOfferRepository,
        IPersistenceRepository persistence,
        SessionStorage.ScrapApplication session,
        ILog log,
        IConfiguration config)
    {
        this.tradeOfferRepository = tradeOfferRepository;
        this.persistence = persistence;
        this.log = log;
        this.config = config;
        this.session = session;
    }

    public async Task ScrapPageAsync(CancellationToken cancellationToken)
    {
        var scrapingWatch = Stopwatch.StartNew();

        try
        {
            log.Trace("Downloading of trade offers started.");
            var latestOffers = await tradeOfferRepository.GetTradeOffersPage(cancellationToken);
            log.Trace("Downloading of trade offers finished.");

            // Ignore trade offers that were added in the previous scrap
            var newBuyOffers = latestOffers.BuyOffers.Except(session.PreviousScrapBuyOffers).ToList();
            var newSellOffers = latestOffers.SellOffers.Except(session.PreviousScrapSellOffers).ToList();

            // Not passing the cancellation token here as we don't want to terminate the persistence operation.
            // We want only to terminate the scraping when requested.
            await persistence.AddBuyOffers(newBuyOffers);
            await persistence.AddSellOffers(newSellOffers);

            if (IsNotFirstRun() && LastRunDidNotFail() && !HasOverlapWithPreviousScrap(latestOffers, newBuyOffers, newSellOffers))
                log.Warn("No offers overlap between scraps. Scraping interval can be decreased.");

            session.NumberOfFailedTries = 0;

            session.PreviousScrapBuyOffers = latestOffers.BuyOffers;
            session.PreviousScrapSellOffers = latestOffers.SellOffers;

            log.Info($"Scraped {newBuyOffers.Count + newSellOffers.Count} new offers in {scrapingWatch.ElapsedMilliseconds} ms.");
        }
        catch (TradingDataServiceIsNotAvailableException ex)
        {
            session.NumberOfFailedTries++;
            log.Error($"'{ex.DataServiceName}' website is not available. Retry attempt {session.NumberOfFailedTries} of {config.ScrapRetryMaxAttempts}.");
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            session.NumberOfFailedTries++;
            log.Error($"Scraping has timed out. Retry attempt {session.NumberOfFailedTries} of {config.ScrapRetryMaxAttempts}.");
        }
        catch (HttpRequestException)
        {
            session.NumberOfFailedTries++;
            log.Error($"Connectivity issue has occurred. Retry attempt {session.NumberOfFailedTries} of {config.ScrapRetryMaxAttempts}.");
        }
        finally
        {
            scrapingWatch.Stop();
        }

        if (session.NumberOfFailedTries == config.ScrapRetryMaxAttempts)
            throw new PageScrapFailedAfterNumberOfRetriesException();
    }

    private bool IsNotFirstRun()
    {
        if (session.PreviousScrapBuyOffers.Count != 0)
            return true;

        if (session.PreviousScrapSellOffers.Count != 0)
            return true;

        return false;
    }

    private bool LastRunDidNotFail()
    {
        return session.NumberOfFailedTries == 0;
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