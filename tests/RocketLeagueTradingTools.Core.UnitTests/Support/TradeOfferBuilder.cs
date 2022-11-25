using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;

namespace RocketLeagueTradingTools.Core.UnitTests.Support;

internal class TradeOfferBuilder
{
    private TradeOfferType offerType;
    private TradeItemBuilder tradeItemBuilder = null!;
    private int price;
    private string link = "";
    private Trader trader = null!;
    
    public TradeOfferBuilder WithType(TradeOfferType offerType)
    {
        this.offerType = offerType;

        return this;
    }

    public TradeOfferBuilder WithItem(TradeItemBuilder tradeItemBuilder)
    {
        this.tradeItemBuilder = tradeItemBuilder;

        return this;
    }

    public TradeOfferBuilder WithPrice(int price)
    {
        this.price = price;

        return this;
    }

    public TradeOfferBuilder WithLink(string link)
    {
        this.link = link;

        return this;
    }
    
    public TradeOfferBuilder WithTrader(TradingSite site, string name)
    {
        this.trader = new Trader(site, name);

        return this;
    }

    public TradeOffer Build()
    {
        if (tradeItemBuilder is null)
            throw new ArgumentException("The field is required.", nameof(tradeItemBuilder));
        
        return new TradeOffer
        (
            offerType,
            tradeItemBuilder.Build(),
            price,
            link,
            trader
        );
    }
}