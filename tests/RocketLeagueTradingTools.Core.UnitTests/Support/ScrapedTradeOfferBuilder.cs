using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Core.UnitTests.Support;

internal class ScrapedTradeOfferBuilder
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
        if (tradeOfferBuilder is null)
            throw new ArgumentException("The field is required.", nameof(tradeOfferBuilder));

        return new ScrapedTradeOffer(tradeOfferBuilder.Build(), scrapedDate);
    }
}