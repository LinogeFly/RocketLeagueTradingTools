using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.Enumerations;

namespace RocketLeagueTradingTools.Core.UnitTests.Support;

class TradeOfferBuilder
{
    private TradeItem tradeItem = null!;
    private int price;
    private DateTime scrapedDate;
    private string link = "";
    private TradingSite tradingSite;

    public TradeOfferBuilder WithTradeItem(TradeItem tradeItem)
    {
        this.tradeItem = tradeItem;
        this.scrapedDate = DateTime.UtcNow;

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
        this.link = $"https://rocket-league.com/trade/{sourceId}";
        this.tradingSite = TradingSite.RocketLeagueGarage;

        return this;
    }

    public TradeOffer Build()
    {
        return new TradeOffer
        (
            tradeItem,
            price,
            scrapedDate,
            link,
            tradingSite,
            nameof(TradeOfferBuilder)
        );
    }
}