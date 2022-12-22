using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;

namespace RocketLeagueTradingTools.Test.Builders;

public class TradeOfferBuilder
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
    
    public TradeOfferBuilder WithTrader(TraderBuilder traderBuilder)
    {
        this.trader = traderBuilder.Build();

        return this;
    }
    
    public TradeOfferBuilder WithTrader(Trader trader)
    {
        this.trader = trader;

        return this;
    }

    public TradeOffer Build()
    {
        ArgumentNullException.ThrowIfNull(tradeItemBuilder);
        ArgumentNullException.ThrowIfNull(trader);

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