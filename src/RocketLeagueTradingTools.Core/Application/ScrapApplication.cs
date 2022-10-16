using Microsoft.Extensions.Logging;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Application.Contracts;
using RocketLeagueTradingTools.Core.Domain.Exceptions;

namespace RocketLeagueTradingTools.Core.Application;

public class ScrapApplication
{
    private readonly ITradeOfferRepository tradeOfferRepository;
    private readonly IPersistenceRepository persistenceRepository;
    private readonly ILog log;
    private readonly IConfiguration config;
    private static int numberOfFailedTries;
    private static IList<TradeOffer> previousScrapBuyOffers = new List<TradeOffer>();
    private static IList<TradeOffer> previousScrapSellOffers = new List<TradeOffer>();

    public ScrapApplication(
        ITradeOfferRepository tradeOfferRepository,
        IPersistenceRepository persistenceRepository,
        ILog log,
        IConfiguration config)
    {
        this.tradeOfferRepository = tradeOfferRepository;
        this.persistenceRepository = persistenceRepository;
        this.log = log;
        this.config = config;
    }

    public async Task ScrapPageAsync(CancellationToken cancellationToken)
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
            await persistenceRepository.AddBuyOffers(newBuyOffers);
            await persistenceRepository.AddSellOffers(newSellOffers);

            if (IsNotFirstRun() && !HasOverlapWithPreviousScrap(latestOffers, newBuyOffers, newSellOffers))
                log.Warn("No offers overlap between scraps. Scraping interval can be decreased.");

            previousScrapBuyOffers = latestOffers.BuyOffers;
            previousScrapSellOffers = latestOffers.SellOffers;
            numberOfFailedTries = 0;
        }
        catch (TradingDataSourceIsDownException ex)
        {
            numberOfFailedTries++;
            log.Error($"'{ex.DataSourceName}' trading website is down. Retry attempt {numberOfFailedTries} of {config.ScrapRetryMaxAttempts}.");
        }
        catch (HttpRequestException)
        {
            numberOfFailedTries++;
            log.Error($"Connection issue has occurred. Retry attempt {numberOfFailedTries} of {config.ScrapRetryMaxAttempts}.");
        }

        if (numberOfFailedTries == config.ScrapRetryMaxAttempts)
            throw new PageScrapFailedAfterNumberOfRetriesException();
    }

    private bool IsNotFirstRun()
    {
        if (previousScrapBuyOffers.Count != 0)
            return true;

        if (previousScrapSellOffers.Count != 0)
            return true;

        return false;
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