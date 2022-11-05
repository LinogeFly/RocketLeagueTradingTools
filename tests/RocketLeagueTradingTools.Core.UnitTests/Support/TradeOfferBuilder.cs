using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Core.UnitTests.Support;

class TradeOfferBuilder
{
    private TradeItem tradeItem = null!;
    private int price;
    private DateTime scrapedDate;
    private string sourceId = "";
    private string link = "";

    public TradeOfferBuilder WithTradeItem(TradeItem item)
    {
        this.tradeItem = item;

        return this;
    }

    public TradeOfferBuilder WithPrice(int price)
    {
        this.price = price;

        return this;
    }

    public TradeOfferBuilder WithScrapedDate(DateTime scrapedDate)
    {
        this.scrapedDate = scrapedDate;

        return this;
    }

    public TradeOfferBuilder WithRlgId(string sourceId)
    {
        this.sourceId = sourceId;
        this.link = $"https://rocket-league.com/trade/{sourceId}";

        return this;
    }

    public TradeOffer Build()
    {
        return new TradeOffer
        (
            tradeItem,
            price,
            scrapedDate,
            sourceId,
            link
        );
    }
}