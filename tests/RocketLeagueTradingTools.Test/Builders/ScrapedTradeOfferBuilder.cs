using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Test.Builders;

public class ScrapedTradeOfferBuilder
{
    private DateTime scrapedDate;
    private TradeOfferBuilder tradeOfferBuilder = null!;

    public ScrapedTradeOfferBuilder WithOffer(TradeOfferBuilder tradeOfferBuilder)
    {
        this.tradeOfferBuilder = tradeOfferBuilder;

        return this;
    }

    public ScrapedTradeOfferBuilder WithScrapedDate(DateTime scrapedDate)
    {
        this.scrapedDate = scrapedDate;

        return this;
    }

    public ScrapedTradeOffer Build()
    {
        ArgumentNullException.ThrowIfNull(tradeOfferBuilder);

        return new ScrapedTradeOffer(tradeOfferBuilder.Build(), scrapedDate);
    }
}
